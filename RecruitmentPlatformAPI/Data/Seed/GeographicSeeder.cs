using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Data.Seed
{
    public static class GeographicSeeder
    {
        public static async Task SeedAsync(AppDbContext context, IWebHostEnvironment env, ILogger logger)
        {
            try
            {
                // If a previous version of the seed run is present (no CountryCode populated), wipe the table and re-seed.
                // This is safe because the data is purely from countries.json — no user-generated content lives here.
                var existingCount = await context.Countries.CountAsync();
                if (existingCount > 0)
                {
                    var sample = await context.Countries.AsNoTracking().FirstOrDefaultAsync();
                    if (sample != null && string.IsNullOrEmpty(sample.CountryCode))
                    {
                        logger.LogInformation("Existing country rows are missing CountryCode. Wiping Country table and re-seeding from countries.json...");
                        await context.Database.ExecuteSqlRawAsync("DELETE FROM [Country]");
                    }
                    else
                    {
                        logger.LogInformation("Countries already seeded with CountryCode. Skipping geographic seeding.");
                        return;
                    }
                }

                // Path to countries.json (located in Data/Seed)
                var filePath = Path.Combine(env.ContentRootPath, "Data", "SeedData", "countries.json");

                if (!File.Exists(filePath))
                {
                    logger.LogWarning("Geographic data file not found at {FilePath}", filePath);
                    return;
                }

                var jsonContent = await File.ReadAllTextAsync(filePath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var countryDtos = JsonSerializer.Deserialize<List<CountryJsonDto>>(jsonContent, options);

                if (countryDtos == null || !countryDtos.Any())
                {
                    logger.LogWarning("No data could be deserialized from countries.json");
                    return;
                }

                var countriesToInsert = countryDtos.Select(dto => new Country
                {
                    Id = dto.Id,
                    NameEn = dto.Name,
                    NameAr = dto.Translations != null && dto.Translations.ContainsKey("ar") ? dto.Translations["ar"] : dto.Name,
                    CountryCode = string.IsNullOrWhiteSpace(dto.Iso2) ? null : dto.Iso2.ToUpperInvariant()
                }).ToList();

                // We must preserve the exact Id integers, so we wrap the EF Core insertion inside a transaction and use SET IDENTITY_INSERT
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Country ON");

                    await context.Countries.AddRangeAsync(countriesToInsert);
                    await context.SaveChangesAsync();

                    await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Country OFF");

                    await transaction.CommitAsync();
                    logger.LogInformation("Successfully seeded {Count} countries (with CountryCode)", countriesToInsert.Count);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // Attempt to ensure IDENTITY_INSERT is turned off even if insertion fails
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Country OFF");
                    }
                    catch { /* Ignore */ }

                    logger.LogError(ex, "Failed to seed countries with IDENTITY_INSERT");
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding countries data");
            }
        }

        public static async Task SeedCitiesAsync(AppDbContext context, IWebHostEnvironment env, ILogger logger)
        {
            try
            {
                var cityCount = await context.Cities.CountAsync();
                
                // If we have massive granular data from the old cities.json (150k+ records), wipe it.
                if (cityCount > 10000)
                {
                    logger.LogInformation("Found granular cities (Count: {Count}). Clearing the City table to replace with states/regions...", cityCount);
                    // EF Core 7+ bulk delete
                    await context.Cities.ExecuteDeleteAsync();
                    logger.LogInformation("Successfully cleared the City table.");
                }
                else if (cityCount > 0)
                {
                    logger.LogInformation("Regions/States already seeded as Cities. Skipping geographic seeding.");
                    return;
                }

                var filePath = Path.Combine(env.ContentRootPath, "Data", "SeedData", "states.json");

                if (!File.Exists(filePath))
                {
                    logger.LogWarning("States data file not found at {FilePath}", filePath);
                    return;
                }

                logger.LogInformation("Reading states data file...");
                var jsonContent = await File.ReadAllTextAsync(filePath);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                logger.LogInformation("Deserializing states data...");
                var stateDtos = JsonSerializer.Deserialize<List<StateJsonDto>>(jsonContent, options);

                if (stateDtos == null || !stateDtos.Any())
                {
                    logger.LogWarning("No data could be deserialized from states.json");
                    return;
                }

                var citiesToInsert = stateDtos.Select(dto => new City
                {
                    Id = dto.Id,
                    NameEn = dto.Name,
                    NameAr = dto.Translations != null && dto.Translations.ContainsKey("ar") ? dto.Translations["ar"] : dto.Name,
                    CountryId = dto.CountryId
                }).ToList();

                logger.LogInformation("Beginning city insertion process (Total: {Count})", citiesToInsert.Count);

                using var transaction = await context.Database.BeginTransactionAsync();
                
                // Performance: disable auto detect changes
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                try
                {
                    await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT City ON");
                    
                    var chunkSize = 10000;
                    var totalChunks = (int)Math.Ceiling((double)citiesToInsert.Count / chunkSize);

                    for (int i = 0; i < totalChunks; i++)
                    {
                        var chunk = citiesToInsert.Skip(i * chunkSize).Take(chunkSize).ToList();
                        
                        await context.Cities.AddRangeAsync(chunk);
                        await context.SaveChangesAsync();
                        
                        // Clear the tracker to completely free memory
                        context.ChangeTracker.Clear();
                        
                        logger.LogInformation("Seeded chunk {Chunk}/{TotalChunks} ({Count} cities)", i + 1, totalChunks, chunk.Count);
                    }

                    await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT City OFF");
                    
                    await transaction.CommitAsync();
                    logger.LogInformation("Successfully seeded all {Count} cities", citiesToInsert.Count);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    
                    try 
                    {
                        await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT City OFF");
                    } 
                    catch { /* Ignore */ }
                    
                    logger.LogError(ex, "Failed to seed cities with IDENTITY_INSERT");
                    throw;
                }
                finally
                {
                    // Re-enable auto detect changes
                    context.ChangeTracker.AutoDetectChangesEnabled = true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding cities data");
            }
        }

        private class CountryJsonDto
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("iso2")]
            public string? Iso2 { get; set; }

            [JsonPropertyName("translations")]
            public Dictionary<string, string>? Translations { get; set; }
        }

        private class StateJsonDto
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("country_id")]
            public int CountryId { get; set; }

            [JsonPropertyName("translations")]
            public Dictionary<string, string>? Translations { get; set; }
        }
    }
}
