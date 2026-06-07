using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Data.Seed
{
    public class LanguageSeeder
    {
        private class LanguageJsonWrapper
        {
            [JsonPropertyName("languages")]
            public List<LanguageSeedItem> Languages { get; set; } = new();
        }

        private class LanguageSeedItem
        {
            [JsonPropertyName("code")]
            public string Code { get; set; } = string.Empty;

            [JsonPropertyName("english_name")]
            public string EnglishName { get; set; } = string.Empty;

            [JsonPropertyName("arabic_name")]
            public string ArabicName { get; set; } = string.Empty;
        }

        public static async Task SeedAsync(AppDbContext context, ILogger logger, string basePath)
        {
            try
            {
                var filePath = Path.Combine(basePath, "Data", "SeedData", "languages.json");
                if (!File.Exists(filePath))
                {
                    logger.LogWarning("Language seed file not found at {FilePath}", filePath);
                    return;
                }

                if (await context.Languages.AnyAsync())
                {
                    logger.LogInformation("Languages already seeded. Skipping language seeding.");
                    return;
                }

                logger.LogInformation("Seeding languages from JSON...");
                var jsonContent = await File.ReadAllTextAsync(filePath);
                
                var wrapper = JsonSerializer.Deserialize<LanguageJsonWrapper>(jsonContent);
                if (wrapper?.Languages != null && wrapper.Languages.Count > 0)
                {
                    var languages = wrapper.Languages.Select((lang, index) => new Language
                    {
                        Code = lang.Code,
                        NameEn = lang.EnglishName,
                        NameAr = lang.ArabicName,
                        SortOrder = index,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await context.Languages.AddRangeAsync(languages);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Successfully seeded {Count} languages.", languages.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding languages.");
            }
        }
    }
}
