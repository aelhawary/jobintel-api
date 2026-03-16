# Project Context - JobIntel Backend API

## 1. Project Overview

### What this project is
JobIntel is a backend REST API for a recruitment platform with two account types:
- Job seeker accounts
- Recruiter accounts

### Main goal
The system handles:
- Authentication and account security
- Profile completion workflows (wizard-based)
- Recruiter job posting management
- Profile assets (resume, certificates, profile picture)
- Lookup/reference data for UI forms

### Application type
- ASP.NET Core Web API (monolith)
- Layered service-oriented backend
- SQL Server + EF Core persistence

## 2. Tech Stack

### Backend and language
- ASP.NET Core 9.0
- C# (net9.0)

### Data
- Entity Framework Core 9
- SQL Server (LocalDB in development)
- EF Core migrations (currently one initial migration)

### Authentication and authorization
- JWT Bearer authentication
- JWT signed with HMAC-SHA256
- Google OAuth token validation via Google API library
- BCrypt password hashing

### External libraries and services
- MailKit for SMTP email sending
- Swashbuckle (Swagger/OpenAPI)

### Serialization and API behavior
- JSON camelCase policy
- Enums serialized as strings in API responses
- `[ApiController]` model validation behavior

## 3. System Architecture & Folder Structure

### Architectural style
The codebase follows a layered/N-tier style:
- Controller layer (HTTP/API contract)
- Service layer (business logic)
- Data access through EF Core `AppDbContext`
- SQL Server persistence

Repository pattern is not used. Services query `AppDbContext` directly.

### Request flow
Typical request flow:
1. Controller receives request
2. Controller validates model state and extracts authenticated user ID
3. Controller calls service interface
4. Service applies validation/business rules
5. Service uses `AppDbContext` for read/write
6. Service maps entities to response DTOs
7. Controller returns HTTP status and response DTO/wrapper

### Folder structure and purpose
- `RecruitmentPlatformAPI/Controllers`: API endpoint definitions
- `RecruitmentPlatformAPI/Services/Auth`: auth, token, email services
- `RecruitmentPlatformAPI/Services/JobSeeker`: job seeker profile domain services
- `RecruitmentPlatformAPI/Services/Recruiter`: recruiter/company and jobs services
- `RecruitmentPlatformAPI/DTOs`: API contracts by module
- `RecruitmentPlatformAPI/Models`: EF entities by domain
- `RecruitmentPlatformAPI/Data/AppDbContext.cs`: DbSets, relations, constraints, indexes, seed wiring
- `RecruitmentPlatformAPI/Data/Migrations`: schema migration history
- `RecruitmentPlatformAPI/Data/Seed`: reference seed data
- `RecruitmentPlatformAPI/Configuration`: strongly typed settings classes
- `RecruitmentPlatformAPI/Enums`: domain enums
- `RecruitmentPlatformAPI/Program.cs`: DI, auth, CORS, Swagger, middleware pipeline

### Middleware and pipeline
In `Program.cs` pipeline order is:
- `UseHttpsRedirection`
- `UseCors("AllowFrontend")`
- `UseAuthentication`
- `UseAuthorization`
- `MapControllers`

No custom middleware class exists in the repository.

## 4. Database Design

### Main entities
Identity/security:
- `User`
- `EmailVerification`
- `PasswordReset`

Job seeker profile domain:
- `JobSeeker`
- `Experience`
- `Education`
- `Project`
- `Resume`
- `SocialAccount`
- `Certificate`
- `JobSeekerSkill`

Recruiter/jobs domain:
- `Recruiter`
- `Job`
- `JobSkill`
- `Recommendation`

Assessment domain:
- `AssessmentQuestion`
- `AssessmentAttempt`
- `AssessmentAnswer`

Reference domain:
- `Country`
- `Language`
- `JobTitle`
- `Skill`

### Important relationships
- `User` 1:1 `JobSeeker`
- `User` 1:1 `Recruiter`
- `JobSeeker` 1:M `Experience`, `Education`, `Project`, `Resume`, `Certificate`
- `JobSeeker` 1:1 `SocialAccount`
- `JobSeeker` M:N `Skill` via `JobSeekerSkill`
- `Recruiter` 1:M `Job`
- `Job` M:N `Skill` via `JobSkill`
- `Job` M:N `JobSeeker` via `Recommendation` (with score metadata)
- `JobSeeker` 1:M `AssessmentAttempt`
- `AssessmentAttempt` 1:M `AssessmentAnswer`

