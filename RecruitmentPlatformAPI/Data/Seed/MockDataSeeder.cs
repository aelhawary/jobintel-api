using Bogus;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Identity;
using RecruitmentPlatformAPI.Models.Jobs;
using RecruitmentPlatformAPI.Models.JobSeeker;
using RecruitmentPlatformAPI.Models.Recruiter;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Data.Seed
{
    /// <summary>
    /// Seeds realistic mock data for development and AI Recommendation Engine testing.
    /// Generates ~30 Job Seekers, 3 Recruiters, and ~10 Jobs with skills.
    /// Idempotent: skips entirely if JobSeekers or Jobs already exist.
    /// Uses a fixed Bogus seed (42) for deterministic, reproducible data.
    /// </summary>
    public static class MockDataSeeder
    {
        private const int JobSeekerCount = 30;
        private const int RecruiterCount = 3;
        private const int JobCount = 10;
        private const int BogusSeed = 42;
        private const string MockPassword = "Mock@123";

        public static async Task SeedAsync(AppDbContext context, ILogger logger)
        {
            // ── Guard: skip if data already exists ──
            var jobSeekerCount = await context.JobSeekers.CountAsync();
            var jobCount = await context.Jobs.CountAsync();

            if (jobSeekerCount > 10 && jobCount > 0)
            {
                logger.LogInformation("Mock data already exists. Skipping MockDataSeeder.");
                return;
            }

            logger.LogInformation("Seeding mock data for development...");

            // ── Load reference data from DB ──
            var countriesWithCities = await context.Countries
                .Include(c => c.Cities)
                .Where(c => c.Cities.Any())
                .ToListAsync();

            if (!countriesWithCities.Any())
            {
                logger.LogWarning("No countries with cities found. Cannot seed mock data.");
                return;
            }

            var skillIds = await context.Skills.Select(s => s.Id).ToListAsync();
            var jobTitleIds = await context.JobTitles.Select(jt => jt.Id).ToListAsync();
            var languageIds = await context.Languages.Select(l => l.Id).ToListAsync();

            if (!skillIds.Any() || !jobTitleIds.Any() || !languageIds.Any())
            {
                logger.LogWarning("Reference data (Skills/JobTitles/Languages) not found. Cannot seed mock data.");
                return;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(MockPassword);
            var faker = new Faker { Random = new Randomizer(BogusSeed) };

            // ══════════════════════════════════════════
            // STEP 1: Create Job Seeker Users
            // ══════════════════════════════════════════
            var jobSeekerUsers = new List<User>();
            var jobSeekerUserFaker = new Faker<User>()
                .UseSeed(BogusSeed)
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName).ToLower())
                .RuleFor(u => u.PasswordHash, _ => passwordHash)
                .RuleFor(u => u.AccountType, _ => AccountType.JobSeeker)
                .RuleFor(u => u.AuthProvider, _ => AuthProvider.Email)
                .RuleFor(u => u.IsEmailVerified, _ => true)
                .RuleFor(u => u.IsActive, _ => true)
                .RuleFor(u => u.ProfileCompletionStep, _ => 4)
                .RuleFor(u => u.CreatedAt, f => f.Date.Recent(60).ToUniversalTime())
                .RuleFor(u => u.UpdatedAt, (f, u) => u.CreatedAt);

            jobSeekerUsers = jobSeekerUserFaker.Generate(JobSeekerCount);

            // ══════════════════════════════════════════
            // STEP 2: Create Recruiter Users
            // ══════════════════════════════════════════
            var recruiterUserFaker = new Faker<User>()
                .UseSeed(BogusSeed + 100)
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName, "company.com").ToLower())
                .RuleFor(u => u.PasswordHash, _ => passwordHash)
                .RuleFor(u => u.AccountType, _ => AccountType.Recruiter)
                .RuleFor(u => u.AuthProvider, _ => AuthProvider.Email)
                .RuleFor(u => u.IsEmailVerified, _ => true)
                .RuleFor(u => u.IsActive, _ => true)
                .RuleFor(u => u.ProfileCompletionStep, _ => 1) // Recruiter wizard is single-step
                .RuleFor(u => u.CreatedAt, f => f.Date.Recent(90).ToUniversalTime())
                .RuleFor(u => u.UpdatedAt, (f, u) => u.CreatedAt);

            var recruiterUsers = recruiterUserFaker.Generate(RecruiterCount);

            // Save all users first to get auto-generated IDs
            var allUsers = new List<User>();
            allUsers.AddRange(jobSeekerUsers);
            allUsers.AddRange(recruiterUsers);

            await context.Users.AddRangeAsync(allUsers);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} users ({JsCount} job seekers, {RCount} recruiters).",
                allUsers.Count, jobSeekerUsers.Count, recruiterUsers.Count);

            // ══════════════════════════════════════════
            // STEP 3: Create JobSeeker entities
            // ══════════════════════════════════════════
            var workModelValues = Enum.GetValues<WorkModel>();
            var proficiencyValues = Enum.GetValues<LanguageProficiency>();
            var employmentTypeValues = Enum.GetValues<EmploymentType>();

            var jobSeekers = new List<JobSeeker>();
            for (int i = 0; i < JobSeekerCount; i++)
            {
                var user = jobSeekerUsers[i];
                var country = faker.PickRandom(countriesWithCities);
                var city = faker.PickRandom(country.Cities.ToList());
                var jobTitleId = faker.PickRandom(jobTitleIds);

                // Random 1-3 distinct WorkModel preferences
                var workPrefs = faker.PickRandom(workModelValues, faker.Random.Int(1, 3))
                    .Distinct()
                    .ToList();

                // Random 1-3 distinct EmploymentType preferences
                var empTypes = faker.PickRandom(employmentTypeValues, faker.Random.Int(1, 3))
                    .Distinct()
                    .ToList();

                // Second language: nullable ~30% of the time
                int? secondLangId = faker.Random.Bool(0.7f)
                    ? faker.PickRandom(languageIds)
                    : null;

                var js = new JobSeeker
                {
                    UserId = user.Id,
                    JobTitleId = jobTitleId,
                    YearsOfExperience = faker.Random.Int(0, 15),
                    CountryId = country.Id,
                    CityId = city.Id,
                    PhoneNumber = faker.Phone.PhoneNumber("+##########"),
                    FirstLanguageId = faker.PickRandom(languageIds),
                    FirstLanguageProficiency = faker.PickRandom(proficiencyValues),
                    SecondLanguageId = secondLangId,
                    SecondLanguageProficiency = secondLangId.HasValue
                        ? faker.PickRandom(proficiencyValues)
                        : null,
                    Bio = faker.Lorem.Sentences(2),
                    WorkPreferences = workPrefs,
                    DesiredEmploymentTypes = empTypes,
                    CurrentAssessmentScore = Math.Round(faker.Random.Decimal(40m, 95m), 2),
                    LastAssessmentDate = faker.Date.Recent(30).ToUniversalTime(),
                    AssessmentJobTitleId = jobTitleId,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                jobSeekers.Add(js);
            }

            await context.JobSeekers.AddRangeAsync(jobSeekers);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} job seekers.", jobSeekers.Count);

            // ══════════════════════════════════════════
            // STEP 4: Create Recruiter entities
            // ══════════════════════════════════════════
            var companySizes = new[] { "1-50", "51-200", "201-500", "500+" };

            var recruiters = new List<Recruiter>();
            for (int i = 0; i < RecruiterCount; i++)
            {
                var user = recruiterUsers[i];
                var country = faker.PickRandom(countriesWithCities);
                var city = faker.PickRandom(country.Cities.ToList());

                var recruiter = new Recruiter
                {
                    UserId = user.Id,
                    CompanyName = faker.Company.CompanyName(),
                    CompanySize = faker.PickRandom(companySizes),
                    Industry = faker.Commerce.Department(),
                    CountryId = country.Id,
                    CityId = city.Id,
                    Website = faker.Internet.Url(),
                    LinkedIn = $"https://linkedin.com/company/{faker.Internet.DomainWord()}",
                    CompanyDescription = faker.Company.CatchPhrase(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                recruiters.Add(recruiter);
            }

            await context.Recruiters.AddRangeAsync(recruiters);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} recruiters.", recruiters.Count);

            // ══════════════════════════════════════════
            // STEP 5: Create JobSeekerSkills (3-5 per seeker)
            // ══════════════════════════════════════════
            var jobSeekerSkills = new List<JobSeekerSkill>();
            foreach (var js in jobSeekers)
            {
                var skillCount = faker.Random.Int(3, 5);
                var selectedSkills = faker.PickRandom(skillIds, skillCount).Distinct().ToList();

                foreach (var skillId in selectedSkills)
                {
                    jobSeekerSkills.Add(new JobSeekerSkill
                    {
                        JobSeekerId = js.Id,
                        SkillId = skillId,
                        Source = "Self"
                    });
                }
            }

            await context.JobSeekerSkills.AddRangeAsync(jobSeekerSkills);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} job seeker skills.", jobSeekerSkills.Count);

            // ══════════════════════════════════════════
            // STEP 6: Create Jobs (10 jobs across recruiters)
            // ══════════════════════════════════════════

            var jobs = new List<Job>();
            for (int i = 0; i < JobCount; i++)
            {
                var recruiter = recruiters[i % RecruiterCount];
                var workModel = faker.PickRandom(workModelValues);

                // Business rule: Remote jobs have no location
                int? countryId = null;
                int? cityId = null;
                if (workModel != WorkModel.Remote)
                {
                    var country = faker.PickRandom(countriesWithCities);
                    var city = faker.PickRandom(country.Cities.ToList());
                    countryId = country.Id;
                    cityId = city.Id;
                }

                var job = new Job
                {
                    RecruiterId = recruiter.Id,
                    Title = faker.Name.JobTitle(),
                    JobTitleId = faker.PickRandom(jobTitleIds),
                    Description = faker.Lorem.Paragraphs(2),
                    Requirements = faker.Lorem.Paragraphs(1),
                    EmploymentType = faker.PickRandom(employmentTypeValues),
                    MinYearsOfExperience = faker.Random.Int(0, 8),
                    WorkModel = workModel,
                    CountryId = countryId,
                    CityId = cityId,
                    IsActive = true,
                    PostedAt = faker.Date.Recent(30).ToUniversalTime(),
                    UpdatedAt = DateTime.UtcNow
                };

                jobs.Add(job);
            }

            await context.Jobs.AddRangeAsync(jobs);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} jobs.", jobs.Count);

            // ══════════════════════════════════════════
            // STEP 7: Create JobSkills (3-5 per job)
            // ══════════════════════════════════════════
            var jobSkills = new List<JobSkill>();
            foreach (var job in jobs)
            {
                var skillCount = faker.Random.Int(3, 5);
                var selectedSkills = faker.PickRandom(skillIds, skillCount).Distinct().ToList();

                foreach (var skillId in selectedSkills)
                {
                    jobSkills.Add(new JobSkill
                    {
                        JobId = job.Id,
                        SkillId = skillId
                    });
                }
            }

            await context.JobSkills.AddRangeAsync(jobSkills);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} job skills.", jobSkills.Count);

            logger.LogInformation(
                "MockDataSeeder complete — {JsCount} job seekers, {RCount} recruiters, {JCount} jobs.",
                jobSeekers.Count, recruiters.Count, jobs.Count);
        }
    }
}
