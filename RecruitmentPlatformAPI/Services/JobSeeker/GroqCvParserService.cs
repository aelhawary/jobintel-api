using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public class GroqCvParserService : ICvParserService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly LlmSettings _settings;
        private readonly ILogger<GroqCvParserService> _logger;
        private readonly SkillMatcher _skillMatcher;
        private readonly CvTextSkillValidator _skillValidator;
        private static readonly Random _rng = new();
        private static readonly SemaphoreSlim _circuitLock = new(1, 1);

        // Circuit breaker (thread-safe via SemaphoreSlim)
        private static int _consecutiveFailures;
        private static DateTime _circuitOpenUntil = DateTime.MinValue;
        private const int FailureThreshold = 3;
        private const int CircuitOpenSeconds = 60;

        // Maximum characters to send to the LLM (prevents context overflow)
        private const int MaxCvTextLength = 15000;

        private const string SystemPrompt =
@"You are an expert HR CV parser. Extract structured data from a CV/resume.

Return valid JSON wrapped in ```json``` code fences. Example:

```json
{""jobTitle"": ""Frontend Developer"", ""yearsOfExperience"": 3, ""phoneNumber"": ""+201234567890"", ""countryName"": ""Egypt"", ""cityName"": ""Cairo"", ""firstLanguage"": ""Arabic"", ""bio"": ""3 years of frontend experience."", ""experiences"": [{""jobTitle"": ""Frontend Developer"", ""companyName"": ""Acme"", ""countryName"": ""Egypt"", ""cityName"": ""Cairo"", ""employmentType"": ""FullTime"", ""responsibilities"": ""Built UI components"", ""startDate"": ""2020-01-01"", ""endDate"": null, ""isCurrent"": true}], ""educations"": [{""institution"": ""Cairo University"", ""degree"": ""Bachelor"", ""fieldOfStudy"": ""Computer Science"", ""gradeOrGpa"": null, ""startDate"": ""2015-09-01"", ""endDate"": ""2019-06-01"", ""isCurrent"": false}], ""projects"": [{""title"": ""My App"", ""technologiesUsed"": ""React, Node.js"", ""description"": ""A web app"", ""projectLink"": ""https://example.com""}], ""skills"": [""React"", ""JavaScript""], ""socialAccounts"": {""linkedIn"": ""https://linkedin.com/in/user"", ""github"": ""https://github.com/user"", ""behance"": """", ""dribbble"": """", ""personalWebsite"": """"}}
```