### Important fields and purpose
- `User.ProfileCompletionStep`: wizard progress state
- `User.AuthProvider`, `User.ProviderUserId`: auth source and OAuth linkage
- `User.FailedLoginAttempts`, `User.LockoutEnd`: account lockout mechanism
- `IsDeleted` + `DeletedAt` in multiple job seeker entities: soft-delete behavior
- `AssessmentAttempt.OverallScore`, `ScoreExpiresAt`, `Status`: assessment lifecycle fields

### Constraints and indexes worth noting
- Unique email index on `User.Email`
- Date constraints:
  - Education: end >= start
  - Experience: end >= start
  - Certificate: expiration >= issue
- Filtered unique index: single active resume per job seeker
- Filtered unique index: one in-progress assessment attempt per job seeker
- Unique composite indexes on junctions (`JobSkill`, `JobSeekerSkill`)

### Seeded data
Seed classes populate:
- Job titles
- Countries
- Languages
- Skills

## 5. API Structure & Endpoints

### Authentication module (`AuthController`)
Purpose: registration, login, verification, reset, and current-user identity

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

Primary DTOs:
- Inputs: `RegisterDto`, `LoginDto`, `GoogleAuthDto`, `EmailVerificationDto`, `ResendVerificationDto`, `ForgotPasswordDto`, `ValidateResetTokenDto`, `ResetPasswordDto`
- Outputs: `AuthResponseDto`, `CurrentUserResponse`

### Job seeker core profile module (`JobSeekerController`)
Purpose: personal info, wizard status, step advancement, job titles, profile picture

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

Primary DTOs:
- Inputs: `PersonalInfoRequestDto`
- Outputs: `PersonalInfoDto`, `WizardStatusDto`, `ProfileResponseDto`, profile picture DTOs

### Experience module (`ExperienceController`)
Endpoints:
- `GET /api/jobseeker/experience`
- `GET /api/jobseeker/experience/{id}`
- `POST /api/jobseeker/experience`
- `PUT /api/jobseeker/experience/{id}`
- `DELETE /api/jobseeker/experience/{id}`
- `POST /api/jobseeker/experience/reorder`
- `GET /api/jobseeker/experience/exists`

DTOs:
- Input: `ExperienceRequestDto`
- Output: `ExperienceResponseDto`, `ExperienceListResponseDto`

### Education module (`EducationController`)
Endpoints:
- `GET /api/jobseeker/education`
- `GET /api/jobseeker/education/{id}`
- `POST /api/jobseeker/education`
- `PUT /api/jobseeker/education/{id}`
- `DELETE /api/jobseeker/education/{id}`
- `POST /api/jobseeker/education/reorder`
- `GET /api/jobseeker/education/exists`

DTOs:
- Input: `EducationRequestDto`
- Output: `EducationResponseDto`, `EducationListResponseDto`

### Projects module (`ProjectsController`)
Endpoints:
- `GET /api/jobseeker/projects`
- `POST /api/jobseeker/projects`
- `PUT /api/jobseeker/projects/{projectId}`
- `DELETE /api/jobseeker/projects/{projectId}`

DTOs:
- Inputs: `AddProjectDto`, `UpdateProjectDto`
- Outputs: `ProjectResponseDto`, `ProjectDto`

### Skills module (`JobSeekerSkillsController`)
Endpoints:
- `GET /api/jobseeker/skills`
- `PUT /api/jobseeker/skills`
- `DELETE /api/jobseeker/skills`
- `GET /api/jobseeker/skills/available`

DTOs:
- Input: `UpdateSkillsRequestDto`
- Output: `SkillsResponseDto`, `SkillDto`

### Social accounts module (`SocialAccountsController`)
Endpoints:
- `PUT /api/jobseeker/social-accounts`
- `GET /api/jobseeker/social-accounts`
- `DELETE /api/jobseeker/social-accounts`

DTOs:
- Input: `UpdateSocialAccountDto`
- Output: `SocialAccountResponseDto`

### Resume module (`ResumeController`)
Endpoints:
- `POST /api/jobseeker/resume/upload`
- `GET /api/jobseeker/resume`
- `GET /api/jobseeker/resume/download`
- `DELETE /api/jobseeker/resume`
- `GET /api/jobseeker/resume/exists`

