-- ============================================================
-- Seed Test Data for Recruiter Candidates Page
-- Run this AFTER the app has started at least once (so reference
-- tables like Countries, Cities, Skills, JobTitles are seeded).
--
-- Usage: Execute in SQL Server Management Studio or sqlcmd
--   sqlcmd -S (localdb)\mssqllocaldb -d RecruitmentPlatformDb -i seed-test-data.sql
--
-- Test accounts:
--   Recruiter:  recruiter@test.com  / TestPass123
--   Candidate:  ahmed.ali@test.com / TestPass123
-- ============================================================

-- Prevent duplicate inserts
IF EXISTS (SELECT 1 FROM [User] WHERE Email = 'recruiter@test.com')
BEGIN
    PRINT 'Seed data already exists. Skipping.';
    RETURN;
END

-- ─────────────────────────────────────────────
-- 1. Create Recruiter User
-- Password: TestPass123  (BCrypt hash)
-- ─────────────────────────────────────────────
DECLARE @RecruiterUserId INT;
INSERT INTO [User] (FirstName, LastName, Email, PasswordHash, AuthProvider, AccountType, IsEmailVerified, IsActive, FailedLoginAttempts, ProfileCompletionStep, CreatedAt, UpdatedAt)
OUTPUT INSERTED.Id
VALUES ('Test', 'Recruiter', 'recruiter@test.com', '$2b$10$3pXtvubCfI6a7YKv82sH5.ugMHY3.R0DGngX2Vb4d2o0Ksj2XS.7.', 1, 'Recruiter', 1, 1, 0, 0, GETUTCDATE(), GETUTCDATE());

SET @RecruiterUserId = SCOPE_IDENTITY();

-- ─────────────────────────────────────────────
-- 2. Create Recruiter Company Profile
-- ─────────────────────────────────────────────
DECLARE @RecruiterId INT;
INSERT INTO Recruiters (UserId, CompanyName, CompanySize, Industry, CountryId, CityId, CreatedAt, UpdatedAt)
OUTPUT INSERTED.Id
VALUES (@RecruiterUserId, 'TechCorp Solutions', '50-200', 'Technology', 65, 3223, GETUTCDATE(), GETUTCDATE());

SET @RecruiterId = SCOPE_IDENTITY();

-- ─────────────────────────────────────────────
-- 3. Create Job Seeker (Candidate) User
-- Password: TestPass123  (BCrypt hash)
-- ─────────────────────────────────────────────
DECLARE @CandidateUserId INT;
INSERT INTO [User] (FirstName, LastName, Email, PasswordHash, AuthProvider, AccountType, IsEmailVerified, IsActive, FailedLoginAttempts, ProfileCompletionStep, CreatedAt, UpdatedAt)
OUTPUT INSERTED.Id
VALUES ('Ahmed', 'Ali', 'ahmed.ali@test.com', '$2b$10$3pXtvubCfI6a7YKv82sH5.ugMHY3.R0DGngX2Vb4d2o0Ksj2XS.7.', 1, 'JobSeeker', 1, 1, 0, 4, GETUTCDATE(), GETUTCDATE());

SET @CandidateUserId = SCOPE_IDENTITY();

-- ─────────────────────────────────────────────
-- 4. Create Job Seeker Profile
-- ─────────────────────────────────────────────
DECLARE @CandidateId INT;
INSERT INTO JobSeekers (
    UserId, JobTitleId, YearsOfExperience, CountryId, CityId, PhoneNumber,
    FirstLanguageId, FirstLanguageProficiency, SecondLanguageId, SecondLanguageProficiency,
    Bio, WorkPreferences, DesiredEmploymentTypes,
    CurrentAssessmentScore, LastAssessmentDate,
    CreatedAt, UpdatedAt
)
OUTPUT INSERTED.Id
VALUES (
    @CandidateUserId,
    2,              -- JobTitleId: Frontend Developer
    4,              -- YearsOfExperience
    65,             -- CountryId: Egypt
    3223,           -- CityId: Cairo
    '+201234567890',
    13,             -- FirstLanguageId: English
    'Native',       -- FirstLanguageProficiency: stored as string
    1,              -- SecondLanguageId: Arabic
    'Native',       -- SecondLanguageProficiency: stored as string
    'Passionate frontend developer with 4 years of experience building modern, responsive web applications using React, TypeScript, and Tailwind CSS. Strong focus on clean code, accessibility, and performance optimization.',
    '["Remote","Hybrid"]',
    '["FullTime"]',
    88.50,
    DATEADD(DAY, -5, GETUTCDATE()),
    GETUTCDATE(),
    GETUTCDATE()
);

