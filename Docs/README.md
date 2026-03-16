# Documentation Index

**Project:** JobIntel Recruitment Platform — Backend API
**Version:** 1.5.1
**Last Updated:** February 2026

---

## Documentation Status

All documentation updated and verified (February 2026).

**Backend Version:** 1.5.1
**Endpoints:** 56 across 9 controllers
**Status:** Production Ready for Frontend Integration

### Current Implementation
- **Authentication System:** Complete (9 endpoints)
  - Email/Password with verification
  - Google OAuth integration
  - Password reset flow (cryptographic token link)
  - Account lockout protection

- **Job Seeker Profile Wizard (4 steps):** Complete (35 endpoints)
  - Step 1: Personal Information (name, job title, location, bio, languages)
  - Step 2: Experience + Education (CRUD with soft delete & reorder)
  - Step 3: Projects Portfolio (CRUD with soft delete & auto-reorder)
  - Step 4: Skills + Social Links + Certificates
  - Resume upload (PDF with magic bytes validation, max 5MB)
  - Profile Picture (upload, download, delete — JPEG/PNG/WebP, max 2MB)

- **Recruiter Profile:** Complete (10 endpoints)
  - Company info, wizard status, industries, company sizes, profile picture

- **Reference Data:** Complete (2 endpoints)
  - Countries, Languages (bilingual EN/AR)
  - 90 job titles, 65 countries, 50 languages seeded

### Next Phase
- Assessment runtime APIs
- AI-powered recommendations
- Job seeker-side job discovery and applications

---

## Quick Links

### For Frontend Developers (START HERE)
- **[Auth API Integration Guide](API/AUTH_API_INTEGRATION.md)** — Authentication endpoints with request/response examples
- **[Google OAuth Guide](Guides/GOOGLE_AUTH_GUIDE.md)** — Google Sign-In implementation guide

### For API Documentation & Standards
- **[API Reference](API/API_REFERENCE.md)** — Complete endpoint documentation with validation rules
- **[Swagger Improvements](API/SWAGGER_IMPROVEMENTS.md)** — Swagger/OpenAPI documentation guide
- **[API Documentation Standards](API/API_DOCUMENTATION_STANDARDS.md)** — Quick reference for maintaining consistent API docs

### For Setup & Configuration
- **[Teammate Setup Guide](Guides/TEAMMATE_SETUP_GUIDE.md)** — Streamlined 10-minute setup for new developers
- **[Setup Guide](Guides/SETUP_GUIDE.md)** — Detailed project setup with troubleshooting
- **[Email Setup Guide](Guides/EMAIL_SETUP_GUIDE.md)** — Gmail SMTP configuration
- **[Jobs Module Guide](Guides/JOBS_MODULE_IMPLEMENTATION_GUIDE.md)** — Next implementation step

### For Database
- **[ERD Diagram](Database/ERD_DIAGRAM.md)** — Entity Relationship Diagram with complete schema
- **[ERD in DBML Format](Database/ERD_dbdiagram.dbml)** — Database Markup Language format
- **[ERD in PlantUML Format](Database/ERD_PlantUML.puml)** — PlantUML diagram source

### Project Management
- **[Changelog](CHANGELOG.md)** — Version history and recent changes
- **[Documentation Guide](Guides/DOCS_GUIDE.md)** — How to maintain documentation
- **[Class Diagram](Diagrams/CLASS_DIAGRAM.md)** — Full Mermaid class diagram

---

## Documentation Structure