DTOs:
- Output wrappers: `ResumeResponseDto`, `ResumeDto`, `FileValidationResult`

### Certificates module (`CertificatesController`)
Endpoints:
- `GET /api/jobseeker/certificates`
- `GET /api/jobseeker/certificates/{id}`
- `POST /api/jobseeker/certificates` (multipart)
- `PUT /api/jobseeker/certificates/{id}` (multipart)
- `DELETE /api/jobseeker/certificates/{id}`
- `GET /api/jobseeker/certificates/{id}/download`

DTOs:
- Input: `CertificateRequestDto`
- Output: `CertificateResponseDto`, `CertificateListResponseDto`, `CertificateDto`

### Recruiter profile module (`RecruiterController`)
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
- Input: `RecruiterCompanyInfoRequestDto`
- Output: `RecruiterCompanyInfoDto`, `IndustryDto`, `CompanySizeDto`, profile/wizard DTOs

### Recruiter job management module (`JobsController`)
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
- Input: `JobRequestDto`
- Output: `JobResponseDto`, `JobListResponseDto`, `SkillOptionDto`

### Locations/reference module (`LocationsController`)
Endpoints:
- `GET /api/locations/countries`
- `GET /api/locations/languages`

DTOs:
- `CountryDto`, `LanguageDto`

## 6. Completed Modules

Fully implemented based on current code:
- Authentication and security module
- Email verification and password reset flows
- Google OAuth login/linking behavior
- Account lockout policy with lockout email notification
- Job seeker personal profile module
- Job seeker wizard status + explicit wizard advance
- Experience CRUD + reorder + soft delete
- Education CRUD + reorder + soft delete
- Projects CRUD + soft delete + order maintenance
- Skills assignment module
- Social accounts module
- Resume module (upload/download/delete/exists)
- Certificates module (with optional file attachments)
- Profile picture module (job seeker + recruiter)
- Recruiter company profile module
- Recruiter job posting management module
- Reference lookup endpoints (countries/languages + skills/job titles through dedicated endpoints)

## 7. In-Progress Modules

Partially implemented (present in schema/model/config but incomplete as product feature):
- Assessment module
  - Entities and EF mappings exist
  - Constants/enums exist
  - No assessment controllers/services exposed for runtime usage
- Recommendation/matching module
  - `Recommendation` table/entity exists
  - No controller/service implementing recommendation generation or retrieval API

Additional partial consistency items:
- Some comments/docs in code still reflect old wizard step descriptions
- Returned URL in `ProfilePictureService` uses `/api/profile/picture` while actual routes are under `/api/jobseeker/picture` and `/api/recruiter/picture`

## 8. Missing / Not Yet Implemented Modules

Not found as complete backend modules in current code:
- Candidate-side job application workflow (apply/unapply/application status entities/endpoints)
- Public job discovery/search endpoints for job seekers (beyond recruiter-owned jobs)
- Assessment runtime APIs (start attempt, serve questions, submit answers, complete assessment)
- Recommendation APIs for recruiter or job seeker consumption
- Admin/backoffice module
- Custom middleware layer (global exception middleware, custom rate-limit middleware)

Assumption:
- Items listed as missing may exist in planning documents, but are absent in executable API/service code.

## 9. Profile Completion Wizard (Detailed)

Current implementation in code is a 4-step job seeker wizard with explicit advancement.

### Step 1: Personal Info
Data stored:
- `JobSeeker`: job title, years of experience, country, city, phone, first/second language + proficiency, bio
- `User.ProfileCompletionStep` may be bumped to 1 after save

Endpoints:
- `POST /api/jobseeker/personal-info`
- `GET /api/jobseeker/personal-info`

Validation:
- DataAnnotations on request DTO
- Service checks FK existence for job title/country/languages
- Duplicate language guard (first and second language must differ)
- Normalization of city/phone

Persistence:
- Upsert-like behavior for missing `JobSeeker` records (backward compatibility)
- Saves through EF Core and updates user step when required

### Step 2: Experience and Education
Data stored:
- `Experience` rows
- `Education` rows

Endpoints:
- Experience: list/get/create/update/delete/reorder/exists
- Education: list/get/create/update/delete/reorder/exists

Validation:
- Date logic in service (current flags, end date checks)
- DB check constraints also protect date consistency

Persistence:
- CRUD in dedicated services
- Soft delete (`IsDeleted`, `DeletedAt`)
- Reorder operations update display order
- No implicit wizard advancement

