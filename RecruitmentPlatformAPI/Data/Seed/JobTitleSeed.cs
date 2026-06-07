using System.Text.Json;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Data.Seed
{
    /// <summary>
    /// Seed data for JobTitle reference table.
    /// Reads from job-titles.json at runtime.
    /// </summary>
    public static class JobTitleSeed
    {
        public static List<JobTitle> GetJobTitles()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "SeedData", "job-titles.json");
            
            if (!File.Exists(path))
            {
                // Fallback to project directory if running from EF tools in development
                var projectDir = Directory.GetCurrentDirectory();
                var altPath = Path.Combine(projectDir, "Data", "SeedData", "job-titles.json");
                if (File.Exists(altPath))
                {
                    path = altPath;
                }
                else
                {
                    throw new FileNotFoundException($"Could not find seed file at {path} or {altPath}");
                }
            }

            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var titles = JsonSerializer.Deserialize<List<JobTitle>>(json, options);

            return titles ?? new List<JobTitle>();
        }
    }
}