```
Docs/
+-- README.md                          # This file - Documentation index
+-- CHANGELOG.md                       # Version history
+-- PROJECT_OVERVIEW.md                # Quick onboarding for new members
+-- PROJECT_DETAILED_GUIDE.md          # Deep technical reference
|
+-- API/                               # API Documentation
|   +-- API_REFERENCE.md              # Complete endpoint reference
|   +-- AUTH_API_INTEGRATION.md       # Auth handoff for frontend team
|   +-- API_DOCUMENTATION_STANDARDS.md # API documentation standards
|   +-- SWAGGER_IMPROVEMENTS.md       # Swagger documentation guide
|
+-- Database/                          # Database Documentation
|   +-- ERD_DIAGRAM.md               # Main ERD (Markdown + Mermaid)
|   +-- ERD_dbdiagram.dbml           # DBML format for dbdiagram.io
|   +-- ERD_PlantUML.puml            # PlantUML format
|
+-- Diagrams/                          # Architecture Diagrams
|   +-- CLASS_DIAGRAM.md             # Mermaid class diagram
|
+-- Guides/                            # Setup & Integration Guides
|   +-- TEAMMATE_SETUP_GUIDE.md      # Quick setup for new devs
|   +-- SETUP_GUIDE.md               # Detailed setup instructions
|   +-- EMAIL_SETUP_GUIDE.md         # Gmail SMTP configuration
|   +-- GOOGLE_AUTH_GUIDE.md         # Google OAuth setup
|   +-- DOCS_GUIDE.md               # Documentation maintenance
|   +-- JOBS_MODULE_IMPLEMENTATION_GUIDE.md  # Jobs module plan
|   +-- JOBS_MODULE_QUICK_GUIDE.md          # Jobs module quick ref
|
+-- Time Plan/
    +-- TimePlan_Updated.docx         # Project time plan
```

---

## Essential Reading Order for New Developers

1. **[Guides/TEAMMATE_SETUP_GUIDE.md](Guides/TEAMMATE_SETUP_GUIDE.md)** — 10-minute project setup
2. **[API/AUTH_API_INTEGRATION.md](API/AUTH_API_INTEGRATION.md)** — Authentication flows and frontend integration
3. **[API/API_REFERENCE.md](API/API_REFERENCE.md)** — All 56 API endpoints documented
4. **[Guides/GOOGLE_AUTH_GUIDE.md](Guides/GOOGLE_AUTH_GUIDE.md)** — Google OAuth implementation
5. **[Database/ERD_DIAGRAM.md](Database/ERD_DIAGRAM.md)** — 19-table database schema

---

## Getting Started

### For Frontend Developers
1. Read [API/AUTH_API_INTEGRATION.md](API/AUTH_API_INTEGRATION.md) for auth flow
2. Reference [API/API_REFERENCE.md](API/API_REFERENCE.md) for endpoint details
3. Check [Guides/TEAMMATE_SETUP_GUIDE.md](Guides/TEAMMATE_SETUP_GUIDE.md) to run backend locally

### For Backend Developers
1. Follow [Guides/SETUP_GUIDE.md](Guides/SETUP_GUIDE.md) to set up the project
2. Use [API/API_REFERENCE.md](API/API_REFERENCE.md) for API implementation details
3. Refer to [Guides/EMAIL_SETUP_GUIDE.md](Guides/EMAIL_SETUP_GUIDE.md) for email configuration
4. Review [Database/ERD_DIAGRAM.md](Database/ERD_DIAGRAM.md) for database schema
5. See [Guides/JOBS_MODULE_IMPLEMENTATION_GUIDE.md](Guides/JOBS_MODULE_IMPLEMENTATION_GUIDE.md) for the next module

---

## API Quick Reference

**Base URL:** `http://localhost:5217`
**Swagger UI:** `http://localhost:5217/swagger` (Development only)

| Category | Route Prefix | Endpoints | Description |
|----------|-------------|-----------|-------------|
| Authentication | `/api/auth/` | 9 | Register, login, OAuth, verify, reset |
| Job Seeker | `/api/jobseeker/` | 35 | Profile wizard (4 steps) + profile picture |
| Recruiter | `/api/recruiter/` | 10 | Company info + profile picture |
| Reference Data | `/api/locations/` | 2 | Countries, languages (bilingual) |

---

## Project Status

- **Authentication System:** Complete (Email/Password + Google OAuth 2.0)
- **Security Features:** Complete (Account Lockout + Password Policy + Constant-Time Comparison)
- **Job Seeker Profile:** Complete (all 4 wizard steps)
- **Recruiter Profile:** Complete (company info + picture)
- **API Documentation:** Updated February 2026
- **Next Phase:** Assessment runtime + recommendation modules

---

**Questions?** Check the relevant guide above or review Swagger UI at `http://localhost:5217/swagger`.
