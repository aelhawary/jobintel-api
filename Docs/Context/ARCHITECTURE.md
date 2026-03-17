# JobIntel Backend Architecture

## Purpose
This document gives a fast architectural map for AI-assisted development and extension.

## System Style
- Monolithic ASP.NET Core Web API
- Layered architecture (Controller -> Service -> DbContext -> SQL Server)
- No repository abstraction currently in use

## Runtime Composition

### Entry Point
- `RecruitmentPlatformAPI/Program.cs`

### Runtime pipeline
1. HTTPS redirection
2. CORS (`AllowFrontend` policy)
3. JWT authentication
4. Authorization
5. Controller routing

### Service registration model
- Services are registered as `Scoped`
- Interface-first DI pattern (`I*Service` -> `*Service`)

## Codebase Layout
- `RecruitmentPlatformAPI/Controllers`: HTTP endpoints and status-code shaping
- `RecruitmentPlatformAPI/Services/Auth`: auth, token creation, email dispatch
- `RecruitmentPlatformAPI/Services/JobSeeker`: wizard/profile modules
- `RecruitmentPlatformAPI/Services/Recruiter`: company profile and recruiter job CRUD
- `RecruitmentPlatformAPI/Data`: `AppDbContext`, migrations, seeders
- `RecruitmentPlatformAPI/Models`: EF entities grouped by bounded context
- `RecruitmentPlatformAPI/DTOs`: transport contracts by module
- `RecruitmentPlatformAPI/Configuration`: settings classes for JWT/email/files

## Request Flow

### Typical authenticated endpoint flow
1. Controller receives request (`[Authorize]` where required)
2. Controller extracts `NameIdentifier` claim (user id)
3. Controller validates DTO/model state
4. Controller calls service method
5. Service applies business rules + EF Core access
6. Service maps entities to DTO response shape
7. Controller returns `ApiResponse<T>` or domain-specific response DTO

## Persistence Architecture
- EF Core SQL Server provider
- One `AppDbContext` with explicit Fluent API configuration
- Initial migration defines schema, constraints, indexes, and seeded reference data
- Soft-delete convention used on multiple entities (not all)

## Domain Boundaries

### Identity/Auth
- `User`, `EmailVerification`, `PasswordReset`
- Password login + Google OAuth
- Lockout policy and secure reset flow

### Job Seeker Profile
- Personal info
- Experience
- Education
- Projects
- Skills
- Social accounts
- Resume and certificates (file-backed)

### Recruiter
- Company profile
- Job posting lifecycle management

### Assessment and Recommendation (partially surfaced)
- Data model exists
- API/service workflows not fully exposed for production use

## Cross-Cutting Rules

### Security
- JWT bearer auth
- BCrypt password hashing
- Lockout after failed attempts
- Verification code and reset token expiration logic

### API conventions
- camelCase JSON output
- string enum serialization
- consistent model validation via DataAnnotations + service checks

### Error/response patterns
- Common wrappers: `ApiResponse<T>`, `ApiErrorResponse`
- Some modules use module-specific response DTO wrappers for richer context

## File Storage Architecture
- Root configurable via `FileStorageSettings`
- Separate folders for resumes, profile pictures, and certificates
- Validation layers include extension, MIME, and (for some files) signature checks

## Known Architectural Gaps
- No custom middleware for centralized exception handling
- No repository layer (service directly couples business logic with EF queries)
- Route/URL consistency issue in one profile picture URL generation path
- Assessment/recommendation engines are data-modeled but not full API products yet

## Extension Guidance for Next Agent
- Prefer adding new capabilities through existing module folders instead of creating parallel patterns
- Keep DTO validation in contracts and enforce business validation in services
- Preserve claim-based ownership checks for multi-tenant safety (especially recruiter job ownership)
- If adding major modules (assessment runtime or recommendations), create dedicated controller + service slices and reuse existing response conventions
