# Documentation Index

**Project:** JobIntel Recruitment Platform - Backend API  
**Last Updated:** January 1, 2026

---

## 📋 Documentation Status

**✅ All documentation updated and verified (January 1, 2026)**

**Backend Version:** 1.5.0  
**Status:** Production Ready for Frontend Integration

### Current Implementation
- ✅ **Authentication System:** Complete (9 endpoints)
  - Email/Password with verification
  - Google OAuth integration
  - Password reset flow
  - Account lockout protection
  
- ✅ **Profile Wizard Step 1:** Personal Information (4 endpoints)
  - Job title, experience, location
  - Languages with proficiency levels
  - Auto-advances wizard tracking
  
- ✅ **Profile Wizard Step 2:** Projects Portfolio (4 endpoints)
  - CRUD operations
  - Auto-ordering and soft delete
  - Project management

- ✅ **Profile Wizard Step 3:** CV Upload (5 endpoints)
  - PDF upload with validation (max 5MB)
  - File content verification (magic bytes)
  - Download and delete functionality
  - Soft delete with automatic file cleanup

- ✅ **Profile Wizard Step 6:** Social Links (3 endpoints)
  - Add/update/remove social media links
  - Completely optional (users can skip)
  - LinkedIn, GitHub, Behance, Dribbble, Personal Website

- ✅ **Reference Data:** Countries, Languages, Job Titles
  - Bilingual support (English/Arabic)
  - 90 job titles, 65 countries, 50 languages seeded

### Next Phase
- 📅 Profile Wizard Step 4: Work Experience
- 📅 Profile Wizard Step 5: Education
- 📅 Job posting and management
- 📅 AI-powered recommendations

---

## Quick Links

### For Frontend Developers (START HERE)
👉 **[React Integration Guide](Guides/REACT_INTEGRATION_GUIDE.md)** - Complete React/TypeScript integration with examples  
👉 **[Auth API Integration Guide](API/AUTH_API_INTEGRATION.md)** - Authentication endpoints with request/response examples  
👉 **[Google OAuth Guide](Guides/GOOGLE_AUTH_GUIDE.md)** - Google Sign-In implementation guide

### For API Documentation & Standards
👉 **[API Reference](API/API_REFERENCE.md)** - Complete endpoint documentation with validation rules  
👉 **[Swagger Improvements](API/SWAGGER_IMPROVEMENTS.md)** - Swagger/OpenAPI documentation guide  
👉 **[API Documentation Standards](API/API_DOCUMENTATION_STANDARDS.md)** - Quick reference for maintaining consistent API docs

### For Setup & Configuration
👉 **[Setup Guide](Guides/SETUP_GUIDE.md)** - Project setup, database, email configuration  
👉 **[Email Setup Guide](Guides/EMAIL_SETUP_GUIDE.md)** - Gmail SMTP configuration

### For Database
👉 **[ERD Diagram](Database/ERD_DIAGRAM.md)** - Entity Relationship Diagram with complete schema
👉 **[ERD in DBML Format](Database/ERD_dbdiagram.dbml)** - Database Markup Language format
👉 **[ERD in PlantUML Format](Database/ERD_PlantUML.puml)** - PlantUML diagram source

### Project Management
👉 **[Changelog](CHANGELOG.md)** - Version history and recent changes
👉 **[Documentation Guide](Guides/DOCS_GUIDE.md)** - How to maintain documentation

---

## 📁 Documentation Structure

```
Docs/
├── README.md                          # This file - Documentation index
├── CHANGELOG.md                       # Version history
│
├── Guides/                            # Setup & Integration Guides
│   ├── SETUP_GUIDE.md                # Project setup instructions
│   ├── EMAIL_SETUP_GUIDE.md          # Gmail SMTP configuration
│   ├── GOOGLE_AUTH_GUIDE.md          # Google OAuth setup
│   ├── REACT_INTEGRATION_GUIDE.md    # Frontend integration guide
│   └── DOCS_GUIDE.md                 # Documentation maintenance
│
├── API/                               # API Documentation
│   ├── API_REFERENCE.md              # Complete endpoint reference
│   ├── API_DOCUMENTATION_STANDARDS.md # API documentation standards
│   ├── AUTH_API_INTEGRATION.md       # Authentication API guide
│   └── SWAGGER_IMPROVEMENTS.md       # Swagger documentation guide
│
└── Database/                          # Database Documentation
    ├── ERD_DIAGRAM.md                # Main ERD documentation
    ├── ERD_dbdiagram.dbml            # DBML format for dbdiagram.io
    └── ERD_PlantUML.puml             # PlantUML format
```

---

## 📋 Documentation Quick Reference

### Essential Reading Order for New Developers

1. **[Guides/SETUP_GUIDE.md](Guides/SETUP_GUIDE.md)** - Start here!
   - Project setup instructions
   - Database configuration
   - Email configuration
   - Running the application
   - Troubleshooting

2. **[Guides/REACT_INTEGRATION_GUIDE.md](Guides/REACT_INTEGRATION_GUIDE.md)** - For Frontend Developers
   - Complete React/TypeScript integration guide
   - Security features (password policy, account lockout)
   - Full component examples (Register, Login with lockout handling)
   - Custom React hooks (useAccountLockout)
   - API service implementation with interceptors
   - Error handling patterns and best practices

