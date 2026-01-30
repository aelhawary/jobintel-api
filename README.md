# JobIntel Recruitment Platform - Backend API

**Version:** 1.4.0  
**Last Updated:** January 1, 2026  
**Framework:** ASP.NET Core 9.0 (C#)  
**Database:** SQL Server (Entity Framework Core)

---

## 🎯 Project Overview

JobIntel is a modern recruitment platform designed to connect job seekers with recruiters. This repository contains the backend REST API built with ASP.NET Core, featuring comprehensive authentication, profile management, and a multi-step profile completion wizard.

### Current Implementation Status

**✅ Completed Features:**
- Complete authentication system (9 endpoints)
- Profile wizard Steps 1-2 (8 endpoints)
- 18 database tables with proper relationships
- Comprehensive API documentation

**🔄 Next Phase:**
- Profile wizard Steps 3-5 (CV Upload, Work Experience, Education)
- Job posting and management
- AI-powered job recommendations

### Key Features

✅ **Dual Authentication System**
- Email/Password authentication with email verification
- Google OAuth 2.0 integration
- JWT token-based authorization
- Account lockout after 5 failed login attempts (15-min lockout)
- Password reset with OTP verification (6-digit code, 15-min expiry)

✅ **Profile Completion Wizard** (6 Steps)
- **Step 1: Personal Information** ✅ 
  - Job title, years of experience, location (country/city)
  - Languages (first + optional second with proficiency levels)
  - Auto-advances wizard progress tracking
- **Step 2: Projects Portfolio** ✅
  - CRUD operations with auto-ordering
  - Soft delete with automatic reordering
  - Project title, description, technologies, live links
- **Step 3: CV Upload** 📅 Planned
- **Step 4: Work Experience** 📅 Planned
- **Step 5: Education** 📅 Planned
- **Step 6: Social Links** ✅
  - Add/update/remove social media links (completely optional)
  - LinkedIn, GitHub, Behance, Dribbble, Personal Website
  - Users can skip or add any combination of links

✅ **Security Features**
- BCrypt password hashing (work factor 12)
- JWT token authentication (24-hour expiration)
- Account lockout protection
- Email verification required before login
- CORS configuration
- Comprehensive input validation

✅ **Database Architecture**
- **18 tables:** User, JobSeeker, Recruiter, Project, Experience, Education, Resume, SocialAccount, Job, Recommendation, Skill, Country, Language, JobTitle, EmailVerification, PasswordReset, JobSeekerSkill, JobSkill
- Entity Framework Core with Code-First migrations
- Reference tables with bilingual support (English/Arabic)
- **Seeded data:** 90 job titles, 65 countries, 50 languages
- Soft delete pattern for data retention

---

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK or later
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 / VS Code / Rider
- Gmail account (for email notifications)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Backend-2
   ```

2. **Configure appsettings.json**
   ```bash
   cd RecruitmentPlatformAPI
   cp appsettings.json appsettings.Development.json
   ```

3. **Update connection string and secrets** in `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RecruitmentPlatformDb;Trusted_Connection=True;"
     },
     "JwtSettings": {
       "SecretKey": "your-256-bit-secret-key-here",
       "Issuer": "JobIntelAPI",
       "Audience": "JobIntelClient",
       "ExpirationMinutes": 1440
     },
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "your-email@gmail.com",
       "SenderName": "JobIntel Platform",
       "Username": "your-email@gmail.com",
       "Password": "your-app-password"
     },
     "GoogleOAuth": {
       "ClientId": "your-google-client-id.apps.googleusercontent.com"
     }
   }
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   ```
   https://localhost:<port>/swagger
   ```

---

## 📁 Project Structure

