# JobIntel Recruitment Platform — Backend API

**Version:** 1.6.0
**Last Updated:** March 2026
**Framework:** ASP.NET Core 9.0 (C# 12)
**Database:** SQL Server (Entity Framework Core 9.0)

---

## Project Overview

JobIntel is a recruitment platform backend that connects **Job Seekers** with **Recruiters**. This repository contains the REST API powering authentication, profile management, and job posting.

### Current Status

| Module | Status | Endpoints |
|--------|--------|-----------|
| Authentication (Email + Google OAuth) | ✅ Complete | 9 |
| Job Seeker Profile Wizard (4 steps) | ✅ Complete | 35 |
| Recruiter Profile | ✅ Complete | 10 |
| Reference Data (Countries, Languages) | ✅ Complete | 2 |
| Job Posting & Management | ✅ Complete | 8 |
| Skill Assessments | ✅ Complete | 9 |
| AI Matching / Recommendations | 📋 Planned | — |
| Notifications / Admin | 📋 Planned | — |

**Total:** 73 API endpoints across 11 controllers

---

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- SQL Server (LocalDB or full instance)
- Gmail account with App Password (for email notifications)

### Setup

```bash
git clone <repository-url>
cd Backend-2/RecruitmentPlatformAPI
dotnet restore
```

Create `appsettings.Development.json` (gitignored):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RecruitmentPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharsLong12345",
    "Issuer": "RecruitmentPlatformAPI",
    "Audience": "RecruitmentPlatformClient",
    "ExpirationMinutes": 1440
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "JobIntel Platform",
    "Username": "your-email@gmail.com",
    "Password": "your-gmail-app-password",
    "EnableSsl": true,
    "FrontendUrl": "http://localhost:3000"
  },
  "GoogleOAuth": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com"
  }
}
```

```bash
dotnet ef database update
dotnet run
```

Swagger UI: `http://localhost:5217/swagger`

> **Teammate?** See [Docs/Guides/TEAMMATE_SETUP_GUIDE.md](Docs/Guides/TEAMMATE_SETUP_GUIDE.md) for a streamlined 10-minute setup.

---

## Project Structure

```
Backend-2/
├── PROJECT_CONTEXT.md                  # Single source of truth for AI/dev context
├── README.md                           # This file
├── RecruitmentPlatform.sln
│
├── Docs/
│   ├── CHANGELOG.md                    # Version history
│   ├── PROJECT_OVERVIEW.md             # Quick onboarding for new team members
│   ├── PROJECT_DETAILED_GUIDE.md       # Deep technical reference
│   ├── API/                            # API documentation
│   │   ├── API_REFERENCE.md
│   │   ├── AUTH_API_INTEGRATION.md     # Auth handoff for frontend team
│   │   ├── API_DOCUMENTATION_STANDARDS.md
│   │   └── SWAGGER_IMPROVEMENTS.md
│   ├── Database/                       # ERD (Markdown, DBML, PlantUML)
│   ├── Diagrams/                       # Class diagram (Mermaid)
│   ├── Guides/                         # Setup, email, Google OAuth, Jobs module
│   └── Context/                        # AI handoff context docs and bootstrap prompts
│
└── RecruitmentPlatformAPI/
    ├── Program.cs                      # DI, JWT, CORS, JSON config, middleware
    ├── Controllers/                    # 11 controllers
    │   ├── AuthController.cs           # 9 endpoints — login, register, OAuth, verify, reset
    │   ├── JobSeekerController.cs      # 9 endpoints — personal info, wizard, profile picture
    │   ├── ProjectsController.cs       # 4 endpoints — CRUD with soft delete & auto-reorder
    │   ├── ExperienceController.cs     # 7 endpoints — CRUD with soft delete & reorder
    │   ├── EducationController.cs      # 7 endpoints — CRUD with soft delete & reorder
    │   ├── ResumeController.cs         # 5 endpoints — PDF upload/download with validation
    │   ├── SocialAccountsController.cs # 3 endpoints — upsert/get/delete social links
    │   ├── RecruiterController.cs      # 10 endpoints — company info, wizard, profile picture
    │   ├── JobsController.cs           # 8 endpoints — job posting CRUD for recruiters
    │   ├── AssessmentController.cs     # 9 endpoints — skill assessment flow (30 Qs, 45 min)
    │   └── LocationsController.cs      # 2 endpoints — countries, languages (bilingual)
    ├── Services/
    │   ├── Auth/                        # AuthService, EmailService, TokenService
    │   ├── JobSeeker/                   # 7 services (profile, projects, experience, etc.)
    │   ├── Recruiter/                   # RecruiterService
    │   ├── Jobs/                        # JobService
    │   └── Assessment/                  # AssessmentService (eligibility, questions, scoring)
    ├── Models/
    │   ├── Identity/                    # User, EmailVerification, PasswordReset
    │   ├── JobSeeker/                   # JobSeeker, Project, Experience, Education, etc.
    │   ├── Recruiter/                   # Recruiter
    │   ├── Jobs/                        # Job, JobSkill, Recommendation (DB ready)
    │   ├── Assessment/                  # AssessmentQuestion, Attempt, Answer (DB ready)
    │   └── Reference/                   # Country, Language, JobTitle, Skill
    ├── DTOs/
    │   ├── Auth/                        # Register, Login, Google, Password Reset DTOs
    │   ├── Common/                      # ApiResponse<T>, ApiErrorResponse, CountryDto, LanguageDto
    │   ├── JobSeeker/                   # Personal info, projects, experience, education, etc.
    │   └── Recruiter/                   # RecruiterDtos
    ├── Enums/                           # AccountType, AuthProvider, EmploymentType, etc.
    ├── Configuration/                   # JwtSettings, EmailSettings, etc.
    ├── Data/
    │   ├── AppDbContext.cs              # 19 DbSets, Fluent API config
    │   ├── Migrations/                  # Single InitialCreate migration
    │   └── Seed/                        # 90 job titles, 65 countries, 50 languages
    └── Uploads/                         # ProfilePictures/, Resumes/ (gitignored content)
```