SET @CandidateId = SCOPE_IDENTITY();

-- ─────────────────────────────────────────────
-- 5. Add Skills to Candidate
--    Skill IDs: 2=JavaScript, 3=TypeScript, 38=HTML/CSS,
--    39=React, 48=Tailwind CSS, 49=Redux
-- ─────────────────────────────────────────────
INSERT INTO JobSeekerSkills (JobSeekerId, SkillId, Source) VALUES
(@CandidateId, 2,  'Self'),
(@CandidateId, 3,  'Self'),
(@CandidateId, 38, 'Self'),
(@CandidateId, 39, 'Self'),
(@CandidateId, 48, 'Self'),
(@CandidateId, 49, 'Self');

-- ─────────────────────────────────────────────
-- 6. Add Work Experiences
-- ─────────────────────────────────────────────
INSERT INTO Experiences (JobSeekerId, JobTitle, CompanyName, CountryId, CityId, EmploymentType, StartDate, EndDate, IsCurrent, Responsibilities, DisplayOrder, CreatedAt, UpdatedAt, IsDeleted)
VALUES
(
    @CandidateId,
    'Frontend Developer',
    'TechStart Inc.',
    65,
    3223,
    1,          -- FullTime (stored as int)
    '2022-01-15',
    NULL,
    1,
    'Led the development of the main customer-facing dashboard using React and TypeScript. Implemented responsive designs with Tailwind CSS, improved page load performance by 40%, and mentored 2 junior developers.',
    1,
    GETUTCDATE(),
    GETUTCDATE(),
    0
),
(
    @CandidateId,
    'Junior Frontend Developer',
    'WebAgency Co.',
    65,
    3223,
    1,          -- FullTime (stored as int)
    '2020-06-01',
    '2022-01-10',
    0,
    'Built responsive landing pages and e-commerce interfaces for multiple clients. Worked with React, JavaScript, and CSS frameworks. Collaborated with designers to implement pixel-perfect UI components.',
    2,
    GETUTCDATE(),
    GETUTCDATE(),
    0
);

-- ─────────────────────────────────────────────
-- 7. Add Education
-- ─────────────────────────────────────────────
INSERT INTO Educations (JobSeekerId, Institution, Degree, FieldOfStudyId, FieldOfStudyName, GradeOrGPA, StartDate, EndDate, IsCurrent, DisplayOrder, CreatedAt, UpdatedAt, IsDeleted)
VALUES (
    @CandidateId,
    'Cairo University',
    'Bachelor',         -- Degree: stored as string
    1,                  -- FieldOfStudyId: Computer Science
    NULL,
    '3.7/4.0',
    '2016-09-01',
    '2020-06-30',
    0,
    1,
    GETUTCDATE(),
    GETUTCDATE(),
    0
);

-- ─────────────────────────────────────────────
-- 8. Add Projects
-- ─────────────────────────────────────────────
INSERT INTO Projects (JobSeekerId, Title, TechnologiesUsed, Description, ProjectLink, DisplayOrder, IsDeleted, CreatedAt, UpdatedAt)
VALUES
(
    @CandidateId,
    'E-Commerce Dashboard',
    'React, TypeScript, Tailwind CSS, Chart.js',
    'A comprehensive admin dashboard for e-commerce platforms featuring real-time analytics, inventory management, and order tracking. Built with React and TypeScript for type safety.',
    'https://github.com/ahmed-ali/ecommerce-dashboard',
    1,
    0,
    GETUTCDATE(),
    GETUTCDATE()
),
(
    @CandidateId,
    'Task Management App',
    'React, Redux, Firebase, Material UI',
    'A collaborative task management application with real-time updates, drag-and-drop boards, and team workspaces. Uses Firebase for backend and Redux for state management.',
    'https://github.com/ahmed-ali/task-manager',
    2,
    0,
    GETUTCDATE(),
    GETUTCDATE()
);