```
Backend-2/
├── .gitignore                      # Git ignore rules
├── .vs/                            # Visual Studio cache (ignored)
├── api-schema.json                 # OpenAPI schema
├── RecruitmentPlatform.sln         # Visual Studio solution
│
├── Docs/                           # 📚 Complete Documentation
│   ├── README.md                   # Documentation index
│   ├── CHANGELOG.md                # Version history
│   ├── SETUP_GUIDE.md              # Setup instructions
│   ├── API_REFERENCE.md            # Complete API documentation
│   ├── REACT_INTEGRATION_GUIDE.md  # Frontend integration guide
│   ├── AUTH_API_INTEGRATION.md     # Auth endpoints reference
│   ├── GOOGLE_AUTH_GUIDE.md        # Google OAuth setup
│   ├── EMAIL_SETUP_GUIDE.md        # Email configuration
│   ├── SWAGGER_IMPROVEMENTS.md     # Swagger documentation guide
│   ├── API_DOCUMENTATION_STANDARDS.md # API standards
│   ├── ERD_DIAGRAM.md              # Entity Relationship Diagram
│   ├── ERD_dbdiagram.dbml          # ERD in DBML format
│   ├── ERD_PlantUML.puml           # ERD in PlantUML format
│   └── DOCS_GUIDE.md               # Documentation guide
│
└── RecruitmentPlatformAPI/         # Main API Project
    ├── Program.cs                  # Application entry point
    ├── appsettings.json            # Configuration (template)
    ├── appsettings.Development.json # Dev config (gitignored)
    │
    ├── Controllers/                # API Controllers
    │   ├── AuthController.cs       # Authentication endpoints
    │   ├── ProfileController.cs    # Profile wizard endpoints
    │   ├── ProjectsController.cs   # Projects CRUD
    │   └── LocationsController.cs  # Reference data
    │
    ├── Services/                   # Business Logic Layer
    │   ├── AuthService.cs          # Authentication logic
    │   ├── ProfileService.cs       # Profile management
    │   ├── ProjectService.cs       # Projects management
    │   ├── EmailService.cs         # Email notifications
    │   └── TokenService.cs         # JWT token generation
    │
    ├── Models/                     # Entity Models
    │   ├── User.cs                 # User entity
    │   ├── JobSeeker.cs            # Job seeker profile
    │   ├── Recruiter.cs            # Recruiter profile
    │   ├── Project.cs              # Project entity
    │   ├── JobTitle.cs             # Reference table
    │   ├── Country.cs              # Reference table
    │   └── Language.cs             # Reference table
    │
    ├── DTOs/                       # Data Transfer Objects
    │   ├── AuthResponseDto.cs      # Auth responses
    │   ├── PersonalInfoDto.cs      # Profile DTOs
    │   └── ProjectDtos.cs          # Project DTOs
    │
    ├── Data/                       # Database Context
    │   ├── AppDbContext.cs         # EF Core DbContext
    │   └── Migrations/             # EF Core migrations
    │
    ├── Enums/                      # Enumerations
    │   ├── AccountType.cs          # JobSeeker/Recruiter
    │   ├── AuthProvider.cs         # Email/Google
    │   └── LanguageProficiency.cs  # Language levels
    │
    └── Configuration/              # Configuration Models
        ├── JwtSettings.cs
        └── EmailSettings.cs
```

---

## 🔌 API Endpoints

### Authentication (9 endpoints)
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login with credentials
- `POST /api/auth/google` - Google OAuth login
- `POST /api/auth/verify-email` - Verify email with code
- `POST /api/auth/resend-verification` - Resend verification code
- `POST /api/auth/forgot-password` - Request password reset OTP
- `POST /api/auth/verify-reset-otp` - Verify OTP and get reset token
- `POST /api/auth/reset-password` - Reset password with token
- `GET /api/auth/me` - Get current user info (requires auth)

### Profile Wizard - Step 1: Personal Information (4 endpoints)
- `POST /api/profile/personal-info` - Save personal information
- `GET /api/profile/personal-info?lang=en` - Get personal info with localization
- `GET /api/profile/wizard-status` - Get wizard completion status
- `GET /api/profile/job-titles` - Get job titles list

### Profile Wizard - Step 2: Projects Portfolio (4 endpoints)
- `POST /api/profile/projects` - Add new project
- `GET /api/profile/projects` - Get all user's projects
- `PUT /api/profile/projects/{id}` - Update project
- `DELETE /api/profile/projects/{id}` - Delete project (soft delete)

### Profile Wizard - Step 6: Social Links (3 endpoints)
- `PUT /api/profile/social-accounts` - Add or update social links (optional)
- `GET /api/profile/social-accounts` - Get current social links
- `DELETE /api/profile/social-accounts` - Delete all social links

### Reference Data (2 endpoints)
- `GET /api/locations/countries?lang=en` - Get countries (localized)
- `GET /api/locations/languages?lang=en` - Get languages (localized)