FIELDS:
- jobTitle: string — The candidate's primary standard role. MUST map to one of these exact strings if possible: 'Backend Developer', 'Frontend Developer', 'Full Stack Developer', 'Mobile Developer', 'Data Scientist', 'DevOps Engineer', 'QA Engineer', 'UI/UX Designer'. If no exact match fits, use their exact title.
- yearsOfExperience: total years
- phoneNumber: with country code
- countryName: from CV
- cityName: from CV
- firstLanguage: if mentioned
- bio: 2-3 sentence summary BASED ONLY ON CV CONTENT
- experiences: ALL work experiences listed, with comma-separated responsibilities (max 2000 chars)
- educations: ALL entries with raw fieldOfStudy text
- projects: ALL with comma-separated technologiesUsed
- skills: Max 25. EXACT technology/tool names (C#, React, Docker). Do NOT extract: soft skills, conceptual patterns (Clean Architecture, Repository Pattern, SOLID), or phrases (.NET ecosystem, RESTful endpoints).
- socialAccounts: URLs or empty strings

CRITICAL RULES:
1. Wrap the JSON in ```json``` fences. Do NOT put anything outside the fences.
2. If a field is missing, use empty string (or null for dates).
3. employmentType: infer from context (FullTime/PartTime/Contract/Freelance/Internship).
4. Bio must reflect ACTUAL CV content — do NOT invent qualifications.
5. Extract the full phone with country code if present.";

        public GroqCvParserService(
            HttpClient httpClient,
            AppDbContext context,
            IOptions<LlmSettings> settings,
            ILogger<GroqCvParserService> logger,
            SkillMatcher skillMatcher,
            CvTextSkillValidator skillValidator)
        {
            _httpClient = httpClient;
            _context = context;
            _settings = settings.Value;
            _logger = logger;
            _skillMatcher = skillMatcher;
            _skillValidator = skillValidator;
        }

        public async Task<ParsedResumeDataDto?> ParseResumeTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey) || _settings.ApiKey == "YOUR_GROQ_API_KEY")
            {
                _logger.LogWarning("Groq API key is not configured. Skipping CV parsing.");
                return null;
            }

            if (DateTime.UtcNow < _circuitOpenUntil)
            {
                _logger.LogWarning("Groq circuit breaker OPEN until {Until}. Skipping.", _circuitOpenUntil);
                return null;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Empty CV text provided.");
                return null;
            }

            // Truncate very long CV text to prevent context overflow
            var rawCvText = text;
            if (text.Length > MaxCvTextLength)
            {
                _logger.LogWarning("CV text truncated from {Original} to {Truncated} chars for LLM processing.",
                    text.Length, MaxCvTextLength);
                text = text[..MaxCvTextLength];
            }

            var userPrompt = $"Extract structured data from this CV:\n\n{text}";

            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.1,
                max_tokens = 8000
            };

            var url = $"{_settings.BaseUrl.TrimEnd('/')}/chat/completions";

            GroqResponseEnvelope? groqResp = null;
            for (int attempt = 1; attempt <= _settings.MaxRetries; attempt++)
            {
                try
                {
                    using var httpReq = new HttpRequestMessage(HttpMethod.Post, url);
                    httpReq.Content = JsonContent.Create(requestBody);
                    httpReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

                    var response = await _httpClient.SendAsync(httpReq);

                    if (response.IsSuccessStatusCode)
                    {
                        groqResp = await response.Content.ReadFromJsonAsync<GroqResponseEnvelope>();
                        break;
                    }

                    var err = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        int delay = ParseRetryAfter(response) ?? (_settings.RetryDelayMs * attempt * 2);
                        _logger.LogWarning("CV parser rate limited (attempt {A}/{Max}). Waiting {Delay}ms.",
                            attempt, _settings.MaxRetries, delay);
                        await Task.Delay(delay);
                        continue;
                    }

                    if (IsTransientError((int)response.StatusCode) && attempt < _settings.MaxRetries)
                    {
                        var baseDelay = TimeSpan.FromMilliseconds(_settings.RetryDelayMs * attempt);
                        var jitter = TimeSpan.FromMilliseconds(_rng.Next(0, 500));
                        _logger.LogWarning("CV parser transient error {Status} (attempt {A}/{Max}). Retrying in {Delay}ms...",
                            response.StatusCode, attempt, _settings.MaxRetries, (baseDelay + jitter).TotalMilliseconds);
                        await Task.Delay(baseDelay + jitter);
                        continue;
                    }

                    _logger.LogError("CV parser API error {Status}: {Error}", response.StatusCode,
                        err.Length > 300 ? err[..300] : err);
                    RecordFailure();
                    return null;
                }
                catch (TaskCanceledException) when (attempt < _settings.MaxRetries)
                {
                    _logger.LogWarning("CV parser timeout (attempt {A}/{Max}). Retrying...", attempt, _settings.MaxRetries);
                    await Task.Delay(_settings.RetryDelayMs * attempt);
                }
                catch (HttpRequestException ex) when (attempt < _settings.MaxRetries)
                {
                    _logger.LogWarning(ex, "CV parser HTTP error (attempt {A}/{Max}). Retrying...", attempt, _settings.MaxRetries);
                    await Task.Delay(_settings.RetryDelayMs * attempt);
                }
            }

            string? raw = groqResp?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(raw))
            {
                _logger.LogWarning("CV parser: empty response from Groq after {Max} attempts.", _settings.MaxRetries);
                RecordFailure();
                return null;
            }

            _logger.LogInformation("Groq raw response ({Len} chars): {Preview}", raw.Length, raw.Length > 500 ? raw[..500] + "..." : raw);

            // Extract JSON from markdown code blocks if present
            var jsonText = ExtractJsonFromMarkdown(raw);

            GroqExtractedData? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<GroqExtractedData>(jsonText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "CV parser: failed to deserialize Groq response ({Len} chars).", raw.Length);
                RecordFailure();
                return null;
            }

            if (parsed == null)
            {
                _logger.LogWarning("CV parser: deserialized to null.");
                RecordFailure();
                return null;
            }

            RecordSuccess();

            _logger.LogInformation("Groq parsed: JobTitle='{JT}', YoE={YoE}, Phone='{Ph}', Country='{Co}', City='{Ci}', Lang='{La}', Bio='{Bio}', Exps={ExpCount}, Edus={EduCount}, Projs={ProjCount}, Skills={SkillCount}, Social={HasSocial}",
                parsed.JobTitle, parsed.YearsOfExperience, parsed.PhoneNumber, parsed.CountryName, parsed.CityName, parsed.FirstLanguage,
                parsed.Bio?.Length > 80 ? parsed.Bio[..80] + "..." : parsed.Bio,
                parsed.Experiences?.Count ?? 0, parsed.Educations?.Count ?? 0, parsed.Projects?.Count ?? 0, parsed.Skills?.Count ?? 0,
                parsed.SocialAccounts != null);

            return await MapToDtoAsync(parsed, rawCvText);
        }

        private async Task<ParsedResumeDataDto> MapToDtoAsync(GroqExtractedData parsed, string rawCvText)
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

            if (!string.IsNullOrWhiteSpace(parsed.JobTitle))
            {
                var jt = FuzzyMatchHelper.FindBestMatch(parsed.JobTitle, refJobTitles, x => x.TitleEn, maxDistance: 4);
                if (jt != null) { result.JobTitleId = jt.Id; result.JobTitleName = jt.TitleEn; }
            }

            if (!string.IsNullOrWhiteSpace(parsed.CountryName))
            {
                var c = FuzzyMatchHelper.FindBestMatch(parsed.CountryName, refCountries, x => x.NameEn, maxDistance: 4);
                if (c != null) { result.CountryId = c.Id; result.CountryName = c.NameEn; }
            }

            if (!string.IsNullOrWhiteSpace(parsed.CityName))
            {
                var cityCandidates = result.CountryId.HasValue
                    ? refCities.Where(x => x.CountryId == result.CountryId.Value)
                    : refCities;
                var c = FuzzyMatchHelper.FindBestMatch(parsed.CityName, cityCandidates, x => x.NameEn, maxDistance: 4);
                if (c != null) { result.CityId = c.Id; result.CityName = c.NameEn; }
            }

            if (!string.IsNullOrWhiteSpace(parsed.FirstLanguage))
            {
                var l = FuzzyMatchHelper.FindBestMatch(parsed.FirstLanguage, refLanguages, x => x.NameEn, maxDistance: 3);
                if (l != null) { result.FirstLanguageId = l.Id; result.FirstLanguageName = l.NameEn; }
            }

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

            if (parsed.Skills != null && parsed.Skills.Count > 0)
            {
                _logger.LogInformation("Groq extracted {Count} skills: [{Skills}]",
                    parsed.Skills.Count, string.Join(", ", parsed.Skills));

                // Post-LLM validation: filter out hallucinated skills (same as Gemini)
                var validatedSkills = _skillValidator.ValidateSkills(parsed.Skills, rawCvText);

                _logger.LogInformation("After CV-text validation: {Valid}/{Extracted} skills remain",
                    validatedSkills.Count, parsed.Skills.Count);

                result.SkillIds = await _skillMatcher.MatchSkillsAsync(validatedSkills);
                _logger.LogInformation("Groq CV parsing: matched {Matched}/{Validated} validated skills to DB",
                    result.SkillIds.Count, validatedSkills.Count);
            }

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
            return statusCode == 429 || (statusCode >= 500 && statusCode <= 504);
        }

        private static int? ParseRetryAfter(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Retry-After", out var vals)
                && int.TryParse(vals.FirstOrDefault(), out var secs))
                return (secs + 1) * 1000;
            return null;
        }

        private void RecordFailure()
        {
            _circuitLock.Wait();
            try
            {
                _consecutiveFailures++;
                if (_consecutiveFailures >= FailureThreshold)
                {
                    _circuitOpenUntil = DateTime.UtcNow.AddSeconds(CircuitOpenSeconds);
                    _logger.LogWarning("Groq circuit breaker OPEN: {Failures} consecutive failures. Skipping for {Seconds}s.",
                        _consecutiveFailures, CircuitOpenSeconds);
                }
            }
            finally
            {
                _circuitLock.Release();
            }
        }

        private void RecordSuccess()
        {
            _circuitLock.Wait();
            try
            {
                _consecutiveFailures = 0;
                _circuitOpenUntil = DateTime.MinValue;
            }
            finally
            {
                _circuitLock.Release();
            }
        }

        private static string ExtractJsonFromMarkdown(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            // Check if wrapped in ```json ... ``` or ``` ... ```
            var trimmed = text.Trim();
            if (trimmed.StartsWith("```"))
            {
                var firstNewline = trimmed.IndexOf('\n');
                if (firstNewline > 0)
                {
                    var afterFirstLine = trimmed[(firstNewline + 1)..];
                    var lastFence = afterLastFence(afterFirstLine);
                    if (lastFence >= 0)
                        return afterFirstLine[..lastFence].Trim();
                }
            }

            return trimmed;

            static int afterLastFence(string s)
            {
                var idx = s.LastIndexOf("```");
                return idx >= 0 ? idx : -1;
            }
        }

        private class GroqResponseEnvelope
        {
            public List<GroqChoice>? Choices { get; set; }
        }

        private class GroqChoice
        {
            public GroqMessage? Message { get; set; }
        }

        private class GroqMessage
        {
            public string? Content { get; set; }
        }

        private class GroqExtractedData
        {
            public string? JobTitle { get; set; }
            public int? YearsOfExperience { get; set; }
            public string? PhoneNumber { get; set; }
            public string? CountryName { get; set; }
            public string? CityName { get; set; }
            public string? FirstLanguage { get; set; }
            public string? Bio { get; set; }
            public List<GroqExperience>? Experiences { get; set; }
            public List<GroqEducation>? Educations { get; set; }
            public List<GroqProject>? Projects { get; set; }
            public List<string>? Skills { get; set; }
            public GroqSocialAccounts? SocialAccounts { get; set; }
        }

        private class GroqExperience
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

        private class GroqEducation
        {
            public string? Institution { get; set; }
            public string? Degree { get; set; }
            public string? FieldOfStudy { get; set; }
            public string? GradeOrGpa { get; set; }
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }
            public bool? IsCurrent { get; set; }
        }

        private class GroqProject
        {
            public string? Title { get; set; }
            public string? TechnologiesUsed { get; set; }
            public string? Description { get; set; }
            public string? ProjectLink { get; set; }
        }

        private class GroqSocialAccounts
        {
            public string? LinkedIn { get; set; }
            public string? Github { get; set; }
            public string? Behance { get; set; }
            public string? Dribbble { get; set; }
            public string? PersonalWebsite { get; set; }
        }
    }
}