---

## API Endpoints

### Authentication (`/api/auth/`) — 9 endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/register` | No | Register (JobSeeker or Recruiter) |
| POST | `/login` | No | Login with email/password |
| POST | `/google` | No | Login/register with Google OAuth |
| POST | `/verify-email` | No | Verify email with 6-digit code |
| POST | `/resend-verification` | No | Resend verification code |
| POST | `/forgot-password` | No | Request password reset link |
| POST | `/validate-reset-token` | No | Check if reset token is valid |
| POST | `/reset-password` | No | Reset password with token |
| GET | `/me` | ✅ | Get current user from JWT |

### Job Seeker Profile (`/api/jobseeker/`) — 35 endpoints

| Group | Endpoints | Description |
|-------|-----------|-------------|
| Personal Info (Step 1) | 4 | Save/get info, wizard status, job titles |
| Profile Picture | 5 | Upload, get info, download, delete, exists |
| Resume | 5 | PDF upload, download, delete, info, exists |
| Experience (Step 2) | 7 | CRUD with reorder & soft delete |
| Education (Step 2) | 7 | CRUD with reorder & soft delete |
| Projects (Step 3) | 4 | CRUD with auto-reorder & soft delete |
| Skills & Social (Step 4) | 3 | Skills update, social links upsert/get/delete |

### Recruiter (`/api/recruiter/`) — 10 endpoints

| Group | Endpoints | Description |
|-------|-----------|-------------|
| Company Info | 3 | Save/get info, wizard status |
| Dropdowns | 2 | Industries, company sizes |
| Profile Picture | 5 | Upload, get info, download, delete, exists |

### Reference Data (`/api/locations/`) — 2 endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/countries?lang=en` | Countries (bilingual EN/AR) |
| GET | `/languages?lang=en` | Languages (bilingual EN/AR) |

### Jobs (`/api/jobs/`) — 8 endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/` | ✅ | List recruiter's jobs (paginated) |
| GET | `/{id}` | ✅ | Get single job details |
| POST | `/` | ✅ | Create new job posting |
| PUT | `/{id}` | ✅ | Update job posting |
| DELETE | `/{id}` | ✅ | Delete job posting |
| PATCH | `/{id}/activate` | ✅ | Activate job |
| PATCH | `/{id}/deactivate` | ✅ | Deactivate job |
| GET | `/skills` | No | Get available skills for job creation |

