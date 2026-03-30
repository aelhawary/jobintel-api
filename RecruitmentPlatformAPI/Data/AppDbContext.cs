using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data.Seed;
using RecruitmentPlatformAPI.Models.Identity;
using RecruitmentPlatformAPI.Models.JobSeeker;
using RecruitmentPlatformAPI.Models.Recruiter;
using RecruitmentPlatformAPI.Models.Reference;
using RecruitmentPlatformAPI.Models.Jobs;
using RecruitmentPlatformAPI.Models.Assessment;

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
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Language> Languages { get; set; }
        
        // Assessment Quiz Models
        public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }
        public DbSet<AssessmentAttempt> AssessmentAttempts { get; set; }
        public DbSet<AssessmentAnswer> AssessmentAnswers { get; set; }

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
                
                // Precision for assessment score
                b.Property(j => j.CurrentAssessmentScore).HasPrecision(5, 2);
            });

            // Recruiter - one-to-one with User
            modelBuilder.Entity<Recruiter>(b =>
            {
                b.HasOne(r => r.User)
                 .WithOne()
                 .HasForeignKey<Recruiter>(r => r.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
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
                
                // Ensure only one active (non-deleted) CV per job seeker using unique composite index.
                // Application logic enforces active-record behavior consistently.
                b.HasIndex(r => new { r.JobSeekerId, r.IsDeleted })
                 .IsUnique()
                 .HasDatabaseName("IX_Resume_JobSeekerId_IsDeleted_Unique");
                
                // Index for querying non-deleted resumes
                b.HasIndex(r => new { r.JobSeekerId, r.IsDeleted })
                 .HasDatabaseName("IX_Resume_JobSeekerId_IsDeleted");
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

                // Store EmploymentType enum as string in database (matches existing nvarchar(50) column)
                b.Property(j => j.EmploymentType)
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
                 .WithMany()
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
                b.HasIndex(jt => jt.Title).IsUnique();
                
                // Store JobTitleRoleFamily enum as int in database
                b.Property(jt => jt.RoleFamily)
                 .HasConversion<int>();
            });

            // Country - Reference table
            modelBuilder.Entity<Country>(b =>
            {
                b.ToTable("Country");
                b.HasKey(c => c.Id);
                b.HasIndex(c => c.IsoCode).IsUnique();
            });

            // Language - Reference table
            modelBuilder.Entity<Language>(b =>
            {
                b.ToTable("Language");
                b.HasKey(l => l.Id);
                b.HasIndex(l => l.IsoCode).IsUnique();
            });

            // JobSeeker-Country relationship
            modelBuilder.Entity<JobSeeker>(b =>
            {
                b.HasOne(j => j.Country)
                 .WithMany()
                 .HasForeignKey(j => j.CountryId)
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
            modelBuilder.Entity<Country>().HasData(CountrySeed.GetCountries());
            modelBuilder.Entity<Language>().HasData(LanguageSeed.GetLanguages());
            modelBuilder.Entity<Skill>().HasData(SkillSeed.GetSkills());
            modelBuilder.Entity<AssessmentQuestion>().HasData(AssessmentQuestionSeed.GetQuestions());

            // ============= Assessment Quiz Configuration =============
            
            // AssessmentQuestion
            modelBuilder.Entity<AssessmentQuestion>(b =>
            {
                b.ToTable("AssessmentQuestion");
                b.HasKey(q => q.Id);
                
                // Store enums as int in database
                b.Property(q => q.Category)
                 .HasConversion<int>();
                
                b.Property(q => q.Difficulty)
                 .HasConversion<int>();
                
                b.Property(q => q.SeniorityLevel)
                 .HasConversion<int>();
                
                b.Property(q => q.RoleFamily)
                 .HasConversion<int>();
                
                // Index for efficient question filtering by role family, category, difficulty, seniority
                b.HasIndex(q => new { q.RoleFamily, q.Category, q.Difficulty, q.SeniorityLevel, q.IsActive })
                 .HasDatabaseName("IX_AssessmentQuestion_Filtering");
                
                // Relationship with Skill (nullable for soft skills)
                b.HasOne(q => q.Skill)
                 .WithMany()
                 .HasForeignKey(q => q.SkillId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
            
            // AssessmentAttempt
            modelBuilder.Entity<AssessmentAttempt>(b =>
            {
                b.ToTable("AssessmentAttempt");
                b.HasKey(a => a.Id);
                
                // Store enum as int
                b.Property(a => a.Status)
                 .HasConversion<int>();
                
                // Precision for scores
                b.Property(a => a.OverallScore).HasPrecision(5, 2);
                b.Property(a => a.TechnicalScore).HasPrecision(5, 2);
                b.Property(a => a.SoftSkillsScore).HasPrecision(5, 2);
                
                // Relationships
                b.HasOne(a => a.JobSeeker)
                 .WithMany(j => j.AssessmentAttempts)
                 .HasForeignKey(a => a.JobSeekerId)
                 .OnDelete(DeleteBehavior.Cascade);
                
                b.HasOne(a => a.JobTitle)
                 .WithMany()
                 .HasForeignKey(a => a.JobTitleId)
                 .OnDelete(DeleteBehavior.Restrict);
                
                // Indexes for efficient queries
                b.HasIndex(a => new { a.JobSeekerId, a.IsActive })
                 .HasDatabaseName("IX_AssessmentAttempt_JobSeeker_Active");
                
                b.HasIndex(a => new { a.JobSeekerId, a.Status, a.StartedAt })
                 .HasDatabaseName("IX_AssessmentAttempt_JobSeeker_Status");
                
                // Ensure only one in-progress assessment per job seeker at a time.
                // Application logic enforces the in-progress constraint.
                b.HasIndex(a => new { a.JobSeekerId, a.Status })
                 .HasDatabaseName("IX_AssessmentAttempt_JobSeeker_Status_Unique");
            });
            
            // AssessmentAnswer
            modelBuilder.Entity<AssessmentAnswer>(b =>
            {
                b.ToTable("AssessmentAnswer");
                b.HasKey(a => a.Id);
                
                // Relationships
                b.HasOne(a => a.AssessmentAttempt)
                 .WithMany(at => at.Answers)
                 .HasForeignKey(a => a.AssessmentAttemptId)
                 .OnDelete(DeleteBehavior.Cascade);
                
                b.HasOne(a => a.Question)
                 .WithMany()
                 .HasForeignKey(a => a.QuestionId)
                 .OnDelete(DeleteBehavior.Restrict);
                
                // Ensure one answer per question per attempt
                b.HasIndex(a => new { a.AssessmentAttemptId, a.QuestionId })
                 .IsUnique()
                 .HasDatabaseName("IX_AssessmentAnswer_Attempt_Question");
            });
        }
    }
}
