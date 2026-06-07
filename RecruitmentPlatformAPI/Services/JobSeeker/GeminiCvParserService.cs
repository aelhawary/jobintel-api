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
                
                var response = await _httpClient.PostAsJsonAsync(url, requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error: {Status} - {Error}", response.StatusCode, err);
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
                            IsCurrent = exp.IsCurrent ?? false
                        };
                        
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
                    foreach (var edu in parsed.Educations)
                    {
                        var mappedEdu = new ParsedEducationDto
                        {
                            Institution = edu.Institution,
                            Degree = edu.Degree,
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
                    var limitedSkills = parsed.SkillIds.Distinct().Take(15).ToList();
                    foreach (var skillId in limitedSkills)
                    {
                        if (!result.SkillIds.Contains(skillId))
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
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }
            public bool? IsCurrent { get; set; }
        }

        private class GeminiEducation
        {
            public string? Institution { get; set; }
            public string? Degree { get; set; }
            public string? FieldOfStudy { get; set; }
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
