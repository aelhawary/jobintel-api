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

        public GeminiCvParserService(
            HttpClient httpClient,
            AppDbContext context,
            IOptions<LlmSettings> settings,
            ILogger<GeminiCvParserService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<ParsedResumeDataDto?> ParseResumeTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(_settings.GeminiApiKey) || _settings.GeminiApiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                _logger.LogWarning("Gemini API key is not configured. Skipping CV parsing.");
                return null;
            }

            try
            {
                var jobTitles = await _context.JobTitles.Where(j => j.IsActive).Select(j => new { j.Id, j.TitleEn }).ToListAsync();
                var jobTitlesListStr = string.Join(", ", jobTitles.Select(j => $"ID: {j.Id} - {j.TitleEn}"));

                var allSkills = await _context.Skills.Select(s => new { s.Id, s.Name }).ToListAsync();
                var skillsListStr = string.Join(", ", allSkills.Select(s => $"ID: {s.Id} - {s.Name}"));

                var prompt = $@"You are an expert HR CV parser. Extract the following details from the CV text and return ONLY a raw JSON object. Do not wrap it in markdown formatting blocks like ```json.
{{
  ""jobTitleId"": (integer, pick the single BEST matching Job Title ID from this list. Return 0 if no match: [{jobTitlesListStr}]),
  ""yearsOfExperience"": (integer, total years of experience, or 0),
  ""phoneNumber"": ""(string, extract phone number)"",
  ""countryName"": ""(string, current country)"",
  ""cityName"": ""(string, current city)"",
  ""firstLanguage"": ""(string, main language name, e.g. 'English', 'Arabic')"",
  ""bio"": ""(string, a 2-3 sentence professional summary based on the CV)"",
  ""experiences"": [
    {{
      ""jobTitle"": ""(string)"",
      ""companyName"": ""(string)"",
      ""countryName"": ""(string)"",
      ""cityName"": ""(string)"",
      ""employmentType"": ""(string, one of: FullTime, PartTime, Contract, Freelance, Internship. Infer from context if not explicit.)"",
      ""responsibilities"": ""(string, comma-separated list of key responsibilities and achievements. Max 2000 chars.)"",
      ""startDate"": ""(string, YYYY-MM-DD)"",
      ""endDate"": ""(string, YYYY-MM-DD, or null if current)"",
      ""isCurrent"": (boolean)
    }}
  ],
  ""educations"": [
    {{
      ""institution"": ""(string)"",
      ""degree"": ""(string, e.g. Bachelor, Master, PhD, Diploma, HighSchool, Associate, Other)"",
      ""fieldOfStudy"": ""(string)"",
      ""gradeOrGpa"": ""(string, GPA or grade if mentioned, e.g. '3.8/4.0', 'First Class Honours', null if not mentioned)"",
      ""startDate"": ""(string, YYYY-MM-DD)"",
      ""endDate"": ""(string, YYYY-MM-DD, or null if current)"",
      ""isCurrent"": (boolean)
    }}
  ],
  ""projects"": [
    {{
      ""title"": ""(string)"",
      ""technologiesUsed"": ""(string, comma separated)"",
      ""description"": ""(string)"",
      ""projectLink"": ""(string, url if present)""
    }}
  ],
  ""skillIds"": [(array of integers, strictly map candidate's CORE skills to the best matching Skill IDs from this list: [{skillsListStr}]. CRITICAL: You MUST select NO MORE THAN 15 skills. Only pick the absolute most important technical and professional skills.)],
  ""socialAccounts"": {{
    ""linkedIn"": ""(string, url)"",
    ""github"": ""(string, url)"",
    ""behance"": ""(string, url)"",
    ""dribbble"": ""(string, url)"",
    ""personalWebsite"": ""(string, url)""
  }}
}}

CV Text:
{text}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
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

                // Retry transient errors (429 / 5xx) with exponential backoff.
                // Gemini routinely returns 503 "model is currently experiencing
                // high demand" during traffic spikes; Google's own message
                // advises "please try again later". Giving up on the first
                // failure was the root cause of failed CV parses during peak
                // load. Non-transient errors (400 / 401 / 403) fail fast.
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
                        return null;
                    }

                    // Respect server-provided Retry-After when present,
                    // otherwise fall back to exponential backoff (2s, 4s, 8s, 16s).
                    var delay = TimeSpan.FromSeconds(2 * Math.Pow(2, attempt - 1));
                    if (response.Headers.RetryAfter is { } retryAfter)
                    {
                        if (retryAfter.Delta.HasValue)
                        {
                            delay = retryAfter.Delta.Value;
                        }
                        else if (retryAfter.Date.HasValue)
                        {
                            var serverDelay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
                            if (serverDelay > TimeSpan.Zero) delay = serverDelay;
                        }
                    }

                    _logger.LogWarning(
                        "Gemini API transient error {Status} on attempt {Attempt}/{Max}. Retrying in {DelaySeconds:F1}s...",
                        response.StatusCode, attempt, maxAttempts, delay.TotalSeconds);

                    // Dispose the failed response before waiting; we will
                    // issue a fresh request on the next attempt.
                    response.Dispose();
                    await Task.Delay(delay);
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    return null;
                }

                var jsonDoc = await response.Content.ReadFromJsonAsync<JsonElement>();
                var contentText = jsonDoc.GetProperty("candidates")[0]
                                      .GetProperty("content")
                                      .GetProperty("parts")[0]
                                      .GetProperty("text").GetString();

                if (string.IsNullOrWhiteSpace(contentText)) return null;

                var parsed = JsonSerializer.Deserialize<GeminiExtractedData>(contentText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed == null) return null;

                // Map to DB IDs
                var result = new ParsedResumeDataDto
                {
                    YearsOfExperience = parsed.YearsOfExperience,
                    PhoneNumber = parsed.PhoneNumber,
                    Bio = parsed.Bio
                };

                // Fetch reference data into memory for fast fuzzy matching
                var refCountries = await _context.Countries.ToListAsync();
                var refLanguages = await _context.Languages.ToListAsync();
                var refJobTitles = await _context.JobTitles.ToListAsync();
                var refCities = await _context.Cities.ToListAsync();
                var refFieldsOfStudy = await _context.FieldsOfStudy.Where(f => f.IsActive).ToListAsync();

                // Job Title Match
                if (parsed.JobTitleId.HasValue && parsed.JobTitleId.Value > 0)
                {
                    var jt = refJobTitles.FirstOrDefault(x => x.Id == parsed.JobTitleId.Value);
                    if (jt != null) { result.JobTitleId = jt.Id; result.JobTitleName = jt.TitleEn; }
                }

                // Country Match
                if (!string.IsNullOrWhiteSpace(parsed.CountryName))
                {
                    var c = FuzzyMatchHelper.FindBestMatch(parsed.CountryName, refCountries, x => x.NameEn, maxDistance: 4);
                    if (c != null) { result.CountryId = c.Id; result.CountryName = c.NameEn; }
                }

                // City Match
                if (!string.IsNullOrWhiteSpace(parsed.CityName))
                {
                    // If we have a country matched, prioritize cities in that country, otherwise search all
                    var cityCandidates = result.CountryId.HasValue ? refCities.Where(x => x.CountryId == result.CountryId.Value) : refCities;
                    var c = FuzzyMatchHelper.FindBestMatch(parsed.CityName, cityCandidates, x => x.NameEn, maxDistance: 4);
                    if (c != null) { result.CityId = c.Id; result.CityName = c.NameEn; }
                }

                // Language Match
                if (!string.IsNullOrWhiteSpace(parsed.FirstLanguage))
                {
                    var l = FuzzyMatchHelper.FindBestMatch(parsed.FirstLanguage, refLanguages, x => x.NameEn, maxDistance: 3);
                    if (l != null) { result.FirstLanguageId = l.Id; result.FirstLanguageName = l.NameEn; }
                }

                // Map Collections
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
                            var cityCandidates = mappedExp.CountryId.HasValue ? refCities.Where(x => x.CountryId == mappedExp.CountryId.Value) : refCities;
                            var c = FuzzyMatchHelper.FindBestMatch(exp.CityName, cityCandidates, x => x.NameEn, maxDistance: 4);
                            if (c != null) { mappedExp.CityId = c.Id; mappedExp.CityName = c.NameEn; }
                        }

                        result.Experiences.Add(mappedExp);
                    }
                }

                if (parsed.Educations != null)
                {
                    var validFieldOfStudyIds = await _context.FieldsOfStudy.Where(f => f.IsActive).Select(f => f.Id).ToHashSetAsync();
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
                        }
                        if (mappedEdu.FieldOfStudyId.HasValue && !validFieldOfStudyIds.Contains(mappedEdu.FieldOfStudyId.Value))
                        {
                            mappedEdu.FieldOfStudyId = null;
                            mappedEdu.FieldOfStudyName = null;
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

                if (parsed.SkillIds != null)
                {
                    var validSkillIds = await _context.Skills.Select(s => s.Id).ToHashSetAsync();
                    var limitedSkills = parsed.SkillIds.Distinct().Take(15).ToList();
                    foreach (var skillId in limitedSkills)
                    {
                        if (validSkillIds.Contains(skillId) && !result.SkillIds.Contains(skillId))
                        {
                            result.SkillIds.Add(skillId);
                        }
                    }
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CV with Gemini.");
                return null;
            }
        }

        /// <summary>
        /// Returns true for HTTP status codes that are worth retrying: 429
        /// (rate limit) and the 5xx server-error family. Everything else
        /// (400, 401, 403, 404, …) is a client mistake or auth problem and
        /// will not get better with a retry.
        /// </summary>
        private static bool IsTransientError(int statusCode)
        {
            return statusCode == 429
                || (statusCode >= 500 && statusCode <= 504);
        }

        private class GeminiExtractedData
        {
            public int? JobTitleId { get; set; }
            public int? YearsOfExperience { get; set; }
            public string? PhoneNumber { get; set; }
            public string? CountryName { get; set; }
            public string? CityName { get; set; }
            public string? FirstLanguage { get; set; }
            public string? Bio { get; set; }
            public List<GeminiExperience>? Experiences { get; set; }
            public List<GeminiEducation>? Educations { get; set; }
            public List<GeminiProject>? Projects { get; set; }
            public List<int>? SkillIds { get; set; }
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
