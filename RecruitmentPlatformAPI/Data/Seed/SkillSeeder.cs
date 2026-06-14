using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Data.Seed
{
    public class SkillSeeder
    {
        private class SkillSeedItem
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("aliases")]
            public List<string> Aliases { get; set; } = new();
        }

        public static async Task SeedAsync(AppDbContext context, ILogger logger, string basePath)
        {
            try
            {
                var filePath = Path.Combine(basePath, "Data", "SeedData", "skills.json");
                if (!File.Exists(filePath))
                {
                    logger.LogWarning("Skill seed file not found at {FilePath}", filePath);
                    return;
                }

                if (await context.Skills.AnyAsync())
                {
                    logger.LogInformation("Skills already seeded. Skipping skill seeding.");
                    return;
                }

                logger.LogInformation("Seeding skills from JSON...");
                var jsonContent = await File.ReadAllTextAsync(filePath);

                var items = JsonSerializer.Deserialize<List<SkillSeedItem>>(jsonContent);
                if (items != null && items.Count > 0)
                {
                    var skills = items.Select(s => new Skill
                    {
                        Name = s.Name,
                        Aliases = string.Join(",", s.Aliases.Where(a => !string.IsNullOrWhiteSpace(a))),
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await context.Skills.AddRangeAsync(skills);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Successfully seeded {Count} skills.", skills.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding skills.");
            }
        }
    }
}