### Assessments (`/api/assessment/`) — 9 endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/eligibility` | ✅ | Check if user can start assessment |
| POST | `/start` | ✅ | Start new 30-question assessment |
| GET | `/current` | ✅ | Get in-progress assessment status |
| GET | `/question` | ✅ | Get next unanswered question |
| POST | `/answer` | ✅ | Submit answer (no immediate feedback) |
| POST | `/complete` | ✅ | Finish and get full results |
| POST | `/abandon` | ✅ | Abandon current attempt |
| GET | `/history` | ✅ | Get all past attempts |
| GET | `/result/{id}` | ✅ | Get detailed result for an attempt |

---

## Technology Stack

| Component | Package | Version |
|-----------|---------|---------|
| Framework | ASP.NET Core | 9.0 |
| ORM | Microsoft.EntityFrameworkCore.SqlServer | 9.0.10 |
| Auth | Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.10 |
| JWT | System.IdentityModel.Tokens.Jwt | 8.14.0 |
| Password | BCrypt.Net-Next | 4.0.3 |
| Email | MailKit | 4.14.1 |
| Google OAuth | Google.Apis.Auth | 1.73.0 |
| API Docs | Swashbuckle.AspNetCore | 6.5.0 |

---

## Database

**19 tables** across 6 categories:

| Category | Tables |
|----------|--------|
| Identity | User, EmailVerification, PasswordReset |
| Job Seeker | JobSeeker, Project, Experience, Education, Resume, SocialAccount, JobSeekerSkill |
| Recruiter | Recruiter |
| Jobs | Job, JobSkill, Recommendation |
| Assessment | AssessmentQuestion, AssessmentAttempt, AssessmentAnswer |
| Reference | Country, Language, JobTitle, Skill |

Seed data: **90** job titles, **65** countries, **50** languages (bilingual EN/AR), **73** assessment questions.

---

## Security

- BCrypt password hashing (cost factor 12)
- JWT Bearer tokens (HMAC-SHA256, 24-hour expiry)
- Email verification required before login
- Account lockout: 5 failed attempts → 15-minute lock
- Password reset via secure cryptographic token (not OTP)
- Constant-time comparison for verification codes
- CORS configured for localhost (update for production)

---

## Documentation

| Document | Purpose |
|----------|---------|
| [PROJECT_CONTEXT.md](PROJECT_CONTEXT.md) | Single source of truth for all development |
| [Docs/Guides/TEAMMATE_SETUP_GUIDE.md](Docs/Guides/TEAMMATE_SETUP_GUIDE.md) | Quick 10-min setup for new developers |
| [Docs/Guides/SETUP_GUIDE.md](Docs/Guides/SETUP_GUIDE.md) | Detailed setup with troubleshooting |
| [Docs/API/AUTH_API_INTEGRATION.md](Docs/API/AUTH_API_INTEGRATION.md) | Auth handoff guide for frontend team |
| [Docs/API/JOBSEEKER_WIZARD_INTEGRATION.md](Docs/API/JOBSEEKER_WIZARD_INTEGRATION.md) | Job Seeker wizard frontend integration |
| [Docs/API/ASSESSMENT_MODULE_GUIDE.md](Docs/API/ASSESSMENT_MODULE_GUIDE.md) | Complete Assessment Module technical guide |
| [Docs/API/API_REFERENCE.md](Docs/API/API_REFERENCE.md) | Complete endpoint reference |
| [Docs/Context/README.md](Docs/Context/README.md) | AI handoff docs index and bootstrap references |
| [Docs/Database/ERD_DIAGRAM.md](Docs/Database/ERD_DIAGRAM.md) | Entity Relationship Diagram |
| [Docs/Diagrams/CLASS_DIAGRAM.md](Docs/Diagrams/CLASS_DIAGRAM.md) | Full class diagram (Mermaid) |
| [Docs/CHANGELOG.md](Docs/CHANGELOG.md) | Version history |

---

## Next Phase

The immediate next implementation focus is **AI-Powered Recommendation APIs**. This will enable intelligent job-candidate matching based on skills, experience, and assessment scores. Start with [PROJECT_CONTEXT.md](PROJECT_CONTEXT.md), then follow [Docs/Context/DEVELOPMENT_ROADMAP.md](Docs/Context/DEVELOPMENT_ROADMAP.md) for prioritized tasks.