### Step 3: Projects
Data stored:
- `Project` rows with display order and soft-delete fields

Endpoints:
- `GET /api/jobseeker/projects`
- `POST /api/jobseeker/projects`
- `PUT /api/jobseeker/projects/{projectId}`
- `DELETE /api/jobseeker/projects/{projectId}`

Validation:
- DTO length/url checks
- Role ownership checks in service

Persistence:
- Create computes next display order
- Delete is soft delete + reorders remaining active projects
- No implicit wizard advancement

### Step 4: Skills, Social Accounts, Certificates
Data stored:
- `JobSeekerSkill` mappings
- `SocialAccount` (single profile links record)
- `Certificate` rows + optional file metadata

Endpoints:
- Skills: get/update/clear/available
- Social accounts: upsert/get/delete
- Certificates: list/get/create/update/delete/download

Validation:
- Skill IDs verified against reference skills table
- Social URLs validated by DataAnnotations
- Certificate date and file validations (size/type)

Persistence:
- Skills update is replace-all model (remove then insert)
- Social account uses upsert logic
- Certificates support optional file replace and soft delete
- No implicit wizard advancement

### Current step tracking status
Exists and works in backend:
- Tracked in `User.ProfileCompletionStep`
- Read via `/api/jobseeker/wizard-status`
- Advanced via explicit `/api/jobseeker/wizard/advance/{step}`
- Recruiter has separate one-step wizard status/advance endpoints

### Missing or incomplete wizard logic
- Backend does not strictly enforce required data completion before allowing manual `advance/{step}`
- Token claim `ProfileCompletionStep` can become stale until token refresh/re-login
- Some comments still mention older step naming/numbering

## 10. Business Rules & Special Logic

### Authentication and security
- Email is normalized before lookup/write
- Register creates role-specific record (`JobSeeker` or `Recruiter`)
- Email verification uses recent unused code with expiration
- Constant-time comparison used for verification code check
- Password reset always returns non-enumerating response for unknown email
- Account lockout after 5 failed attempts for 15 minutes
- Lockout applies to Google login as well

### Data integrity and ownership
- Recruiter job endpoints enforce ownership before update/delete
- Job seeker modules enforce account type
- Many modules include backward-compatible create-if-missing behavior for profile root row

### File handling
- Resume validation includes extension, MIME, and file signature checks
- Profile picture validation includes extension/MIME/magic bytes
- Certificate files are optional and validated on upload

### Persistence behavior pattern
- Most profile modules use soft delete
- Recruiter jobs use hard delete + `IsActive` toggle operations

## 11. Coding Conventions

### Naming and organization
- Interfaces prefixed with `I` (e.g., `IAuthService`)
- Controllers grouped by domain in `Controllers`
- DTOs grouped by feature in `DTOs`
- Entities grouped by bounded domain in `Models`

### DTO and response patterns
- Common wrappers: `ApiResponse<T>` and `ApiErrorResponse`
- Module-specific wrappers used where richer response state is needed

### Dependency injection
- Services registered as scoped in `Program.cs`
- Constructor injection is standard across controllers/services

### Validation patterns
- DataAnnotations on DTOs for contract-level validation
- Service-level business validation for ownership/FKs/date logic
- Additional DB-level check constraints and unique indexes

### Data access patterns
- EF Core LINQ directly from services
- Manual entity-to-DTO mapping (no AutoMapper usage)
- `AsNoTracking()` used in read paths where appropriate

## 12. Current Development Status

### Finished
- Auth and security foundations
- Job seeker and recruiter profile domains
- Explicit wizard step advancement model
- Recruiter job posting CRUD and status toggling
- Profile asset modules (resume/picture/certificate)
- Reference data APIs

### Partially done
- Assessment domain is structurally prepared but API/business workflow missing
- Recommendation domain exists in schema but no service/controller workflow
- Minor route/comment inconsistencies remain

### Still to build
- Job seeker-facing job discovery/application pipeline
- Assessment runtime module (attempt lifecycle)
- Recommendation engine API surface
- Admin/operations API area
- Optional hardening: custom middleware for uniform exception handling/rate limiting

## Assumptions and Notes
- Status labels are based only on executable code found in this repository.
- If a feature appears in docs but not in controller/service/runtime code, it is marked as in progress or missing.
- The source of truth for behavior is implementation, not comments where they conflict.
