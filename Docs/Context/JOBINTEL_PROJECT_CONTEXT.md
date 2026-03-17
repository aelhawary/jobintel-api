# JOBINTEL_PROJECT_CONTEXT

## 1. Project Overview

### What the project is
JobIntel is a backend API for a recruitment platform that supports two user roles:
- Job seekers
- Recruiters

### Main idea and goal
The system provides:
- Full authentication and account security flows (email/password + Google OAuth)
- Profile completion workflows for both account types
- Job posting management for recruiters
- Reference data APIs for countries/languages/skills/job titles
- Storage for resumes, certificates, and profile pictures

### Type of application
- ASP.NET Core Web API (REST)
- Monolithic backend service with modular folders
- Database-first behavior via EF Core migrations and seed data

---

## 2. Tech Stack

### Core backend
- Framework: ASP.NET Core 9.0
- Language: C# (net9.0)
- API style: REST + JSON

### Data layer
- ORM: Entity Framework Core 9 (SQL Server provider)
- Database: SQL Server (LocalDB default in development)
- Migrations: EF Core migration set in `Data/Migrations`

### Auth and security
- JWT Bearer authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- JWT signing algorithm: HMAC-SHA256
- Password hashing: BCrypt (`BCrypt.Net-Next`)
- Google OAuth token validation: `Google.Apis.Auth`
- Email verification + password reset tokens

### API and integration
- Swagger/OpenAPI: `Swashbuckle.AspNetCore`
- Email service: MailKit (SMTP)
- CORS policy allowing localhost origins in development

### Serialization and conventions
- JSON camelCase policy
- Enums serialized as strings in API responses (global `JsonStringEnumConverter`)

---

## 3. System Architecture

### Architectural style
Layered/N-tier architecture:
- Controllers (HTTP layer)
- Services (business logic)
- EF Core DbContext (data access)
- SQL Server (persistence)

No repository pattern is used; services access `AppDbContext` directly.

### Folder structure and purpose
- `Controllers/`: API endpoints by module
- `Services/Auth`: authentication, token, email services
- `Services/JobSeeker`: profile and wizard-related job seeker business logic
- `Services/Recruiter`: recruiter profile and recruiter job posting logic
- `DTOs/`: request/response contracts grouped by domain
- `Models/`: persistence entities grouped by domain
- `Data/AppDbContext.cs`: DbSets + Fluent API relations/indexes/constraints/seeding
- `Data/Seed`: static reference seed data
- `Data/Migrations`: schema evolution (currently initial migration)
- `Configuration/`: strongly typed settings classes
- `Enums/`: domain enums (auth, profile, jobs, assessment)

### Request flow
Typical flow:
1. Controller receives HTTP request and validates `ModelState`
2. Controller extracts current user ID from JWT claims (where needed)
3. Controller calls service interface
4. Service performs business rules + EF Core operations
5. DbContext persists changes to SQL Server
6. Service returns DTO/result wrapper
7. Controller translates result into HTTP response

---

## 4. Database Design

### Main entities
Identity:
- `User`
- `EmailVerification`
- `PasswordReset`

Job seeker:
- `JobSeeker`
- `Experience`
- `Education`
- `Project`
- `Resume`
- `Certificate`
- `SocialAccount`
- `JobSeekerSkill`

Recruiter/jobs:
- `Recruiter`
- `Job`
- `JobSkill`
- `Recommendation`

Assessment:
- `AssessmentQuestion`
- `AssessmentAttempt`
- `AssessmentAnswer`

Reference:
- `Country`
- `Language`
- `JobTitle`
- `Skill`

### Important relationships
- `User` 1:1 `JobSeeker`
- `User` 1:1 `Recruiter`
- `JobSeeker` 1:M `Experience`, `Education`, `Project`, `Resume`, `Certificate`
- `JobSeeker` 1:1 `SocialAccount`
- `JobSeeker` M:N `Skill` through `JobSeekerSkill`
- `Recruiter` 1:M `Job`
- `Job` M:N `Skill` through `JobSkill`
- `Job` M:N `JobSeeker` through `Recommendation` (with score metadata)
- `JobSeeker` 1:M `AssessmentAttempt`
- `AssessmentAttempt` 1:M `AssessmentAnswer`