-- ─────────────────────────────────────────────
-- 9. Add Social Links
-- ─────────────────────────────────────────────
INSERT INTO SocialAccounts (JobSeekerId, LinkedIn, Github, Behance, Dribbble, PersonalWebsite, CreatedAt, UpdatedAt)
VALUES (
    @CandidateId,
    'https://linkedin.com/in/ahmed-ali',
    'https://github.com/ahmed-ali',
    NULL,
    NULL,
    'https://ahmed-ali.dev',
    GETUTCDATE(),
    GETUTCDATE()
);

-- ─────────────────────────────────────────────
-- 10. Create Job Posting
-- ─────────────────────────────────────────────
DECLARE @JobId INT;
INSERT INTO Jobs (RecruiterId, Title, JobTitleId, Description, Requirements, EmploymentType, MinYearsOfExperience, WorkModel, CountryId, CityId, IsActive, PostedAt, UpdatedAt)
OUTPUT INSERTED.Id
VALUES (
    @RecruiterId,
    'Frontend Developer',
    2,              -- JobTitleId: Frontend Developer
    'We are looking for a skilled Frontend Developer to join our growing engineering team. You will work on building and maintaining our customer-facing web applications, collaborating closely with designers and backend engineers to deliver exceptional user experiences.',
    '3+ years of experience with React and modern JavaScript. Proficiency in TypeScript, HTML5, and CSS3. Experience with responsive design and CSS frameworks like Tailwind CSS. Strong understanding of web performance optimization. Familiarity with version control (Git) and CI/CD pipelines.',
    'FullTime',     -- EmploymentType: stored as string
    2,              -- MinYearsOfExperience
    'Hybrid',       -- WorkModel: stored as string
    65,             -- CountryId: Egypt
    3223,           -- CityId: Cairo
    1,              -- IsActive
    GETUTCDATE(),
    GETUTCDATE()
);

SET @JobId = SCOPE_IDENTITY();

-- ─────────────────────────────────────────────
-- 11. Add Skills to Job
--    Skill IDs: 2=JavaScript, 3=TypeScript, 38=HTML/CSS, 39=React
-- ─────────────────────────────────────────────
INSERT INTO JobSkills (JobId, SkillId) VALUES
(@JobId, 2),
(@JobId, 3),
(@JobId, 38),
(@JobId, 39);

-- ─────────────────────────────────────────────
-- 12. Seed Recommendation (AI match result)
-- ─────────────────────────────────────────────
INSERT INTO Recommendations (JobId, JobSeekerId, MatchScore, AiReasoning, MatchedSkillsJson, MissingSkillsJson, GeneratedAt)
VALUES (
    @JobId,
    @CandidateId,
    88.50,
    'Strong match: Candidate has extensive frontend experience with React, TypeScript, and modern CSS frameworks. Assessment score of 88.5% demonstrates solid technical proficiency. Work history shows progressive growth from junior to mid-level frontend developer.',
    '["JavaScript","TypeScript","React","HTML/CSS"]',
    '[]',
    GETUTCDATE()
);

PRINT '==========================================';
PRINT 'Seed data created successfully!';
PRINT '';
PRINT 'Recruiter login:  recruiter@test.com  / TestPass123';
PRINT 'Candidate login:  ahmed.ali@test.com / TestPass123';
PRINT '';
PRINT 'Recruiter ID:     ' + CAST(@RecruiterId AS VARCHAR);
PRINT 'Candidate ID:     ' + CAST(@CandidateId AS VARCHAR);
PRINT 'Job ID:           ' + CAST(@JobId AS VARCHAR);
PRINT '==========================================';