**Total:** 22 API endpoints  
**Full API documentation:** See [Docs/API_REFERENCE.md](Docs/API_REFERENCE.md)

---

## 📚 Documentation

All documentation is located in the `Docs/` folder:

| Document | Description |
|----------|-------------|
| **[README.md](Docs/README.md)** | Documentation index and quick links |
| **[SETUP_GUIDE.md](Docs/SETUP_GUIDE.md)** | Complete setup instructions |
| **[API_REFERENCE.md](Docs/API_REFERENCE.md)** | All API endpoints with examples |
| **[REACT_INTEGRATION_GUIDE.md](Docs/REACT_INTEGRATION_GUIDE.md)** | Frontend integration guide |
| **[GOOGLE_AUTH_GUIDE.md](Docs/GOOGLE_AUTH_GUIDE.md)** | Google OAuth setup |
| **[EMAIL_SETUP_GUIDE.md](Docs/EMAIL_SETUP_GUIDE.md)** | Email configuration |
| **[CHANGELOG.md](Docs/CHANGELOG.md)** | Version history |

👉 **Frontend developers start here:** [REACT_INTEGRATION_GUIDE.md](Docs/REACT_INTEGRATION_GUIDE.md)

---

## 🛠️ Technology Stack

- **Framework:** ASP.NET Core 9.0
- **Language:** C# 12.0
- **ORM:** Entity Framework Core 9.0
- **Database:** SQL Server
- **Authentication:** JWT + Google OAuth 2.0
- **Password Hashing:** BCrypt.Net
- **Email:** SMTP (Gmail)
- **API Documentation:** Swagger/OpenAPI

### NuGet Packages
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `BCrypt.Net-Next`
- `Google.Apis.Auth`
- `Swashbuckle.AspNetCore`

---

## 🗄️ Database Schema

### Core Tables
- **User** - Base user account (shared by job seekers and recruiters)
- **JobSeeker** - Job seeker profile with FK to User
- **Recruiter** - Recruiter profile with FK to User
- **Project** - Job seeker projects with soft delete
- **Experience** - Work experience records
- **Education** - Education records
- **Resume** - CV uploads

### Reference Tables
- **JobTitle** - 90 predefined job titles
- **Country** - 65 countries with English/Arabic names
- **Language** - 50 languages with English/Arabic names

### Supporting Tables
- **EmailVerification** - Email verification codes
- **PasswordReset** - Password reset OTPs
- **Skill** - Skills catalog
- **Job** - Job postings (recruiter side)

**Full schema documentation:** See [Docs/database-documentation.md](Docs/database-documentation.md)

---

## 🔐 Security

- ✅ BCrypt password hashing (cost factor: 12)
- ✅ JWT tokens with configurable expiration
- ✅ Email verification required for login
- ✅ Account lockout after 5 failed login attempts (15 minutes)
- ✅ Password reset with time-limited OTP
- ✅ CORS configured for localhost (update for production)
- ✅ Input validation on all endpoints
- ✅ Enum-based type safety for critical fields
- ✅ No sensitive data in error messages

---

## 📈 Current Status & Roadmap

### ✅ Completed (v1.4.0)
- Authentication system (Email + Google OAuth)
- Profile Wizard Step 1: Personal Information
- Profile Wizard Step 2: Projects Management
- Reference data with bilingual support
- Comprehensive documentation
- Security features

### 🚧 In Progress
- Profile Wizard Step 3: CV Upload
- Profile Wizard Step 4: Work Experience
- Profile Wizard Step 5: Education
- Profile Wizard Step 6: Social Links

### 📋 Planned Features
- Resume parsing
- Skills recommendation system
- Job posting and matching
- Application tracking
- Messaging system
- Notification system
- Admin panel
- Analytics dashboard

---

## 🤝 Contributing

This is a graduation project. Contributions are welcome via pull requests.

### Development Workflow
1. Create a feature branch
2. Make your changes
3. Run `dotnet build` to verify
4. Update relevant documentation
5. Submit a pull request

---

## 📝 License

[Specify your license here]

---

## 👥 Team

[Add team member names and roles]

---

## 📞 Support

For questions or issues:
- Check the [Documentation](Docs/README.md)
- Review the [CHANGELOG](Docs/CHANGELOG.md)
- Open an issue on GitHub

---

**Happy Coding! 🚀**
