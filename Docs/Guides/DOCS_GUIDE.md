# Documentation Guide

**Purpose:** This guide explains all documentation files in the Docs folder and helps you navigate them based on your role.

**Last Updated:** February 2026

---

## Documentation Files Overview

| File | Purpose | Primary Audience |
|------|---------|------------------|
| [README.md](../README.md) (root) | Project overview and quick start | Everyone |
| [Docs/README.md](../Docs/README.md) | Documentation index | Everyone |
| [API/API_REFERENCE.md](../API/API_REFERENCE.md) | Complete API endpoint documentation | Frontend & Backend |
| [API/AUTH_API_INTEGRATION.md](../API/AUTH_API_INTEGRATION.md) | Auth handoff for frontend team | Frontend Developers |
| [API/API_DOCUMENTATION_STANDARDS.md](../API/API_DOCUMENTATION_STANDARDS.md) | API documentation standards | Backend Developers |
| [API/SWAGGER_IMPROVEMENTS.md](../API/SWAGGER_IMPROVEMENTS.md) | Swagger documentation history | Backend Developers |
| [GOOGLE_AUTH_GUIDE.md](GOOGLE_AUTH_GUIDE.md) | Google OAuth 2.0 setup | Frontend Developers |
| [SETUP_GUIDE.md](SETUP_GUIDE.md) | Project setup and configuration | Backend Developers |
| [TEAMMATE_SETUP_GUIDE.md](TEAMMATE_SETUP_GUIDE.md) | Quick 10-min setup | New Developers |
| [EMAIL_SETUP_GUIDE.md](EMAIL_SETUP_GUIDE.md) | Email/SMTP configuration | Backend Developers |
| [JOBS_MODULE_IMPLEMENTATION_GUIDE.md](JOBS_MODULE_IMPLEMENTATION_GUIDE.md) | Jobs module plan | Backend Developers |
| [JOBS_MODULE_QUICK_GUIDE.md](JOBS_MODULE_QUICK_GUIDE.md) | Jobs module quick ref | Backend Developers |
| [../Database/ERD_DIAGRAM.md](../Database/ERD_DIAGRAM.md) | Database ERD (19 tables) | Backend Developers |
| [../CHANGELOG.md](../CHANGELOG.md) | Version history | Everyone |

---

## Where to Start

### For Frontend Developers

**Start here: [API/AUTH_API_INTEGRATION.md](../API/AUTH_API_INTEGRATION.md)**

Comprehensive guide with:
- All 9 authentication endpoints with request/response examples
- Password reset flow (token-based link, not OTP)
- Error handling patterns
- TypeScript interfaces
- cURL examples

**Then read:**
1. [API/API_REFERENCE.md](../API/API_REFERENCE.md) — All 56 endpoints documented
2. [GOOGLE_AUTH_GUIDE.md](GOOGLE_AUTH_GUIDE.md) — Google OAuth implementation

---

### For Backend Developers

**Start here: [SETUP_GUIDE.md](SETUP_GUIDE.md)**

Covers:
- Prerequisites and installation
- Database setup (SQL Server LocalDB)
- Configuration (JWT, Email, CORS)
- Running the project
- Testing with Swagger

**Then read:**
1. [../Database/ERD_DIAGRAM.md](../Database/ERD_DIAGRAM.md) — 19-table database schema
2. [EMAIL_SETUP_GUIDE.md](EMAIL_SETUP_GUIDE.md) — SMTP configuration
3. [API/API_REFERENCE.md](../API/API_REFERENCE.md) — Endpoint specifications

---

### For New Team Members

**Start here: [TEAMMATE_SETUP_GUIDE.md](TEAMMATE_SETUP_GUIDE.md)**

Quick 10-minute setup, then read:
1. [../CHANGELOG.md](../CHANGELOG.md) — Version history
2. [API/API_REFERENCE.md](../API/API_REFERENCE.md) — Understanding the API surface

---

## Quick Reference by Topic

| If you need to... | Read this file |
|-------------------|----------------|
| Set up the project quickly | [TEAMMATE_SETUP_GUIDE.md](TEAMMATE_SETUP_GUIDE.md) |
| Detailed project setup | [SETUP_GUIDE.md](SETUP_GUIDE.md) |
| Understand auth flows | [API/AUTH_API_INTEGRATION.md](../API/AUTH_API_INTEGRATION.md) |
| See all API endpoints | [API/API_REFERENCE.md](../API/API_REFERENCE.md) |
| Implement Google Sign-In | [GOOGLE_AUTH_GUIDE.md](GOOGLE_AUTH_GUIDE.md) |
| Configure email service | [EMAIL_SETUP_GUIDE.md](EMAIL_SETUP_GUIDE.md) |
| Understand database schema | [../Database/ERD_DIAGRAM.md](../Database/ERD_DIAGRAM.md) |
| See what changed recently | [../CHANGELOG.md](../CHANGELOG.md) |
| Implement Jobs module next | [JOBS_MODULE_IMPLEMENTATION_GUIDE.md](JOBS_MODULE_IMPLEMENTATION_GUIDE.md) |

---

## Project Status Summary

| Feature | Status | Documentation |
|---------|--------|---------------|
| Authentication (Email + Google) | Complete | [AUTH_API_INTEGRATION.md](../API/AUTH_API_INTEGRATION.md) |
| Job Seeker Wizard (4 steps) | Complete | [API_REFERENCE.md](../API/API_REFERENCE.md) |
| Recruiter Profile | Complete | [API_REFERENCE.md](../API/API_REFERENCE.md) |
| Reference Data | Complete | [API_REFERENCE.md](../API/API_REFERENCE.md) |
| Job Management | Complete | [API_REFERENCE.md](../API/API_REFERENCE.md) |
