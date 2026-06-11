using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public class GeminiCvParserService : ICvParserService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly LlmSettings _settings;
        private readonly ILogger<GeminiCvParserService> _logger;
        private readonly SkillMatcher _skillMatcher;
        private static readonly Random _rng = new();

        // Circuit breaker: track consecutive failures
        private static int _consecutiveFailures;
        private static DateTime _circuitOpenUntil = DateTime.MinValue;
        private const int FailureThreshold = 3;
        private const int CircuitOpenSeconds = 60;

        private const string SystemPrompt =
@"You are an expert HR CV parser. Extract structured data from a CV/resume and return ONLY a raw JSON object — no markdown fences, no commentary.

SCHEMA (return exactly this shape):
{
  ""jobTitle"": ""string — the candidate's primary job title or role (e.g. 'Frontend Developer', 'DevOps Engineer'). Return empty string if unclear."",
  ""yearsOfExperience"": 0,
  ""phoneNumber"": """",
  ""countryName"": """",
  ""cityName"": """",
  ""firstLanguage"": """",
  ""bio"": ""2-3 sentence professional summary"",
  ""experiences"": [
    {
      ""jobTitle"": """",
      ""companyName"": """",
      ""countryName"": """",
      ""cityName"": """",
      ""employmentType"": ""FullTime|PartTime|Contract|Freelance|Internship"",
      ""responsibilities"": ""comma-separated list, max 2000 chars"",
      ""startDate"": ""YYYY-MM-DD"",
      ""endDate"": ""YYYY-MM-DD or null"",
      ""isCurrent"": false
    }
  ],
  ""educations"": [
    {
      ""institution"": """",
      ""degree"": ""Bachelor|Master|PhD|Diploma|HighSchool|Associate|Other"",
      ""fieldOfStudy"": ""string — the raw field/major name from the CV"",
      ""gradeOrGpa"": ""null if not mentioned"",
      ""startDate"": ""YYYY-MM-DD"",
      ""endDate"": ""YYYY-MM-DD or null"",
      ""isCurrent"": false
    }
  ],
  ""projects"": [
    {
      ""title"": """",
      ""technologiesUsed"": ""comma-separated"",
      ""description"": """",
      ""projectLink"": """"
    }
  ],
  ""skills"": [""skill1"", ""skill2"", ...],
  ""socialAccounts"": {
    ""linkedIn"": """",
    ""github"": """",
    ""behance"": """",
    ""dribbble"": """",
    ""personalWebsite"": """"
  }
}

CRITICAL RULES:
1. Return ONLY the JSON object — no markdown, no explanation.
2. bio: Write a 2-3 sentence professional summary BASED ONLY ON THE ACTUAL CV CONTENT. Do NOT invent skills, technologies, or qualifications that are not explicitly mentioned in the CV.
3. skills: Extract ONLY specific technology/tool names EXACTLY as they appear in the CV (e.g. if the CV says 'JavaScript', extract 'JavaScript' — NOT 'Java'). Use the EXACT name from the CV text. Do NOT extract: soft skills, conceptual patterns (Clean Architecture, Repository Pattern, SOLID), phrases (.NET ecosystem, RESTful endpoints), version numbers (just 'ASP.NET Core' not 'ASP.NET Core 8'), or similar-sounding but DIFFERENT technologies (e.g. 'Java' ≠ 'JavaScript', 'FigJam' ≠ 'Figma'). If unsure whether a word is a technology mentioned in the CV, do NOT extract it. Max 15 skills.
4. phone: extract the full phone number with country code if present.
5. If a field is missing from the CV, use empty string (or null for dates).
6. employmentType: infer from context if not explicit.
7. fieldOfStudy: use the RAW name from the CV (e.g. 'Computer and Communication Engineering').
8. experiences: Extract ALL work experiences listed.";

        public GeminiCvParserService(
            HttpClient httpClient,
            AppDbContext context,
            IOptions<LlmSettings> settings,
            ILogger<GeminiCvParserService> logger,
            SkillMatcher skillMatcher)
        {
            _httpClient = httpClient;
            _context = context;
            _settings = settings.Value;
            _logger = logger;
            _skillMatcher = skillMatcher;
        }

        public async Task<ParsedResumeDataDto?> ParseResumeTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(_settings.GeminiApiKey) || _settings.GeminiApiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                _logger.LogWarning("Gemini API key is not configured. Skipping CV parsing.");
                return null;
            }

            // Circuit breaker: skip if too many recent failures
            if (DateTime.UtcNow < _circuitOpenUntil)
            {
                _logger.LogWarning("Gemini circuit breaker OPEN until {Until}. Skipping.", _circuitOpenUntil);
                return null;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Empty CV text provided.");
                return null;
            }

            try
            {
                var userPrompt = $"Extract structured data from this CV:\n\n{text}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = $"{SystemPrompt}\n\n{userPrompt}" } } }
                    },
                    generationConfig = new
                    {
                        temperature = 0.1,
                        topK = 1,
                        topP = 1,
                        responseMimeType = "application/json"
                    }
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.GeminiModel}:generateContent?key={_settings.GeminiApiKey}";

                const int maxAttempts = 5;
                HttpResponseMessage? response = null;
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    response = await _httpClient.PostAsJsonAsync(url, requestBody);

                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }

                    var statusCode = (int)response.StatusCode;
                    if (!IsTransientError(statusCode) || attempt == maxAttempts)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        _logger.LogError(
                            "Gemini API error after {Attempt}/{Max} attempts: {Status} - {Error}",
                            attempt, maxAttempts, response.StatusCode, err);
                        RecordFailure();
                        return null;
                    }

                    // Exponential backoff with jitter
                    var baseDelay = TimeSpan.FromSeconds(2 * Math.Pow(2, attempt - 1));
                    var jitter = TimeSpan.FromMilliseconds(_rng.Next(0, 1000));
                    var delay = baseDelay + jitter;

                    if (response.Headers.RetryAfter is { } retryAfter)
                    {
                        if (retryAfter.Delta.HasValue)
                            delay = retryAfter.Delta.Value;
                        else if (retryAfter.Date.HasValue)
                        {
                            var serverDelay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
                            if (serverDelay > TimeSpan.Zero) delay = serverDelay;
                        }
                    }

                    _logger.LogWarning(
                        "Gemini API transient error {Status} on attempt {Attempt}/{Max}. Retrying in {DelaySeconds:F1}s...",
                        response.StatusCode, attempt, maxAttempts, delay.TotalSeconds);

                    response.Dispose();
                    await Task.Delay(delay);
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    RecordFailure();
                    return null;
                }

                var jsonDoc = await response.Content.ReadFromJsonAsync<JsonElement>();
                var contentText = jsonDoc.GetProperty("candidates")[0]
                                      .GetProperty("content")
                                      .GetProperty("parts")[0]
                                      .GetProperty("text").GetString();

                if (string.IsNullOrWhiteSpace(contentText))
                {
                    RecordFailure();
                    return null;
                }

                _logger.LogInformation("Gemini raw response ({Len} chars): {Preview}",
                    contentText.Length, contentText.Length > 500 ? contentText[..500] + "..." : contentText);

                var parsed = JsonSerializer.Deserialize<GeminiExtractedData>(contentText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed == null)
                {
                    _logger.LogWarning("Gemini: deserialized to null.");
                    RecordFailure();
                    return null;
                }

                RecordSuccess();

                _logger.LogInformation("Gemini parsed: JobTitle='{JT}', YoE={YoE}, Phone='{Ph}', Country='{Co}', City='{Ci}', Lang='{La}', Bio='{Bio}', Exps={ExpCount}, Edus={EduCount}, Projs={ProjCount}, Skills={SkillCount}, Social={HasSocial}",
                    parsed.JobTitle, parsed.YearsOfExperience, parsed.PhoneNumber, parsed.CountryName, parsed.CityName, parsed.FirstLanguage,
                    parsed.Bio?.Length > 80 ? parsed.Bio[..80] + "..." : parsed.Bio,
                    parsed.Experiences?.Count ?? 0, parsed.Educations?.Count ?? 0, parsed.Projects?.Count ?? 0, parsed.Skills?.Count ?? 0,
                    parsed.SocialAccounts != null);

                // Map to DB IDs via fuzzy matching
                return await MapToDtoAsync(parsed, text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CV with Gemini.");
                RecordFailure();
                return null;
            }
        }

        private async Task<ParsedResumeDataDto> MapToDtoAsync(GeminiExtractedData parsed, string cvText)
        {
            var result = new ParsedResumeDataDto
            {
                YearsOfExperience = parsed.YearsOfExperience,
                PhoneNumber = parsed.PhoneNumber,
                Bio = parsed.Bio
            };

            var refCountries = await _context.Countries.ToListAsync();
            var refLanguages = await _context.Languages.ToListAsync();
            var refJobTitles = await _context.JobTitles.Where(j => j.IsActive).ToListAsync();
            var refCities = await _context.Cities.ToListAsync();
            var refFieldsOfStudy = await _context.FieldsOfStudy.Where(f => f.IsActive).ToListAsync();

            // Job Title — fuzzy match from extracted text
            if (!string.IsNullOrWhiteSpace(parsed.JobTitle))
            {
                var jt = FuzzyMatchHelper.FindBestMatch(parsed.JobTitle, refJobTitles, x => x.TitleEn, maxDistance: 4);
                if (jt != null) { result.JobTitleId = jt.Id; result.JobTitleName = jt.TitleEn; }
            }

            // Country
            if (!string.IsNullOrWhiteSpace(parsed.CountryName))
            {
                var c = FuzzyMatchHelper.FindBestMatch(parsed.CountryName, refCountries, x => x.NameEn, maxDistance: 4);
                if (c != null) { result.CountryId = c.Id; result.CountryName = c.NameEn; }
            }

            // City
            if (!string.IsNullOrWhiteSpace(parsed.CityName))
            {
                var cityCandidates = result.CountryId.HasValue
                    ? refCities.Where(x => x.CountryId == result.CountryId.Value)
                    : refCities;
                var c = FuzzyMatchHelper.FindBestMatch(parsed.CityName, cityCandidates, x => x.NameEn, maxDistance: 4);
                if (c != null) { result.CityId = c.Id; result.CityName = c.NameEn; }
            }

            // Language
            if (!string.IsNullOrWhiteSpace(parsed.FirstLanguage))
            {
                var l = FuzzyMatchHelper.FindBestMatch(parsed.FirstLanguage, refLanguages, x => x.NameEn, maxDistance: 3);
                if (l != null) { result.FirstLanguageId = l.Id; result.FirstLanguageName = l.NameEn; }
            }

            // Experiences
            if (parsed.Experiences != null)
            {
                foreach (var exp in parsed.Experiences)
                {
                    var mappedExp = new ParsedExperienceDto
                    {
                        JobTitle = exp.JobTitle,
                        CompanyName = exp.CompanyName,
                        CountryName = exp.CountryName,
                        CityName = exp.CityName,
                        Responsibilities = exp.Responsibilities,
                        IsCurrent = exp.IsCurrent ?? false
                    };

                    if (Enum.TryParse<RecruitmentPlatformAPI.Enums.EmploymentType>(exp.EmploymentType, true, out var empType))
                        mappedExp.EmploymentType = empType;
                    else
                        mappedExp.EmploymentType = RecruitmentPlatformAPI.Enums.EmploymentType.FullTime;

                    if (DateTime.TryParse(exp.StartDate, out var sDate)) mappedExp.StartDate = sDate;
                    if (DateTime.TryParse(exp.EndDate, out var eDate)) mappedExp.EndDate = eDate;

                    if (!string.IsNullOrWhiteSpace(exp.CountryName))
                    {
                        var c = FuzzyMatchHelper.FindBestMatch(exp.CountryName, refCountries, x => x.NameEn, maxDistance: 4);
                        if (c != null) { mappedExp.CountryId = c.Id; mappedExp.CountryName = c.NameEn; }
                    }

                    if (!string.IsNullOrWhiteSpace(exp.CityName))
                    {
                        var cityCandidates = mappedExp.CountryId.HasValue
                            ? refCities.Where(x => x.CountryId == mappedExp.CountryId.Value)
                            : refCities;
                        var c = FuzzyMatchHelper.FindBestMatch(exp.CityName, cityCandidates, x => x.NameEn, maxDistance: 4);
                        if (c != null) { mappedExp.CityId = c.Id; mappedExp.CityName = c.NameEn; }
                    }

                    result.Experiences.Add(mappedExp);
                }
            }

            // Educations
            if (parsed.Educations != null)
            {
                foreach (var edu in parsed.Educations)
                {
                    var mappedEdu = new ParsedEducationDto
                    {
                        Institution = edu.Institution,
                        Degree = edu.Degree,
                        GradeOrGpa = edu.GradeOrGpa,
                        IsCurrent = edu.IsCurrent ?? false
                    };

                    if (!string.IsNullOrWhiteSpace(edu.FieldOfStudy))
                    {
                        var fos = FuzzyMatchHelper.FindBestMatch(edu.FieldOfStudy, refFieldsOfStudy, x => x.NameEn, maxDistance: 4);
                        if (fos != null)
                        {
                            mappedEdu.FieldOfStudyId = fos.Id;
                            mappedEdu.FieldOfStudyName = fos.NameEn;
                        }
                        else
                        {
                            mappedEdu.FieldOfStudyId = null;
                            mappedEdu.FieldOfStudyName = edu.FieldOfStudy.Trim();
                        }
                    }

                    if (DateTime.TryParse(edu.StartDate, out var sDate)) mappedEdu.StartDate = sDate;
                    if (DateTime.TryParse(edu.EndDate, out var eDate)) mappedEdu.EndDate = eDate;

                    result.Educations.Add(mappedEdu);
                }
            }

            // Projects
            if (parsed.Projects != null)
            {
                foreach (var proj in parsed.Projects)
                {
                    result.Projects.Add(new ParsedProjectDto
                    {
                        Title = proj.Title,
                        TechnologiesUsed = proj.TechnologiesUsed,
                        Description = proj.Description,
                        ProjectLink = proj.ProjectLink
                    });
                }
            }

            // Skills — validate against CV text, then match to DB IDs via SkillMatcher
            if (parsed.Skills != null && parsed.Skills.Count > 0)
            {
                var validated = ValidateSkillsAgainstCvText(parsed.Skills, cvText);
                _logger.LogInformation("Gemini extracted {Extracted} skills, {Validated} validated against CV text",
                    parsed.Skills.Count, validated.Count);
                result.SkillIds = await _skillMatcher.MatchSkillsAsync(validated);
                _logger.LogInformation("Gemini CV parsing: matched {Matched}/{Validated} skills to DB",
                    result.SkillIds.Count, validated.Count);
            }

            // Social Accounts
            if (parsed.SocialAccounts != null)
            {
                result.SocialAccounts = new ParsedSocialAccountDto
                {
                    LinkedIn = parsed.SocialAccounts.LinkedIn,
                    Github = parsed.SocialAccounts.Github,
                    Behance = parsed.SocialAccounts.Behance,
                    Dribbble = parsed.SocialAccounts.Dribbble,
                    PersonalWebsite = parsed.SocialAccounts.PersonalWebsite
                };
            }

            return result;
        }

        private static bool IsTransientError(int statusCode)
        {
            return statusCode == 429
                || (statusCode >= 500 && statusCode <= 504);
        }

        private void RecordFailure()
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= FailureThreshold)
            {
                _circuitOpenUntil = DateTime.UtcNow.AddSeconds(CircuitOpenSeconds);
                _logger.LogWarning("Gemini circuit breaker OPEN: {Failures} consecutive failures. Skipping for {Seconds}s.",
                    _consecutiveFailures, CircuitOpenSeconds);
            }
        }

        private void RecordSuccess()
        {
            _consecutiveFailures = 0;
            _circuitOpenUntil = DateTime.MinValue;
        }

        private List<string> ValidateSkillsAgainstCvText(List<string> skills, string cvText)
        {
            // Normalize: collapse lone newlines (PDF extracts "Java\nScript" from "JavaScript")
            var normalizedCv = System.Text.RegularExpressions.Regex.Replace(cvText, @"\r?\n(?!\r?\n)", " ");
            var cvLower = normalizedCv.ToLowerInvariant();
            var validated = new List<string>();
            var dropped = new List<string>();

            foreach (var skill in skills)
            {
                if (string.IsNullOrWhiteSpace(skill)) continue;

                var skillLower = skill.ToLowerInvariant().Trim();

                // Check 1: word-boundary match
                var pattern = @"\b" + System.Text.RegularExpressions.Regex.Escape(skillLower) + @"\b";
                if (System.Text.RegularExpressions.Regex.IsMatch(cvLower, pattern))
                {
                    validated.Add(skill.Trim());
                    continue;
                }

                // Check 2: substring match (for multi-word skills like "Tailwind CSS")
                if (cvLower.Contains(skillLower))
                {
                    // Guard: reject if skill is just a prefix of a longer word
                    // "java" in "javascript" → dropped; "react" in "react.js" → kept (followed by ".", not a letter)
                    var prefixPattern = @"\b" + System.Text.RegularExpressions.Regex.Escape(skillLower) + @"[a-z]";
                    if (System.Text.RegularExpressions.Regex.IsMatch(cvLower, prefixPattern))
                    {
                        dropped.Add(skill);
                        continue;
                    }
                    validated.Add(skill.Trim());
                    continue;
                }

                dropped.Add(skill);
            }

            if (dropped.Count > 0)
            {
                _logger.LogWarning("Skills dropped (not found in CV text): {Dropped}", string.Join(", ", dropped));
            }

            return validated;
        }

        private class GeminiExtractedData
        {
            public string? JobTitle { get; set; }
            public int? YearsOfExperience { get; set; }
            public string? PhoneNumber { get; set; }
            public string? CountryName { get; set; }
            public string? CityName { get; set; }
            public string? FirstLanguage { get; set; }
            public string? Bio { get; set; }
            public List<GeminiExperience>? Experiences { get; set; }
            public List<GeminiEducation>? Educations { get; set; }
            public List<GeminiProject>? Projects { get; set; }
            public List<string>? Skills { get; set; }
            public GeminiSocialAccounts? SocialAccounts { get; set; }
        }

        private class GeminiExperience
        {
            public string? JobTitle { get; set; }
            public string? CompanyName { get; set; }
            public string? CountryName { get; set; }
            public string? CityName { get; set; }
            public string? EmploymentType { get; set; }
            public string? Responsibilities { get; set; }
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }
            public bool? IsCurrent { get; set; }
        }

        private class GeminiEducation
        {
            public string? Institution { get; set; }
            public string? Degree { get; set; }
            public string? FieldOfStudy { get; set; }
            public string? GradeOrGpa { get; set; }
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }
            public bool? IsCurrent { get; set; }
        }

        private class GeminiProject
        {
            public string? Title { get; set; }
            public string? TechnologiesUsed { get; set; }
            public string? Description { get; set; }
            public string? ProjectLink { get; set; }
        }

        private class GeminiSocialAccounts
        {
            public string? LinkedIn { get; set; }
            public string? Github { get; set; }
            public string? Behance { get; set; }
            public string? Dribbble { get; set; }
            public string? PersonalWebsite { get; set; }
        }
    }
}
