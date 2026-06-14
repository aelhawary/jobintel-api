using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data.Seed;
using RecruitmentPlatformAPI.Models.Identity;
using RecruitmentPlatformAPI.Models.JobSeeker;
using RecruitmentPlatformAPI.Models.Recruiter;
using RecruitmentPlatformAPI.Models.Reference;
using RecruitmentPlatformAPI.Models.Jobs;
using RecruitmentPlatformAPI.Enums;

using RecruitmentPlatformAPI.Models.Assessment.V2;

namespace RecruitmentPlatformAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<JobSeeker> JobSeekers { get; set; }
        public DbSet<Recruiter> Recruiters { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<SocialAccount> SocialAccounts { get; set; }
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<JobSeekerSkill> JobSeekerSkills { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobSkill> JobSkills { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<ShortlistedCandidate> ShortlistedCandidates { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<FieldOfStudy> FieldsOfStudy { get; set; }
        
        // Assessment V2 Models
        public DbSet<AssessmentQuestionV2> AssessmentQuestionsV2 { get; set; }
        public DbSet<AssessmentAttemptV2> AssessmentAttemptsV2 { get; set; }
        public DbSet<AssessmentAnswerV2> AssessmentAnswersV2 { get; set; }

        // Engagement Analytics
        public DbSet<ProfileView> ProfileViews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(b =>
            {
                // Use the plain table name; EF will quote identifiers when generating SQL.
                b.ToTable("User");
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Email).IsUnique();
                
                // Store AccountType enum as string in database
                b.Property(u => u.AccountType)
                 .HasConversion<string>();
                
                // Store AuthProvider enum as int in database
                b.Property(u => u.AuthProvider)
                 .HasConversion<int>();
            });

            // JobSeeker - one-to-one with User
            modelBuilder.Entity<JobSeeker>(b =>
            {
                b.HasOne(j => j.User)
                 .WithOne()
                 .HasForeignKey<JobSeeker>(j => j.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                
                // Store enums as strings in database
                b.Property(j => j.FirstLanguageProficiency)
                 .HasConversion<string>();
                
                b.Property(j => j.SecondLanguageProficiency)
                 .HasConversion<string>();
                
                b.Property(j => j.CurrentAssessmentScore).HasPrecision(5, 2);

                // Configure primitive collection for WorkPreferences (stored as JSON).
                // ElementType().HasConversion<string>() ensures enum values are serialized as
                // string labels (e.g. ["Remote","Hybrid"]) instead of integers ([0,1]).
                // This is critical for Contains() SQL translation parity with Job.WorkModel,
                // which also uses HasConversion<string>().
                b.PrimitiveCollection(j => j.WorkPreferences)
                 .ElementType()
                 .HasConversion<string>();

                b.PrimitiveCollection(j => j.DesiredEmploymentTypes)
                 .ElementType()
                 .HasConversion<string>();
            });

            // Recruiter - one-to-one with User
            modelBuilder.Entity<Recruiter>(b =>
            {
                 b.HasOne(r => r.User)
                  .WithOne()
                  .HasForeignKey<Recruiter>(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

                 b.HasOne(r => r.Country)
                  .WithMany()
                  .HasForeignKey(r => r.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);

                 b.HasOne(r => r.City)
                  .WithMany()
                  .HasForeignKey(r => r.CityId)
                  .OnDelete(DeleteBehavior.Restrict);
            });

            // Education - many-to-one with JobSeeker
            modelBuilder.Entity<Education>(b =>
            {
                b.HasOne(e => e.JobSeeker)
                 .WithMany()
                 .HasForeignKey(e => e.JobSeekerId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Store Degree enum as string in database
                b.Property(e => e.Degree)
                 .HasConversion<string>()
                 .HasMaxLength(50);

                b.HasOne(e => e.FieldOfStudy)
                 .WithMany()
                 .HasForeignKey(e => e.FieldOfStudyId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Restrict);

                // Check constraint: EndDate must be >= StartDate (database-agnostic)
                b.ToTable(t => t.HasCheckConstraint(
                    "CK_Education_EndDateAfterStartDate",
                    "[EndDate] IS NULL OR [EndDate] >= [StartDate]"
                ));

                // Index for querying non-deleted education entries
                b.HasIndex(e => new { e.JobSeekerId, e.IsDeleted })
                 .HasDatabaseName("IX_Education_JobSeekerId_IsDeleted");
            });

            // Experience - many-to-one with JobSeeker
            modelBuilder.Entity<Experience>(b =>
            {
                b.HasOne(e => e.JobSeeker)
                 .WithMany()
                 .HasForeignKey(e => e.JobSeekerId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(e => e.Country)
                 .WithMany()
                 .HasForeignKey(e => e.CountryId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.City)
                 .WithMany()
                 .HasForeignKey(e => e.CityId)
                 .OnDelete(DeleteBehavior.Restrict);
                
                // Store EmploymentType enum as int in database
                b.Property(e => e.EmploymentType)
                 .HasConversion<int>();
                
                // Check constraint: EndDate must be >= StartDate
                b.ToTable(t => t.HasCheckConstraint(
                    "CK_Experience_EndDateAfterStartDate",
                    "[EndDate] IS NULL OR [EndDate] >= [StartDate]"
                ));
                
                // Index for querying non-deleted experiences
                b.HasIndex(e => new { e.JobSeekerId, e.IsDeleted })
                 .HasDatabaseName("IX_Experience_JobSeekerId_IsDeleted");
            });

            // Project - many-to-one with JobSeeker
            modelBuilder.Entity<Project>(b =>
            {
                b.HasOne(p => p.JobSeeker)
                 .WithMany()
                 .HasForeignKey(p => p.JobSeekerId)
                 .OnDelete(DeleteBehavior.Cascade);
                
                // Index for querying non-deleted projects
                b.HasIndex(p => new { p.JobSeekerId, p.IsDeleted })
                 .HasDatabaseName("IX_Project_JobSeekerId_IsDeleted");
            });

            // Resume - many-to-one with JobSeeker (one active resume per job seeker)
            modelBuilder.Entity<Resume>(b =>
            {
                b.HasOne(r => r.JobSeeker)
                 .WithMany()
                 .HasForeignKey(r => r.JobSeekerId)
                 .OnDelete(DeleteBehavior.Cascade);
                
                // Ensure only one CV per job seeker
                b.HasIndex(r => r.JobSeekerId)
                 .IsUnique()
                 .HasDatabaseName("IX_Resume_JobSeekerId_Unique");
            });

            // SocialAccount - one-to-one with JobSeeker
            modelBuilder.Entity<SocialAccount>(b =>
            {
                b.HasOne(s => s.JobSeeker)
                 .WithOne()
                 .HasForeignKey<SocialAccount>(s => s.JobSeekerId)
                 .OnDelete(DeleteBehavior.Cascade);
                
                b.HasIndex(s => s.JobSeekerId).IsUnique();
            });

            // Certificate - many-to-one with JobSeeker
            modelBuilder.Entity<Certificate>(b =>
            {
                b.HasOne(c => c.JobSeeker)
                 .WithMany()
                 .HasForeignKey(c => c.JobSeekerId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Check constraint: ExpirationDate must be >= IssueDate
                b.ToTable(t => t.HasCheckConstraint(
                    "CK_Certificate_ExpirationDateAfterIssueDate",
                    "[ExpirationDate] IS NULL OR [IssueDate] IS NULL OR [ExpirationDate] >= [IssueDate]"
                ));

                // Index for querying non-deleted certificates
                b.HasIndex(c => new { c.JobSeekerId, c.IsDeleted })
                 .HasDatabaseName("IX_Certificate_JobSeekerId_IsDeleted");
            });

            // Job - many-to-one with Recruiter
            modelBuilder.Entity<Job>(b =>
            {
                b.HasOne(j => j.Recruiter)
                 .WithMany()
                 .HasForeignKey(j => j.RecruiterId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(j => j.Country)
                 .WithMany()
                 .HasForeignKey(j => j.CountryId)
                 .OnDelete(DeleteBehavior.Restrict);
                 
                b.HasOne(j => j.City)
                 .WithMany()
                 .HasForeignKey(j => j.CityId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(j => j.JobTitle)
                 .WithMany()
                 .HasForeignKey(j => j.JobTitleId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Store Enums as strings in database
                b.Property(j => j.EmploymentType)
                 .HasConversion<string>()
                 .HasMaxLength(50);
                 
                b.Property(j => j.WorkModel)
                 .HasConversion<string>()
                 .HasMaxLength(50);
            });

            // JobSeekerSkill - many-to-many junction
            modelBuilder.Entity<JobSeekerSkill>(b =>
            {
                b.HasOne(js => js.JobSeeker)
                 .WithMany()
                 .HasForeignKey(js => js.JobSeekerId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(js => js.Skill)
                 .WithMany()
                 .HasForeignKey(js => js.SkillId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(js => new { js.JobSeekerId, js.SkillId }).IsUnique();
            });

            // JobSkill - many-to-many junction
            modelBuilder.Entity<JobSkill>(b =>
            {
                b.HasOne(js => js.Job)
                 .WithMany(j => j.JobSkills)
                 .HasForeignKey(js => js.JobId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(js => js.Skill)
                 .WithMany()
                 .HasForeignKey(js => js.SkillId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(js => new { js.JobId, js.SkillId }).IsUnique();
            });

            // Recommendation - many-to-many junction with metadata
            // Note: Using Restrict on JobSeeker to avoid cascade cycle
            modelBuilder.Entity<Recommendation>(b =>
            {
                b.HasOne(r => r.Job)
                 .WithMany()
                 .HasForeignKey(r => r.JobId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(r => r.JobSeeker)
                 .WithMany()
                 .HasForeignKey(r => r.JobSeekerId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(r => new { r.JobId, r.JobSeekerId }).IsUnique();
                b.Property(r => r.MatchScore).HasPrecision(5, 2);

                // Index for querying recommendations by job seeker (for engagement stats)
                b.HasIndex(r => new { r.JobSeekerId, r.GeneratedAt })
                 .HasDatabaseName("IX_Recommendation_JobSeeker_Date");
            });

            // ShortlistedCandidate
            modelBuilder.Entity<ShortlistedCandidate>(b =>
            {
                b.ToTable("ShortlistedCandidate");
                b.HasKey(sc => sc.Id);

                b.HasOne(sc => sc.Job)
                 .WithMany()
                 .HasForeignKey(sc => sc.JobId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(sc => sc.JobSeeker)
                 .WithMany()
                 .HasForeignKey(sc => sc.JobSeekerId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(sc => sc.Recruiter)
                 .WithMany()
                 .HasForeignKey(sc => sc.RecruiterId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(sc => new { sc.JobId, sc.JobSeekerId }).IsUnique();
            });

            // EmailVerification - one-to-many with User
            modelBuilder.Entity<EmailVerification>(b =>
            {
                b.HasOne(e => e.User)
                 .WithMany()
                 .HasForeignKey(e => e.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // PasswordReset - one-to-many with User
            modelBuilder.Entity<PasswordReset>(b =>
            {
                b.HasOne(p => p.User)
                 .WithMany()
                 .HasForeignKey(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // JobTitle - Reference table
            modelBuilder.Entity<JobTitle>(b =>
            {
                b.ToTable("JobTitle");
                b.HasKey(jt => jt.Id);
                b.HasIndex(jt => jt.TitleEn).IsUnique();
                
                // Store JobTitleRoleFamily enum as int in database
                b.Property(jt => jt.RoleFamily)
                 .HasConversion<int>();
            });

            // Country - Reference table
            modelBuilder.Entity<Country>(b =>
            {
                b.ToTable("Country");
                b.HasKey(c => c.Id);
                b.HasIndex(c => c.NameEn).HasDatabaseName("IX_Country_Name");
            });

            // City - Reference table
            modelBuilder.Entity<City>(b =>
            {
                b.ToTable("City");
                b.HasKey(c => c.Id);
                b.HasIndex(c => c.NameEn).HasDatabaseName("IX_City_Name");
                b.HasIndex(c => c.CountryId).HasDatabaseName("IX_City_CountryId");
                
                b.HasOne(c => c.Country)
                 .WithMany(country => country.Cities)
                 .HasForeignKey(c => c.CountryId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Language - Reference table
            modelBuilder.Entity<Language>(b =>
            {
                b.ToTable("Language");
                b.HasKey(l => l.Id);
                b.HasIndex(l => l.Code).IsUnique();
            });

            // FieldOfStudy - Reference table
            modelBuilder.Entity<FieldOfStudy>(b =>
            {
                b.ToTable("FieldOfStudy");
                b.HasKey(f => f.Id);
                b.HasIndex(f => f.NameEn).IsUnique();
            });

            // JobSeeker-Location relationships
            modelBuilder.Entity<JobSeeker>(b =>
            {
                b.HasOne(j => j.Country)
                 .WithMany()
                 .HasForeignKey(j => j.CountryId)
                 .OnDelete(DeleteBehavior.Restrict);
                 
                b.HasOne(j => j.City)
                 .WithMany()
                 .HasForeignKey(j => j.CityId)
                 .OnDelete(DeleteBehavior.Restrict);
                
                b.HasOne(j => j.JobTitle)
                 .WithMany()
                 .HasForeignKey(j => j.JobTitleId)
                 .OnDelete(DeleteBehavior.Restrict);
                
                b.HasOne(j => j.FirstLanguage)
                 .WithMany()
                 .HasForeignKey(j => j.FirstLanguageId)
                 .OnDelete(DeleteBehavior.Restrict);
                
                b.HasOne(j => j.SecondLanguage)
                 .WithMany()
                 .HasForeignKey(j => j.SecondLanguageId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed reference data using dedicated seed classes
            modelBuilder.Entity<JobTitle>().HasData(JobTitleSeed.GetJobTitles());

            // ============= Assessment V2 Configuration =============
            modelBuilder.Entity<AssessmentQuestionV2>(b =>
            {
                b.ToTable("AssessmentQuestionsV2");
                b.HasKey(q => q.Id);
                b.HasIndex(q => new { q.SkillId, q.Difficulty, q.IsActive });

                b.HasOne(q => q.Skill)
                 .WithMany()
                 .HasForeignKey(q => q.SkillId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AssessmentAttemptV2>(b =>
            {
                b.ToTable("AssessmentAttemptsV2");
                b.HasKey(a => a.Id);
                b.Property(a => a.OverallScore).HasPrecision(5, 2);
                b.Property(a => a.TechnicalScore).HasPrecision(5, 2);
                b.Property(a => a.SoftSkillsScore).HasPrecision(5, 2);
                b.HasIndex(a => new { a.JobSeekerId, a.Status, a.IsActive });

                b.HasOne(a => a.JobSeeker)
                 .WithMany()
                 .HasForeignKey(a => a.JobSeekerId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(a => a.JobTitle)
                 .WithMany()
                 .HasForeignKey(a => a.JobTitleId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AssessmentAnswerV2>(b =>
            {
                b.ToTable("AssessmentAnswersV2");
                b.HasKey(a => a.Id);
                b.HasIndex(a => new { a.AssessmentAttemptId, a.QuestionId }).IsUnique();
            });

            // ProfileView - engagement analytics
            modelBuilder.Entity<ProfileView>(b =>
            {
                b.ToTable("ProfileView");
                b.HasKey(pv => pv.Id);

                b.HasOne(pv => pv.JobSeeker)
                 .WithMany()
                 .HasForeignKey(pv => pv.JobSeekerId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(pv => pv.ViewerRecruiter)
                 .WithMany()
                 .HasForeignKey(pv => pv.ViewerRecruiterId)
                 .OnDelete(DeleteBehavior.NoAction);

                // Composite index for fast weekly aggregation queries
                b.HasIndex(pv => new { pv.JobSeekerId, pv.ViewedAt, pv.ViewType })
                 .HasDatabaseName("IX_ProfileView_JobSeeker_Date_Type");

                // Index for deduplication checks (same recruiter viewing same profile)
                b.HasIndex(pv => new { pv.JobSeekerId, pv.ViewerRecruiterId, pv.ViewType, pv.ViewedAt })
                 .HasDatabaseName("IX_ProfileView_Dedup");
            });
        }
    }
}