### Key fields and purpose
- `User.ProfileCompletionStep`: wizard progress tracker
- `User.AuthProvider`, `User.ProviderUserId`: email vs Google auth linkage
- `User.FailedLoginAttempts`, `User.LockoutEnd`: lockout security
- `Resume.IsDeleted`, `Experience.IsDeleted`, etc.: soft delete pattern in several modules
- `AssessmentAttempt.OverallScore`, `ScoreExpiresAt`: scoring lifecycle support

### Notable constraints and indexes
- Unique email index on `User.Email`
- Check constraints:
  - Education end date >= start date
  - Experience end date >= start date
  - Certificate expiration >= issue date
- Filtered unique index: one active resume per job seeker
- Filtered unique index: one in-progress assessment per job seeker
- Unique composite indexes on many-to-many junctions (`JobSkill`, `JobSeekerSkill`)

### Seeded reference data
- Job titles (~90)
- Countries (~65)
- Languages (~50)
- Skills (~50)

---

## 5. API Structure

Base API is controller-driven. Major modules are below.

### Authentication module (`AuthController`)
Purpose:
- Registration/login/security flows

Endpoints:
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/google`
- `POST /api/auth/verify-email`
- `POST /api/auth/resend-verification`
- `POST /api/auth/forgot-password`
- `POST /api/auth/validate-reset-token`
- `POST /api/auth/reset-password`
- `GET /api/auth/me`

Key DTOs:
- Request: `RegisterDto`, `LoginDto`, `GoogleAuthDto`, `EmailVerificationDto`, `ResendVerificationDto`, `ForgotPasswordDto`, `ValidateResetTokenDto`, `ResetPasswordDto`
- Response: `AuthResponseDto`, `CurrentUserResponse`

### Job seeker profile core module (`JobSeekerController`)
Purpose:
- Personal info + wizard status + explicit step advancement + profile picture + job titles

Endpoints:
- `POST /api/jobseeker/personal-info`
- `GET /api/jobseeker/personal-info`
- `GET /api/jobseeker/wizard-status`
- `POST /api/jobseeker/wizard/advance/{stepNumber}`
- `GET /api/jobseeker/job-titles`
- `POST /api/jobseeker/picture`
- `GET /api/jobseeker/picture/info`
- `GET /api/jobseeker/picture`
- `DELETE /api/jobseeker/picture`
- `GET /api/jobseeker/picture/exists`

DTOs:
- Request: `PersonalInfoRequestDto`
- Response: `PersonalInfoDto`, `WizardStatusDto`, `ProfileResponseDto`, `ProfilePictureResponseDto`, `ProfilePictureUploadResultDto`

### Experience module (`ExperienceController`)
Purpose:
- CRUD + reorder + existence check for experiences

Endpoints:
- `GET /api/jobseeker/experience`
- `GET /api/jobseeker/experience/{id}`
- `POST /api/jobseeker/experience`
- `PUT /api/jobseeker/experience/{id}`
- `DELETE /api/jobseeker/experience/{id}`
- `POST /api/jobseeker/experience/reorder`
- `GET /api/jobseeker/experience/exists`

DTOs:
- Request: `ExperienceRequestDto`
- Response: `ExperienceResponseDto`, `ExperienceListResponseDto`

### Education module (`EducationController`)
Purpose:
- CRUD + reorder + existence check for education entries

Endpoints:
- `GET /api/jobseeker/education`
- `GET /api/jobseeker/education/{id}`
- `POST /api/jobseeker/education`
- `PUT /api/jobseeker/education/{id}`
- `DELETE /api/jobseeker/education/{id}`
- `POST /api/jobseeker/education/reorder`
- `GET /api/jobseeker/education/exists`

DTOs:
- Request: `EducationRequestDto`
- Response: `EducationResponseDto`, `EducationListResponseDto`

### Projects module (`ProjectsController`)
Purpose:
- CRUD for projects with soft delete and display order behavior

Endpoints:
- `GET /api/jobseeker/projects`
- `POST /api/jobseeker/projects`
- `PUT /api/jobseeker/projects/{projectId}`
- `DELETE /api/jobseeker/projects/{projectId}`

DTOs:
- Request: `AddProjectDto`, `UpdateProjectDto`
- Response: `ProjectDto`, `ProjectResponseDto`

### Skills module (`JobSeekerSkillsController`)
Purpose:
- Replace/clear/get assigned skills + list available skill options

Endpoints:
- `GET /api/jobseeker/skills`
- `PUT /api/jobseeker/skills`
- `DELETE /api/jobseeker/skills`
- `GET /api/jobseeker/skills/available`

DTOs:
- Request: `UpdateSkillsRequestDto`
- Response: `SkillsResponseDto`, `SkillDto`

### Social accounts module (`SocialAccountsController`)
Purpose:
- Upsert/get/delete social links

Endpoints:
- `PUT /api/jobseeker/social-accounts`
- `GET /api/jobseeker/social-accounts`
- `DELETE /api/jobseeker/social-accounts`

DTOs:
- Request: `UpdateSocialAccountDto`
- Response: `SocialAccountResponseDto`, `SocialAccountDto`

### Resume module (`ResumeController`)
Purpose:
- Upload/download/delete/check job seeker resume

Endpoints:
- `POST /api/jobseeker/resume/upload`
- `GET /api/jobseeker/resume`
- `GET /api/jobseeker/resume/download`
- `DELETE /api/jobseeker/resume`
- `GET /api/jobseeker/resume/exists`

DTOs:
- Response wrappers: `ResumeResponseDto`, `ResumeDto`, `FileValidationResult`

### Certificates module (`CertificatesController`)
Purpose:
- CRUD + optional file attachment/download for certificates

Endpoints:
- `GET /api/jobseeker/certificates`
- `GET /api/jobseeker/certificates/{id}`
- `POST /api/jobseeker/certificates` (multipart/form-data)
- `PUT /api/jobseeker/certificates/{id}` (multipart/form-data)
- `DELETE /api/jobseeker/certificates/{id}`
- `GET /api/jobseeker/certificates/{id}/download`

DTOs:
- Request: `CertificateRequestDto`
- Response: `CertificateDto`, `CertificateResponseDto`, `CertificateListResponseDto`

### Recruiter profile module (`RecruiterController`)
Purpose:
- Save/retrieve recruiter company profile + recruiter wizard state + recruiter profile picture

Endpoints:
- `POST /api/recruiter/company-info`
- `GET /api/recruiter/company-info`
- `GET /api/recruiter/wizard-status`
- `POST /api/recruiter/wizard/advance/{stepNumber}`
- `GET /api/recruiter/industries`
- `GET /api/recruiter/company-sizes`
- `POST /api/recruiter/picture`
- `GET /api/recruiter/picture/info`
- `GET /api/recruiter/picture`
- `DELETE /api/recruiter/picture`
- `GET /api/recruiter/picture/exists`

DTOs:
- Request: `RecruiterCompanyInfoRequestDto`
- Response: `RecruiterCompanyInfoDto`, `IndustryDto`, `CompanySizeDto`, wizard/profile-picture DTOs reused from JobSeeker DTO namespace

### Recruiter jobs module (`JobsController`)
Purpose:
- Recruiter-side job posting CRUD and recruiter-owned job retrieval

Endpoints:
- `GET /api/jobs/skills`
- `GET /api/jobs`
- `GET /api/jobs/{id}`
- `POST /api/jobs`
- `PUT /api/jobs/{id}`
- `PATCH /api/jobs/{id}/deactivate`
- `PATCH /api/jobs/{id}/reactivate`
- `DELETE /api/jobs/{id}`

DTOs:
- Request: `JobRequestDto`
- Response: `JobResponseDto`, `JobListResponseDto`, `JobSkillDto`, `SkillOptionDto`

### Locations/reference module (`LocationsController`)
Purpose:
- Reference lookup for countries and languages with simple localization switch

Endpoints:
- `GET /api/locations/countries?lang=en|ar`
- `GET /api/locations/languages?lang=en|ar`

DTOs:
- `CountryDto`, `LanguageDto`

---

## 6. Implemented Modules (Completed Features)

Based on current code, these are implemented end-to-end (controller + service + persistence):
- Authentication and account lifecycle (register, verify, login, Google auth, reset password, current-user introspection)
- Account lockout and lockout email notification flow
- Job seeker wizard core (personal info + wizard state API)
- Explicit wizard advancement endpoints (job seeker and recruiter)
- Job seeker profile picture upload/retrieve/delete
- Resume upload/retrieve/download/delete/exists
- Experience module CRUD + reorder + soft delete
- Education module CRUD + reorder + soft delete
- Projects module CRUD + soft delete + reorder behavior
- Skills assignment and lookup module
- Social accounts module
- Certificates module with optional file upload and download
- Recruiter company profile module
- Recruiter job posting management module (create/read/update/deactivate/reactivate/delete)
- Reference data endpoints for countries/languages

---

## 7. In-Progress Modules

These exist structurally but are not fully exposed as complete product features:
- Assessment module:
  - Entities exist (`AssessmentQuestion`, `AssessmentAttempt`, `AssessmentAnswer`)
  - Enum/config support exists (`AssessmentSettings`, related enums)
  - DbContext relationships/indexes exist
  - No assessment controllers/services are currently exposed in this codebase
- Recommendation/matching module:
  - `Recommendation` model/table exists
  - No controller/service implementing recommendation generation or retrieval
- Profile picture URL route consistency:
  - Service stores URL as `/api/profile/picture`, while actual controllers expose `/api/jobseeker/picture` and `/api/recruiter/picture`
  - Functionality still works through endpoint calls, but stored URL format appears inconsistent

---

## 8. Not Yet Implemented / Missing Modules

Inferred from code absence:
- No candidate-facing job browsing/application module (no apply endpoints, no application entity)
- No assessment API endpoints (start attempt, submit answer, finish attempt)
- No recommendation API endpoints
- No admin/backoffice management module
- No custom middleware package in code for rate limiting despite `IpRateLimiting` config block in development settings
- No automated test project present in repository structure shown

Assumption note:
- Some features may be planned in docs, but classification above is strictly based on currently available backend code.

---

## 9. Special Logic or Important Business Rules

### Auth and security rules
- Email normalization (`trim + lowercase`) before identity operations
- Email verification code:
  - 6 digits
  - expires in 15 minutes
  - latest active code is used for validation
  - all prior unused codes are invalidated on resend
- Password reset token:
  - secure random token stored in DB
  - expires in 15 minutes
  - single-use and previous unused tokens are invalidated
- Login lockout:
  - 5 failed attempts => lock for 15 minutes
  - lockout applies to password login and Google auth
  - lockout counters reset after successful login/reset

### Wizard behavior rule
- Entity save operations and wizard advancement are intentionally decoupled
- Wizard step advancement now requires explicit `POST .../wizard/advance/{step}`

### Data integrity and ordering rules
- Soft delete pattern for multiple profile entities (`IsDeleted`, `DeletedAt`)
- Reorder APIs maintain display sequence for education/experience/projects/certificates
- Check constraints enforce date correctness at DB layer

### File handling rules
- Resume and certificate uploads are validated by extension/MIME (and for resume, file signature checks)
- Profile picture validates extension/MIME/magic bytes and size

### Ownership and role checks
- Recruiter-only job operations verify ownership via recruiter linkage
- Job seeker-only modules explicitly reject non-job seeker accounts

---

## 10. Profile Completion Wizard (Detailed Section)

### Step model currently implemented
Job seeker wizard is implemented as 4 steps in business logic:
- Step 1: Personal info (+ profile picture + resume endpoints available)
- Step 2: Experience and education
- Step 3: Projects
- Step 4: Skills, social accounts, certificates

Progress storage:
- `User.ProfileCompletionStep` integer

Status API:
- `GET /api/jobseeker/wizard-status`

Advance API:
- `POST /api/jobseeker/wizard/advance/{stepNumber}`

#### Step 1: Personal Info
Data stored:
- In `JobSeeker`: job title ID, years of experience, country ID, city, phone, language IDs/proficiency, bio
- In `User`: `ProfileCompletionStep` is raised to at least 1 after successful personal-info save

Related endpoints:
- `POST /api/jobseeker/personal-info`
- `GET /api/jobseeker/personal-info`
- Optional/related step assets:
  - `POST/GET/DELETE /api/jobseeker/picture...`
  - `POST/GET/DELETE /api/jobseeker/resume...`

Validation logic:
- FK validation for job title/country/languages
- second language must differ from first
- city and phone normalization in service
- standard data annotation validation in DTO/controller

Persistence logic:
- Upserts `JobSeeker` record (creates record for backward compatibility if missing)
- Updates `User.ProfileCompletionStep` when lower than step 1

#### Step 2: Experience and Education
Data stored:
- `Experience` rows (soft deletable)
- `Education` rows (soft deletable)

Related endpoints:
- Experience: `GET/POST/PUT/DELETE`, `reorder`, `exists`
- Education: `GET/POST/PUT/DELETE`, `reorder`, `exists`

Validation logic:
- Date range validation in service (`EndDate >= StartDate`, current flag handling)
- Additional check constraints at DB level

Persistence logic:
- CRUD in separate services
- `DisplayOrder` managed via reorder endpoints
- No implicit wizard step increment (by design)

#### Step 3: Projects
Data stored:
- `Project` rows with `DisplayOrder`, soft delete markers

Related endpoints:
- `GET /api/jobseeker/projects`
- `POST /api/jobseeker/projects`
- `PUT /api/jobseeker/projects/{id}`
- `DELETE /api/jobseeker/projects/{id}`

Validation logic:
- DTO validation for title, URL formats, max lengths
- Job seeker role checks in service

Persistence logic:
- Add sets next display order
- Delete is soft delete + reorder remaining active projects
- No implicit wizard step increment

#### Step 4: Skills, Social Accounts, Certificates
Data stored:
- `JobSeekerSkill` entries (replace-all model)
- `SocialAccount` single row per job seeker (upsert/delete)
- `Certificate` entries, optional file metadata, soft delete

Related endpoints:
- Skills: `GET`, `PUT`, `DELETE`, `GET /available`
- Social: `PUT`, `GET`, `DELETE`
- Certificates: `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}`, `GET/{id}/download`

Validation logic:
- Skill IDs validated against reference table
- Social URLs validated in DTO
- Certificate date checks and file extension/size validation

Persistence logic:
- Skills are replaced atomically by deleting old rows and inserting selected set
- Social account is upserted
- Certificate supports file replacement and soft deletion
- No implicit wizard step increment

### Whether current step tracking exists
Yes. It exists and is actively used via:
- `User.ProfileCompletionStep`
- wizard status endpoints
- explicit advance endpoints

### How it is implemented
- Step values are controlled in service layer
- Personal/company info save may move step to 1 if behind
- Other entity saves do not move steps
- Frontend must call `wizard/advance/{step}` explicitly when user clicks next/skip

### Missing/weak logic in wizard tracking
- No backend enforcement that required data is present before advancing to a step value
- No explicit "move back" API (forward-only style)
- JWT includes `ProfileCompletionStep` claim at token issue time, so claim may be stale until token refresh/login
- Comments/docstrings in some files still mention older step numbering (for example, step 6 wording for social links)

---

## 11. Coding Conventions

### Naming conventions
- Controllers: `<Module>Controller`
- Services: interface `I...Service` + concrete `...Service`
- DTO suffixes: `RequestDto`, `ResponseDto`, `ListResponseDto`
- Entity classes in `Models/*`

### DTO usage pattern
- Controllers receive request DTOs and return response wrappers/DTOs
- Generic wrappers in common layer: `ApiResponse<T>` and `ApiErrorResponse`
- Some modules use module-specific response wrappers (`ResumeResponseDto`, `SkillsResponseDto`, etc.)

### Dependency injection pattern
- All services registered as scoped in `Program.cs`
- Constructor injection used consistently

### Validation patterns
- Primary validation via DataAnnotations on DTOs
- Service-level validation for business rules (ownership, role, foreign key checks, date logic)
- Additional DB-level integrity via check constraints and unique indexes

### Data access style
- EF Core with direct `DbContext` use in services
- Manual mapping between entities and DTOs
- Frequent use of `AsNoTracking` for read-only queries

---

## 12. Current Development Status

### Finished
- Auth module with email+Google and security hardening
- Job seeker profile modules (personal info, experience, education, projects, skills, social, resume, certificates)
- Recruiter profile module
- Recruiter job CRUD module
- Reference endpoints and seed data
- Wizard tracking with explicit advancement endpoints

### Partially done
- Assessment domain modeled in DB and enums/config but no public API/service orchestration yet
- Recommendation domain modeled in DB but no generation/retrieval APIs
- Some docs/comments are outdated relative to current wizard design and endpoint behavior
- Route/URL consistency issues in a few generated file URLs (service-returned URLs vs controller routes)

### Still needs to be built
- Assessment APIs and business workflows
- Recommendation/matching APIs
- Candidate-side job discovery/application flow
- Admin/operations layer (if planned)
- Optional: stronger middleware strategy (rate limiting/log correlation/exception standardization)
- Optional: automated test projects and CI checks

---

## Assumptions and Inference Notes
- Module status labels (completed/in-progress/missing) are inferred strictly from code presence and endpoint/service availability in this repository.
- Planning statements in markdown docs were not treated as implementation evidence unless matching code exists.
- Where code comments conflict with implementation (for example, step counts), implementation behavior was treated as source of truth.