3. **[API/AUTH_API_INTEGRATION.md](API/AUTH_API_INTEGRATION.md)** - Authentication Quick Start
   - Authentication endpoints (Email + Google OAuth)
   - Request/response examples
   - Security notes
   - cURL examples

4. **[API/API_REFERENCE.md](API/API_REFERENCE.md)** - Complete API Reference
   - All 19 API endpoints documented
   - Request/response formats
   - Validation rules
   - Status codes
   - Testing examples

5. **[Guides/GOOGLE_AUTH_GUIDE.md](Guides/GOOGLE_AUTH_GUIDE.md)** - Google OAuth Setup
   - Google Cloud Console configuration
   - Frontend implementation
   - Security best practices
   - Testing and troubleshooting

6. **[Database/ERD_DIAGRAM.md](Database/ERD_DIAGRAM.md)** - Database Schema
   - Entity Relationship Diagram
   - 18 tables with relationships
   - Complete schema documentation
   - Alternative formats: DBML and PlantUML

### Supporting Documentation

- **[CHANGELOG.md](CHANGELOG.md)** - Version history (current: 1.4.0)
- **[Guides/EMAIL_SETUP_GUIDE.md](Guides/EMAIL_SETUP_GUIDE.md)** - Gmail SMTP configuration
- **[API/API_DOCUMENTATION_STANDARDS.md](API/API_DOCUMENTATION_STANDARDS.md)** - API documentation standards
- **[API/SWAGGER_IMPROVEMENTS.md](API/SWAGGER_IMPROVEMENTS.md)** - Swagger documentation guide
- **[Guides/DOCS_GUIDE.md](Guides/DOCS_GUIDE.md)** - How to maintain documentation

---

## Getting Started

### For Frontend Developers
1. Start with [Guides/REACT_INTEGRATION_GUIDE.md](Guides/REACT_INTEGRATION_GUIDE.md)
2. Reference [API/API_REFERENCE.md](API/API_REFERENCE.md) for endpoint details
3. Check [Guides/SETUP_GUIDE.md](Guides/SETUP_GUIDE.md) if you need to run backend locally

### For Backend Developers
1. Follow [Guides/SETUP_GUIDE.md](Guides/SETUP_GUIDE.md) to set up the project
2. Use [API/API_REFERENCE.md](API/API_REFERENCE.md) for API implementation details
3. Refer to [Guides/EMAIL_SETUP_GUIDE.md](Guides/EMAIL_SETUP_GUIDE.md) for email configuration
4. Review [Database/ERD_DIAGRAM.md](Database/ERD_DIAGRAM.md) for database schema

---

## API Quick Reference

**Base URL:** `http://localhost:5217`  
**Swagger UI:** `http://localhost:5217/swagger` (Development only)

### Authentication Endpoints (9)

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login with email/password
- `POST /api/auth/google` - Login/Register with Google ⭐ NEW
- `POST /api/auth/verify-email` - Verify email with OTP
- `POST /api/auth/resend-verification` - Resend verification code
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/verify-reset-otp` - Verify reset OTP
- `POST /api/auth/reset-password` - Reset password
- `GET /api/auth/me` - Get current user (protected)

---

## Frontend Integration Summary

### 🎯 What Frontend Team Needs to Know

#### Security Features Implemented
1. **Password Policy** - Uppercase, lowercase, digit, 8-100 characters
2. **Account Lockout** - 5 failed attempts = 15 min lockout
3. **OTP Verification** - 6-digit codes, 15 min expiry
4. **JWT Authentication** - 24 hour tokens
5. **Email Enumeration Prevention** - Consistent responses

#### Critical Implementation Points

**Account Lockout Response:**
```json
{
  "success": false,
  "message": "Account is locked...",
  "lockoutEnd": "2025-12-07T15:45:00Z",
  "remainingMinutes": 12
}
```

**Password Validation Regex:**
```javascript
/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,100}$/
```

#### Required UI Components
- ✅ Login with lockout handling and countdown timer
- ✅ Registration with real-time password validation
- ✅ Email verification with resend limiter
- ✅ Password reset flow (3 steps)
- ✅ Google OAuth button
- ✅ Account lockout alerts with unlock options

#### Token Management
```typescript
// Store token after successful login
localStorage.setItem('token', response.token);

// Add to all authenticated requests
headers: {
  'Authorization': `Bearer ${token}`
}

// Handle token expiry (401 response)
if (error.response?.status === 401) {
  localStorage.removeItem('token');
  navigate('/login');
}
```

#### Complete Integration Checklist
See [REACT_INTEGRATION_GUIDE.md](REACT_INTEGRATION_GUIDE.md#frontend-integration-checklist) for the full checklist with 50+ items covering:
- All authentication features
- Security implementations
- Error handling
- User experience
- Testing scenarios

---

## Project Status

✅ **Authentication System:** Complete (Email/Password + Google OAuth 2.0)  
✅ **Security Features:** Complete (Account Lockout + Password Policy)  
✅ **API Documentation:** Complete and up-to-date  
✅ **Frontend Integration:** Ready (React examples provided)  
✅ **Database:** Schema organized, migrations cleaned  
🔄 **Next Phase:** Profile completion wizard, job posting features

---

**Questions?** Check the relevant guide above or review Swagger UI for interactive API testing.
