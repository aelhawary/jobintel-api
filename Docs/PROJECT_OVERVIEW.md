# JobIntel - Recruitment Platform API

## Quick Start Guide for New Team Members

Welcome! This guide will help you understand our backend project quickly.

---

## What is This Project?

A **REST API** for a recruitment platform where:
- **Job Seekers** can build profiles and manage resume, projects, experience, education, skills, social links, and certificates
- **Recruiters** can register companies and manage job postings

Built with: **ASP.NET Core 9.0** + **Entity Framework Core** + **SQL Server**

---

## Project Structure (What's Where)

```
RecruitmentPlatformAPI/
├── Controllers/          # API endpoints (receives HTTP requests)
├── Services/             # Business logic (the actual work)
├── Models/               # Database tables (C# classes → SQL tables)
├── DTOs/                 # Data Transfer Objects (what API sends/receives)
├── Enums/                # Fixed value lists (AccountType, etc.)
├── Data/                 # Database context + migrations
├── Configuration/        # Settings classes (JWT, Email, etc.)
└── Program.cs            # App startup configuration
```

---

## The Two User Types

### 1. Job Seeker (4-step profile wizard)
| Step | What They Fill |
|------|---------------|
| 1 | Personal Info (name, job title, location, bio) |
| 2 | Experience + Education |
| 3 | Projects (portfolio items) |
| 4 | Skills + Social Links + Certificates |

### 2. Recruiter (1-step profile)
| Step | What They Fill |
|------|---------------|
| 1 | Company Info (name, size, industry, location) |

---

## API Routes Summary

All routes start with `http://localhost:5217/api/`

### Authentication (`/api/auth/`)
```
POST /auth/register       → Create account
POST /auth/login          → Get JWT token
POST /auth/verify-email   → Verify email with OTP
POST /auth/google         → Login with Google
```

### Job Seeker (`/api/jobseeker/`)
```
POST /jobseeker/personal-info    → Save basic info
GET  /jobseeker/personal-info    → Get basic info
GET  /jobseeker/wizard-status    → Check profile completion
GET  /jobseeker/job-titles       → List of job titles for dropdown
POST /jobseeker/picture          → Upload profile picture
```

### Job Seeker Sub-routes
```
/api/jobseeker/projects/*        → CRUD for portfolio projects
/api/jobseeker/experience/*      → CRUD for work experience
/api/jobseeker/education/*       → CRUD for education
/api/jobseeker/social-accounts/* → Manage social links
/api/jobseeker/resume/*          → Upload/download CV
```

### Recruiter (`/api/recruiter/`)
```
POST /recruiter/company-info     → Save company info
GET  /recruiter/company-info     → Get company info
GET  /recruiter/wizard-status    → Check profile completion
GET  /recruiter/industries       → List of industries for dropdown
GET  /recruiter/company-sizes    → List of company sizes for dropdown
```

### Reference Data (`/api/locations/`)
```
GET /locations/countries         → All countries
GET /locations/languages         → All languages
```

---

## Key Concepts to Understand

### 1. JWT Authentication
- User logs in → Gets a **token** (long string)
- Every protected request must include: `Authorization: Bearer <token>`
- Token expires after 24 hours

### 2. DTOs (Data Transfer Objects)
- **RequestDto**: What the client sends TO us
- **ResponseDto**: What we send BACK to the client
- Example: `PersonalInfoRequestDto` → `PersonalInfoDto`

### 3. Services Pattern
```
Controller (receives request)
    ↓
Service (does the work)
    ↓
DbContext (talks to database)
```

### 4. Validation
- Model validation: `[Required]`, `[StringLength]`, `[EmailAddress]`
- Business validation: Done in Service layer (e.g., "is industry valid?")

---

## Database Tables (Main Ones)

| Table | Purpose |
|-------|---------|
| `User` | All users (both job seekers & recruiters) |
| `JobSeekers` | Extra fields for job seeker profiles |
| `Recruiters` | Company info for recruiters |
| `Experience` | Work history entries |
| `Education` | Education entries |
| `Project` | Portfolio projects |
| `SocialAccount` | LinkedIn, GitHub links |
| `JobTitle` | Reference: 90 predefined job titles |
| `Country` | Reference: All countries |
| `Language` | Reference: All languages |

---

## How to Run the Project

### 1. Prerequisites
- Visual Studio 2022 or VS Code
- .NET 9 SDK
- SQL Server (LocalDB works fine)

### 2. Setup
```bash
# Clone and navigate
cd Backend-2/RecruitmentPlatformAPI

# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the server
dotnet run
```

### 3. Test the API
- Open browser: `http://localhost:5217/swagger`
- Or use Postman/Thunder Client

---

## Files You'll Work With Most

| File | When to Edit |
|------|--------------|
| `Controllers/*.cs` | Adding new endpoints |
| `Services/JobSeeker/*.cs` | Adding JobSeeker business logic |
| `Services/Recruiter/*.cs` | Adding Recruiter business logic |
| `DTOs/JobSeeker/*.cs` | JobSeeker request/response shapes |
| `DTOs/Recruiter/*.cs` | Recruiter request/response shapes |
| `Models/*.cs` | Changing database structure |
| `Data/AppDbContext.cs` | Configuring table relationships |

---

## Common Tasks

### Adding a New Endpoint
1. Create DTO in `DTOs/` folder
2. Add method to interface in `Services/`
3. Implement in service class
4. Add endpoint in controller
5. Register service in `Program.cs` (if new service)

### Changing Database
1. Modify model in `Models/`
2. Run: `dotnet ef migrations add YourMigrationName`
3. Run: `dotnet ef database update`

---

## Current Status (What's Done)

✅ **Authentication Module**
- Register, Login, Email Verification, Password Reset
- Google OAuth integration
- JWT token generation

✅ **Job Seeker Profile**
- Personal info (Step 1)
- Experience CRUD (Step 2)
- Education CRUD (Step 2)
- Projects CRUD (Step 3)
- Skills + social accounts + certificates (Step 4)
- Resume upload (profile asset)
- Profile picture upload
- Wizard status tracking

✅ **Jobs Management**
- Recruiter-owned job CRUD
- Job activate/deactivate
- Job skills lookup and association

✅ **Recruiter Profile**
- Company info (single step)
- 20 predefined industries
- 6 company size options
- Wizard status tracking

✅ **Reference Data**
- 90 job titles across 8 categories
- Countries with English/Arabic names
- Languages with English/Arabic names

---

## What's NOT Done Yet

- [ ] Job search/feed for job seekers
- [ ] Application lifecycle (apply/unapply/status)
- [ ] Assessment runtime APIs
- [ ] Recommendation APIs
- [ ] Notifications
- [ ] Admin panel

---

## Questions?

Check these docs:
- `Docs/API/API_REFERENCE.md` - Detailed endpoint documentation
- `Docs/API/AUTH_API_INTEGRATION.md` - Auth flow for frontend
- `Docs/Guides/SETUP_GUIDE.md` - Detailed setup instructions

Or just ask! 🚀
