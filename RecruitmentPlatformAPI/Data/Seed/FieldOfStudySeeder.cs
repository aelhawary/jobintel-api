using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Data.Seed
{
    public class FieldOfStudySeeder
    {
        private class FieldOfStudySeedItem
        {
            [JsonPropertyName("NameEn")]
            public string NameEn { get; set; } = string.Empty;

            [JsonPropertyName("NameAr")]
            public string NameAr { get; set; } = string.Empty;
        }

        public static async Task SeedAsync(AppDbContext context, ILogger logger, string basePath)
        {
            try
            {
                var filePath = Path.Combine(basePath, "Data", "SeedData", "FieldsOfStudy.json");
                if (!File.Exists(filePath))
                {
                    logger.LogWarning("FieldOfStudy seed file not found at {FilePath}", filePath);
                    return;
                }

                if (await context.FieldsOfStudy.AnyAsync())
                {
                    logger.LogInformation("Fields of study already seeded. Skipping seeding.");
                    return;
                }

                logger.LogInformation("Seeding fields of study from JSON...");
                var jsonContent = await File.ReadAllTextAsync(filePath);
                
                var items = JsonSerializer.Deserialize<List<FieldOfStudySeedItem>>(jsonContent);
                if (items != null && items.Count > 0)
                {
                    var fields = items.Select((f, index) => new FieldOfStudy
                    {
                        NameEn = f.NameEn,
                        NameAr = f.NameAr,
                        DisplayOrder = index,
                        IsActive = true
                    }).ToList();

                    await context.FieldsOfStudy.AddRangeAsync(fields);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Successfully seeded {Count} fields of study.", fields.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding fields of study.");
            }
        }
    }
}
