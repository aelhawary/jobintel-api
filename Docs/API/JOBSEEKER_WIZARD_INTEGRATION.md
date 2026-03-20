# Job Seeker Profile Wizard — Frontend Integration Guide

**Last Updated:** March 2026
**Base URL:** `http://localhost:5217/api`

---

## Table of Contents

1. [Overview](#overview)
2. [Architectural Overview](#architectural-overview)
3. [Response Format](#response-format)
4. [Enum Reference](#enum-reference)
5. [Wizard Status & Advancement](#wizard-status--advancement)
6. [Step 1: Personal Info & Files](#step-1-personal-info--files)
7. [Step 2: Professional Background](#step-2-professional-background)
8. [Step 3: Projects](#step-3-projects)
9. [Step 4: Skills & Extras](#step-4-skills--extras)
10. [Reference Data Endpoints](#reference-data-endpoints)
11. [Tricky Parts](#tricky-parts)
12. [React Integration](#react-integration)
13. [Error Handling Patterns](#error-handling-patterns)
14. [Troubleshooting](#troubleshooting)

---

## Overview

The Profile Completion Wizard guides newly registered Job Seekers through **4 mandatory steps** to complete their profile before they can apply for jobs.

| Step | Title | Key Endpoints | Required? |
|------|-------|---------------|-----------|
| **1** | Personal Info & CV | `/personal-info`, `/picture`, `/resume` | Yes |
| **2** | Professional Background | `/experience`, `/education` | Optional* |
| **3** | Portfolio | `/projects` | Optional |
| **4** | Skills & Extras | `/skills`, `/social-accounts`, `/certificates` | Optional |

> **\*Note:** All steps must be **advanced** explicitly (user clicks "Next" or "Skip"), but adding data is optional for Steps 2-4.

### Authentication Requirements

All wizard endpoints (except reference data) require:
```
Authorization: Bearer <jwt_token>
```

The user must have `AccountType: JobSeeker` (role claim in JWT).

---

## Architectural Overview

### The "Paging vs Saving" Model

**Critical Concept:** Saving data does NOT automatically advance the wizard step.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        WIZARD STEP FLOW                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   [Save Data]           →    Stores entity only (experience, etc.)      │
│   POST /experience           Does NOT change ProfileCompletionStep      │
│                                                                         │
│   [Advance Step]        →    Moves wizard forward                       │
│   POST /wizard/advance/2     Updates ProfileCompletionStep to 2         │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   User clicks "Add Experience"  →  POST /experience (save data)         │
│   User clicks "Next Step"       →  POST /wizard/advance/2 (advance)     │
│   User clicks "Skip"            →  POST /wizard/advance/2 (same call)   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Why This Design?

1. **Multiple entries:** Users can add 3 jobs, 2 degrees without triggering step changes
2. **Skip support:** Users can skip Steps 2-4 without adding any data
3. **Save-as-you-go:** Data persists even if user leaves mid-wizard
4. **Clear control flow:** Frontend has explicit control over navigation

### Wizard Step Tracking

The backend tracks progress via `ProfileCompletionStep` (integer 0-4):

| Value | Status | User Can Access |
|-------|--------|-----------------|
| `0` | New user (hasn't started Step 1) | Wizard only |
| `1` | Completed Step 1 | Wizard + limited features |
| `2` | Completed Step 2 | Wizard + limited features |
| `3` | Completed Step 3 | Wizard + limited features |
| `4` | **Fully Complete** | All platform features |

> **Google OAuth Users:** Start at `ProfileCompletionStep = 0` just like email/password users. The wizard flow is identical.

---

## Response Format

### Standard API Response (Success)

All endpoints wrap data in `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* payload */ }
}
```

### Error Response (Business Logic)

```json
{
  "success": false,
  "message": "Validation failed: Job title ID is invalid"
}
```

### Error Response (Model Validation - ASP.NET)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "JobTitleId": ["Job title is required"],
    "City": ["City must be between 2 and 100 characters"]
  }
}
```

> **Tip:** Check for `errors` object first (ASP.NET validation), then fall back to `message` (business logic).

---

## Enum Reference

### Degree (Education)

| Value | Name |
|-------|------|
| `1` | HighSchool |
| `2` | Diploma |
| `3` | Associate |
| `4` | Bachelor |
| `5` | Master |
| `6` | PhD |
| `7` | Other |

### LanguageProficiency

| Value | Name |
|-------|------|
| `1` | Beginner |
| `2` | Intermediate |
| `3` | Advanced |
| `4` | Native |

---

## Wizard Status & Advancement

### GET Wizard Status

Call on app load to determine where to redirect the user.

```http
GET /api/jobseeker/wizard-status
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "currentStep": 2,
    "isComplete": false,
    "stepName": "Projects",
    "completedSteps": ["Personal Info & CV", "Professional Background"]
  }
}
```

**Frontend Logic:**
```typescript
if (wizardStatus.isComplete) {
  navigate('/dashboard');
} else {
  navigate(`/wizard/step-${wizardStatus.currentStep + 1}`);
}
```

---

### POST Advance Wizard Step

Call when user clicks **"Next"** or **"Skip"**.

```http
POST /api/jobseeker/wizard/advance/{stepNumber}
Authorization: Bearer <token>
```

**Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `stepNumber` | int (path) | The step to mark as completed (1-4) |

**Example:** Finish Step 2 → `POST /api/jobseeker/wizard/advance/2`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Wizard step advanced successfully.",
  "data": {
    "success": true,
    "message": "Wizard step advanced successfully.",
    "profileCompletionStep": 2
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Cannot skip steps. Please complete step 1 first."
}
```

> **Important:** Steps are forward-only. You cannot advance to Step 3 until Step 2 is completed.

---

## Step 1: Personal Info & Files

**Goal:** Collect basic profile details, profile picture, and resume.

### 1.1 Save Personal Info

```http
POST /api/jobseeker/personal-info
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "jobTitleId": 12,
  "yearsOfExperience": 3,
  "countryId": 1,
  "city": "Cairo",
  "phoneNumber": "+201234567890",
  "firstLanguageId": 1,
  "firstLanguageProficiency": 4,
  "secondLanguageId": 2,
  "secondLanguageProficiency": 3,
  "bio": "Passionate software developer with 3+ years of experience..."
}
```

**Field Validation:**

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `jobTitleId` | int | Yes | Must exist in JobTitles table |
| `yearsOfExperience` | int | Yes | 0-50 |
| `countryId` | int | Yes | Must exist in Countries table |
| `city` | string | Yes | 2-100 chars, auto-normalized to Title Case |
| `phoneNumber` | string | No | Valid phone format, max 20 chars |
| `firstLanguageId` | int | Yes | Must exist in Languages table |
| `firstLanguageProficiency` | enum | Yes | 1-4 (Beginner to Native) |
| `secondLanguageId` | int | No | Must differ from firstLanguageId |
| `secondLanguageProficiency` | enum | Conditional | Required if secondLanguageId provided |
| `bio` | string | No | Max 500 chars |

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Personal information saved successfully",
  "data": {
    "success": true,
    "message": "Personal information saved successfully",
    "profileCompletionStep": 1
  }
}
```

> **Note:** This endpoint automatically advances to Step 1 if user was at Step 0.

---

### 1.2 Get Personal Info

```http
GET /api/jobseeker/personal-info?lang=en
Authorization: Bearer <token>
```

**Query Parameters:**
| Param | Default | Description |
|-------|---------|-------------|
| `lang` | `en` | `en` for English, `ar` for Arabic names |

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "jobTitleId": 12,
    "jobTitle": "Backend Developer",
    "yearsOfExperience": 3,
    "countryId": 1,
    "country": "Egypt",
    "city": "Cairo",
    "phoneNumber": "+201234567890",
    "firstLanguageId": 1,
    "firstLanguage": "Arabic",
    "firstLanguageProficiency": 4,
    "secondLanguageId": 2,
    "secondLanguage": "English",
    "secondLanguageProficiency": 3,
    "bio": "Passionate software developer..."
  }
}
```

---

### 1.3 Upload Profile Picture

```http
POST /api/jobseeker/picture
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

**Form Data:**
| Field | Type | Validation |
|-------|------|------------|
| `file` | File | JPEG, PNG, or WebP. Max 2MB |

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Profile picture uploaded successfully",
  "url": "/api/jobseeker/picture",
  "fileName": "profile_123.jpg",
  "hasProfilePicture": true,
  "isOAuthPicture": false
}
```

**Error Response (400):**
```json
{
  "success": false,
  "message": "Invalid file format. Only JPEG, PNG, and WebP are allowed."
}
```

---

### 1.4 Get Profile Picture

```http
GET /api/jobseeker/picture
Authorization: Bearer <token>
```

**Response:** Returns the image file directly, or redirects to OAuth picture URL.

---

### 1.5 Delete Profile Picture

```http
DELETE /api/jobseeker/picture
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Profile picture deleted successfully",
  "data": true
}
```

---

### 1.6 Upload Resume (CV)

```http
POST /api/jobseeker/resume/upload
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

**Form Data:**
| Field | Type | Validation |
|-------|------|------------|
| `file` | File | PDF only. Max 5MB |

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Resume uploaded successfully",
  "resume": {
    "fileName": "John_Doe_Resume.pdf",
    "originalFileName": "resume.pdf",
    "fileSizeBytes": 245760,
    "fileSizeDisplay": "240 KB",
    "downloadUrl": "/api/jobseeker/resume/download",
    "uploadedAt": "2026-03-19T10:30:00Z"
  }
}
```

---

### 1.7 Get Resume Info

```http
GET /api/jobseeker/resume
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Resume exists",
  "resume": {
    "fileName": "John_Doe_Resume.pdf",
    "originalFileName": "resume.pdf",
    "fileSizeBytes": 245760,
    "fileSizeDisplay": "240 KB",
    "downloadUrl": "/api/jobseeker/resume/download",
    "uploadedAt": "2026-03-19T10:30:00Z"
  }
}
```

---

### 1.8 Download Resume

```http
GET /api/jobseeker/resume/download
Authorization: Bearer <token>
```

**Response:** Returns PDF file with `Content-Disposition: attachment`.

---

### 1.9 Delete Resume

```http
DELETE /api/jobseeker/resume
Authorization: Bearer <token>
```

---

### 1.10 Check Resume Exists

```http
GET /api/jobseeker/resume/exists
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Resume exists",
  "data": true
}
```

---

**After Step 1 is complete, call:**
```http
POST /api/jobseeker/wizard/advance/1
```

---

## Step 2: Professional Background

**Goal:** Add work experience and education entries.

### 2.1 Add Experience

```http
POST /api/jobseeker/experience
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "jobTitle": "Senior Product Designer",
  "companyName": "TechFlow Inc.",
  "location": "San Francisco, CA",
  "startDate": "2021-01-01",
  "endDate": null,
  "isCurrent": true,
  "displayOrder": 0
}
```

**Field Validation:**

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `jobTitle` | string | Yes | Max 100 chars |
| `companyName` | string | Yes | Max 100 chars |
| `location` | string | No | Max 150 chars |
| `startDate` | date | Yes | ISO 8601 format |
| `endDate` | date | No | Must be after startDate, null if current |
| `isCurrent` | bool | No | If true, endDate should be null |
| `displayOrder` | int | No | Lower = appears first |

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Experience added successfully",
  "data": {
    "id": 1,
    "jobTitle": "Senior Product Designer",
    "companyName": "TechFlow Inc.",
    "location": "San Francisco, CA",
    "startDate": "2021-01-01T00:00:00",
    "endDate": null,
    "isCurrent": true,
    "displayOrder": 0,
    "dateRange": "Jan 2021 - Present",
    "createdAt": "2026-03-19T10:30:00Z",
    "updatedAt": "2026-03-19T10:30:00Z"
  }
}
```

---

### 2.2 Get All Experiences

```http
GET /api/jobseeker/experience
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "experiences": [
      {
        "id": 1,
        "jobTitle": "Senior Product Designer",
        "companyName": "TechFlow Inc.",
        "location": "San Francisco, CA",
        "startDate": "2021-01-01T00:00:00",
        "endDate": null,
        "isCurrent": true,
        "displayOrder": 0,
        "dateRange": "Jan 2021 - Present",
        "createdAt": "2026-03-19T10:30:00Z",
        "updatedAt": "2026-03-19T10:30:00Z"
      }
    ],
    "totalCount": 1
  }
}
```

---

### 2.3 Get Single Experience

```http
GET /api/jobseeker/experience/{id}
Authorization: Bearer <token>
```

---

### 2.4 Update Experience

```http
PUT /api/jobseeker/experience/{id}
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:** Same as Add Experience.

---

### 2.5 Delete Experience

```http
DELETE /api/jobseeker/experience/{id}
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Experience deleted successfully",
  "data": true
}
```

---

### 2.6 Reorder Experiences

```http
POST /api/jobseeker/experience/reorder
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
[3, 1, 2]  // Array of experience IDs in desired order
```

---

### 2.7 Check Has Experience

```http
GET /api/jobseeker/experience/exists
Authorization: Bearer <token>
```

---

### 2.8 Add Education

```http
POST /api/jobseeker/education
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "institution": "Stanford University",
  "degree": 4,
  "fieldOfStudy": "Computer Science",
  "startDate": "2017-09-01",
  "endDate": "2021-06-01",
  "isCurrent": false,
  "displayOrder": 0
}
```

**Field Validation:**

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `institution` | string | Yes | Max 150 chars |
| `degree` | enum | Yes | 1-7 (see Degree enum) |
| `fieldOfStudy` | string | Yes | Max 150 chars |
| `startDate` | date | Yes | ISO 8601 format |
| `endDate` | date | No | Must be after startDate |
| `isCurrent` | bool | No | If true, endDate should be null |
| `displayOrder` | int | No | Lower = appears first |

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Education added successfully",
  "data": {
    "id": 1,
    "institution": "Stanford University",
    "degree": 4,
    "fieldOfStudy": "Computer Science",
    "startDate": "2017-09-01T00:00:00",
    "endDate": "2021-06-01T00:00:00",
    "isCurrent": false,
    "displayOrder": 0,
    "dateRange": "Sep 2017 - Jun 2021",
    "createdAt": "2026-03-19T10:30:00Z",
    "updatedAt": "2026-03-19T10:30:00Z"
  }
}
```

---

### 2.9 Get All Education

```http
GET /api/jobseeker/education
Authorization: Bearer <token>
```

---

### 2.10 Get Single Education

```http
GET /api/jobseeker/education/{id}
Authorization: Bearer <token>
```

---

### 2.11 Update Education

```http
PUT /api/jobseeker/education/{id}
Authorization: Bearer <token>
Content-Type: application/json
```

---

### 2.12 Delete Education

```http
DELETE /api/jobseeker/education/{id}
Authorization: Bearer <token>
```

---

### 2.13 Reorder Education

```http
POST /api/jobseeker/education/reorder
Authorization: Bearer <token>
Content-Type: application/json
```

---

### 2.14 Check Has Education

```http
GET /api/jobseeker/education/exists
Authorization: Bearer <token>
```

---

**After Step 2 (or to skip), call:**
```http
POST /api/jobseeker/wizard/advance/2
```

---

## Step 3: Projects

**Goal:** Add portfolio projects (optional).

### 3.1 Add Project

```http
POST /api/jobseeker/projects
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "E-Commerce Platform",
  "technologiesUsed": "React, Node.js, MongoDB, Stripe",
  "description": "Full-stack e-commerce platform with payment integration and admin dashboard",
  "projectLink": "https://github.com/username/ecommerce-platform"
}
```

**Field Validation:**

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `title` | string | Yes | Max 150 chars |
| `technologiesUsed` | string | No | Max 300 chars |
| `description` | string | No | Max 1200 chars |
| `projectLink` | string | No | Valid URL, max 300 chars |

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Project added successfully",
  "project": {
    "id": 1,
    "title": "E-Commerce Platform",
    "technologiesUsed": "React, Node.js, MongoDB, Stripe",
    "description": "Full-stack e-commerce platform...",
    "projectLink": "https://github.com/username/ecommerce-platform",
    "displayOrder": 1,
    "createdAt": "2026-03-19T10:30:00Z",
    "updatedAt": "2026-03-19T10:30:00Z"
  }
}
```

---

### 3.2 Get All Projects

```http
GET /api/jobseeker/projects
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "title": "E-Commerce Platform",
      "technologiesUsed": "React, Node.js, MongoDB, Stripe",
      "description": "Full-stack e-commerce platform...",
      "projectLink": "https://github.com/username/ecommerce-platform",
      "displayOrder": 1,
      "createdAt": "2026-03-19T10:30:00Z",
      "updatedAt": "2026-03-19T10:30:00Z"
    }
  ]
}
```

---

### 3.3 Update Project

```http
PUT /api/jobseeker/projects/{projectId}
Authorization: Bearer <token>
Content-Type: application/json
```

---

### 3.4 Delete Project

```http
DELETE /api/jobseeker/projects/{projectId}
Authorization: Bearer <token>
```

---

**After Step 3 (or to skip), call:**
```http
POST /api/jobseeker/wizard/advance/3
```

---

## Step 4: Skills & Extras

**Goal:** Add skills, social links, and certificates.

### 4.1 Get Available Skills

```http
GET /api/jobseeker/skills/available
```

> **Note:** This endpoint is **public** (no auth required). Use for populating skill selection UI.

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Found 150 available skills",
  "data": [
    { "id": 1, "name": "JavaScript" },
    { "id": 2, "name": "TypeScript" },
    { "id": 3, "name": "React" },
    { "id": 4, "name": "Node.js" },
    { "id": 5, "name": "Python" }
  ]
}
```

---

### 4.2 Get User's Skills

```http
GET /api/jobseeker/skills
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Skills retrieved successfully",
  "skills": [
    { "id": 1, "name": "JavaScript" },
    { "id": 3, "name": "React" }
  ],
  "totalCount": 2
}
```

---

### 4.3 Update Skills (Replace All)

```http
PUT /api/jobseeker/skills
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "skillIds": [1, 3, 4, 15, 22]
}
```

> **Important:** This **replaces** all existing skills. Send the complete desired list.

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Skills updated successfully",
  "skills": [
    { "id": 1, "name": "JavaScript" },
    { "id": 3, "name": "React" },
    { "id": 4, "name": "Node.js" },
    { "id": 15, "name": "PostgreSQL" },
    { "id": 22, "name": "Docker" }
  ],
  "totalCount": 5
}
```

---

### 4.4 Clear All Skills

```http
DELETE /api/jobseeker/skills
Authorization: Bearer <token>
```

---

### 4.5 Get Social Links

```http
GET /api/jobseeker/social-accounts
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Social accounts retrieved successfully",
  "socialAccounts": {
    "linkedIn": "https://linkedin.com/in/johndoe",
    "github": "https://github.com/johndoe",
    "behance": null,
    "dribbble": null,
    "personalWebsite": "https://johndoe.dev",
    "createdAt": "2026-03-19T10:30:00Z",
    "updatedAt": "2026-03-19T10:30:00Z"
  }
}
```

---

### 4.6 Update Social Links

```http
PUT /api/jobseeker/social-accounts
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "linkedIn": "https://linkedin.com/in/johndoe",
  "github": "https://github.com/johndoe",
  "behance": null,
  "dribbble": null,
  "personalWebsite": "https://johndoe.dev"
}
```

**Field Validation:**

| Field | Type | Validation |
|-------|------|------------|
| `linkedIn` | string | Valid URL, max 300 chars |
| `github` | string | Valid URL, max 300 chars |
| `behance` | string | Valid URL, max 300 chars |
| `dribbble` | string | Valid URL, max 300 chars |
| `personalWebsite` | string | Valid URL, max 300 chars |

> **Note:** All fields are optional. Send `null` or omit to clear.

---

### 4.7 Delete All Social Links

```http
DELETE /api/jobseeker/social-accounts
Authorization: Bearer <token>
```

---

### 4.8 Get All Certificates

```http
GET /api/jobseeker/certificates
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Certificates retrieved successfully",
  "certificates": [
    {
      "id": 1,
      "title": "AWS Solutions Architect",
      "issuingOrganization": "Amazon Web Services",
      "issueDate": "2024-01-15T00:00:00",
      "expirationDate": "2027-01-15T00:00:00",
      "hasFile": true,
      "fileName": "aws-cert.pdf",
      "fileSizeBytes": 1258291,
      "fileSizeDisplay": "1.2 MB",
      "downloadUrl": "/api/jobseeker/certificates/1/download",
      "displayOrder": 1,
      "createdAt": "2026-03-19T10:30:00Z",
      "updatedAt": "2026-03-19T10:30:00Z"
    }
  ],
  "totalCount": 1
}
```

---

### 4.9 Add Certificate (with optional file)

```http
POST /api/jobseeker/certificates
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

**Form Data:**

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `title` | string | Yes | Max 150 chars |
| `issuingOrganization` | string | No | Max 150 chars |
| `issueDate` | date | No | ISO 8601 format |
| `expirationDate` | date | No | Must be after issueDate |
| `file` | File | No | PDF or DOCX, max 10MB |

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Certificate added successfully",
  "certificate": {
    "id": 1,
    "title": "AWS Solutions Architect",
    "issuingOrganization": "Amazon Web Services",
    "issueDate": "2024-01-15T00:00:00",
    "expirationDate": "2027-01-15T00:00:00",
    "hasFile": true,
    "fileName": "aws-cert.pdf",
    "fileSizeBytes": 1258291,
    "fileSizeDisplay": "1.2 MB",
    "downloadUrl": "/api/jobseeker/certificates/1/download",
    "displayOrder": 1,
    "createdAt": "2026-03-19T10:30:00Z",
    "updatedAt": "2026-03-19T10:30:00Z"
  }
}
```

---

### 4.10 Get Single Certificate

```http
GET /api/jobseeker/certificates/{id}
Authorization: Bearer <token>
```

---

### 4.11 Update Certificate

```http
PUT /api/jobseeker/certificates/{id}
Authorization: Bearer <token>
Content-Type: multipart/form-data
```

> **Note:** Providing a new file replaces the existing one.

---

### 4.12 Delete Certificate

```http
DELETE /api/jobseeker/certificates/{id}
Authorization: Bearer <token>
```

---

### 4.13 Download Certificate File

```http
GET /api/jobseeker/certificates/{id}/download
Authorization: Bearer <token>
```

**Response:** Returns file with `Content-Disposition: attachment`.

---

**After Step 4 (or to skip), call:**
```http
POST /api/jobseeker/wizard/advance/4
```

This marks the profile as **complete** (`ProfileCompletionStep = 4`).

---

## Reference Data Endpoints

These endpoints provide seed/lookup data for form dropdowns. **No authentication required.**

### When to Fetch Reference Data

```
┌────────────────────────────────────────────────────────────────┐
│                   REFERENCE DATA MAPPING                       │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│   STEP 1: Personal Info                                        │
│   ├── GET /api/jobseeker/job-titles  →  Job Title dropdown     │
│   ├── GET /api/locations/countries   →  Country dropdown       │
│   └── GET /api/locations/languages   →  Language dropdowns     │
│                                                                │
│   STEP 4: Skills                                               │
│   └── GET /api/jobseeker/skills/available → Skills multi-select│
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

| Data | Endpoint | Used In | Cache Strategy |
|------|----------|---------|----------------|
| **Job Titles** | `GET /api/jobseeker/job-titles` | Step 1 | localStorage (24h) |
| **Countries** | `GET /api/locations/countries` | Step 1 | localStorage (24h) |
| **Languages** | `GET /api/locations/languages` | Step 1 | localStorage (24h) |
| **Skills** | `GET /api/jobseeker/skills/available` | Step 4 | sessionStorage |

---

### GET Job Titles

```http
GET /api/jobseeker/job-titles
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    { "id": 1, "title": "Frontend Developer", "category": "Technology" },
    { "id": 2, "title": "Backend Developer", "category": "Technology" },
    { "id": 3, "title": "UI/UX Designer", "category": "Design" },
    { "id": 4, "title": "Product Manager", "category": "Operations" }
  ]
}
```

> **Tip:** 90 job titles across 8 categories. Consider grouping by `category` in UI.

---

### GET Countries

```http
GET /api/locations/countries?lang=en
```

**Query Parameters:**
| Param | Default | Description |
|-------|---------|-------------|
| `lang` | `en` | `en` or `ar` for localized names |

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "Egypt", "isoCode": "EG", "phoneCode": "+20" },
    { "id": 2, "name": "Saudi Arabia", "isoCode": "SA", "phoneCode": "+966" },
    { "id": 3, "name": "United Arab Emirates", "isoCode": "AE", "phoneCode": "+971" }
  ]
}
```

> **Note:** 65 countries, Arab countries sorted first.

---

### GET Languages

```http
GET /api/locations/languages?lang=en
```

**Query Parameters:**
| Param | Default | Description |
|-------|---------|-------------|
| `lang` | `en` | `en` or `ar` for localized names |

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "Arabic", "isoCode": "ar" },
    { "id": 2, "name": "English", "isoCode": "en" },
    { "id": 3, "name": "French", "isoCode": "fr" }
  ]
}
```

> **Note:** 50 languages, Arabic and English sorted first.

---

## Tricky Parts

### 1. Step Advancement is NOT Automatic

```typescript
// WRONG: Assumes saving data advances the step
await api.post('/jobseeker/experience', experienceData);
navigate('/wizard/step-3'); // ❌ Will break!

// CORRECT: Explicitly advance after saving
await api.post('/jobseeker/experience', experienceData);
await api.post('/jobseeker/wizard/advance/2'); // ✓ Advance step
navigate('/wizard/step-3');
```

---

### 2. Skills Update is Replace-All

```typescript
// WRONG: Trying to add one skill
await api.put('/jobseeker/skills', { skillIds: [newSkillId] });
// ❌ This DELETES all existing skills!

// CORRECT: Fetch existing, merge, then update
const current = await api.get('/jobseeker/skills');
const currentIds = current.data.skills.map(s => s.id);
const mergedIds = [...new Set([...currentIds, newSkillId])];
await api.put('/jobseeker/skills', { skillIds: mergedIds });
```

---

### 3. Two Error Response Formats

```typescript
// Handle BOTH formats
const handleError = (error: AxiosError) => {
  const data = error.response?.data as any;

  // Check for ASP.NET validation errors first
  if (data?.errors) {
    const messages = Object.values(data.errors).flat();
    return messages.join(', ');
  }

  // Fall back to business logic error
  return data?.message || 'An unexpected error occurred';
};
```

---

### 4. File Upload Content-Type

```typescript
// WRONG: Setting Content-Type manually
await api.post('/jobseeker/picture', formData, {
  headers: { 'Content-Type': 'multipart/form-data' }
});
// ❌ May cause boundary issues!

// CORRECT: Let Axios set it automatically
const formData = new FormData();
formData.append('file', file);
await api.post('/jobseeker/picture', formData);
// ✓ Axios adds correct boundary
```

---

### 5. endDate vs isCurrent

```typescript
// If isCurrent is true, endDate MUST be null
const experienceData = {
  jobTitle: "Senior Developer",
  companyName: "TechCorp",
  startDate: "2020-01-01",
  endDate: isCurrent ? null : selectedEndDate,  // ← Important!
  isCurrent: isCurrent
};
```

---

### 6. Second Language Validation

```typescript
// secondLanguageProficiency is REQUIRED if secondLanguageId is provided
const personalInfo = {
  firstLanguageId: 1,
  firstLanguageProficiency: 4,
  secondLanguageId: hasSecondLanguage ? secondLangId : null,
  secondLanguageProficiency: hasSecondLanguage ? secondLangProf : null
};

// Also: secondLanguageId must DIFFER from firstLanguageId
if (secondLanguageId === firstLanguageId) {
  // ❌ Backend will reject this
}
```

---

## React Integration

### Wizard Context

```typescript
// src/context/WizardContext.tsx
import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import api from '../api/axios';

interface WizardStatus {
  currentStep: number;
  isComplete: boolean;
  stepName: string;
  completedSteps: string[];
}

interface WizardContextValue {
  status: WizardStatus | null;
  loading: boolean;
  error: string | null;
  refreshStatus: () => Promise<void>;
  advanceStep: (step: number) => Promise<void>;
}

const WizardContext = createContext<WizardContextValue | null>(null);

export const WizardProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [status, setStatus] = useState<WizardStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refreshStatus = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const { data } = await api.get('/jobseeker/wizard-status');
      setStatus(data.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to fetch wizard status');
    } finally {
      setLoading(false);
    }
  }, []);

  const advanceStep = useCallback(async (stepNumber: number) => {
    try {
      await api.post(`/jobseeker/wizard/advance/${stepNumber}`);
      await refreshStatus();
    } catch (err: any) {
      const message = err.response?.data?.message || 'Failed to advance step';
      throw new Error(message);
    }
  }, [refreshStatus]);

  useEffect(() => {
    refreshStatus();
  }, [refreshStatus]);

  return (
    <WizardContext.Provider value={{ status, loading, error, refreshStatus, advanceStep }}>
      {children}
    </WizardContext.Provider>
  );
};

export const useWizard = () => {
  const context = useContext(WizardContext);
  if (!context) throw new Error('useWizard must be used within WizardProvider');
  return context;
};
```

---

### Reference Data Context

```typescript
// src/context/ReferenceDataContext.tsx
import React, { createContext, useContext, useState, useEffect } from 'react';
import api from '../api/axios';

interface RefData {
  jobTitles: Array<{ id: number; title: string; category: string }>;
  countries: Array<{ id: number; name: string; isoCode: string; phoneCode: string }>;
  languages: Array<{ id: number; name: string; isoCode: string }>;
  skills: Array<{ id: number; name: string }>;
}

const CACHE_KEY = 'refData';
const CACHE_EXPIRY = 24 * 60 * 60 * 1000; // 24 hours

const ReferenceDataContext = createContext<RefData | null>(null);

export const ReferenceDataProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [data, setData] = useState<RefData | null>(null);

  useEffect(() => {
    const loadData = async () => {
      // Check cache
      const cached = localStorage.getItem(CACHE_KEY);
      if (cached) {
        const { data: cachedData, timestamp } = JSON.parse(cached);
        if (Date.now() - timestamp < CACHE_EXPIRY) {
          setData(cachedData);
          return;
        }
      }

      // Fetch fresh data
      const [jobTitles, countries, languages, skills] = await Promise.all([
        api.get('/jobseeker/job-titles'),
        api.get('/locations/countries'),
        api.get('/locations/languages'),
        api.get('/jobseeker/skills/available'),
      ]);

      const refData: RefData = {
        jobTitles: jobTitles.data.data,
        countries: countries.data.data,
        languages: languages.data.data,
        skills: skills.data.data,
      };

      // Cache it
      localStorage.setItem(CACHE_KEY, JSON.stringify({
        data: refData,
        timestamp: Date.now(),
      }));

      setData(refData);
    };

    loadData();
  }, []);

  return (
    <ReferenceDataContext.Provider value={data}>
      {children}
    </ReferenceDataContext.Provider>
  );
};

export const useReferenceData = () => useContext(ReferenceDataContext);
```

---

### Step Component Example (Step 2)

```tsx
// src/pages/wizard/ProfessionalStep.tsx
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useWizard } from '../../context/WizardContext';
import api from '../../api/axios';

interface Experience {
  id: number;
  jobTitle: string;
  companyName: string;
  startDate: string;
  endDate: string | null;
  isCurrent: boolean;
}

const ProfessionalStep = () => {
  const { advanceStep } = useWizard();
  const navigate = useNavigate();
  const [experiences, setExperiences] = useState<Experience[]>([]);
  const [loading, setLoading] = useState(false);

  // Load existing experiences
  useEffect(() => {
    const loadExperiences = async () => {
      const { data } = await api.get('/jobseeker/experience');
      setExperiences(data.data.experiences);
    };
    loadExperiences();
  }, []);

  // Add new experience
  const handleAddExperience = async (formData: any) => {
    const { data } = await api.post('/jobseeker/experience', formData);
    setExperiences(prev => [...prev, data.data]);
  };

  // Delete experience
  const handleDeleteExperience = async (id: number) => {
    await api.delete(`/jobseeker/experience/${id}`);
    setExperiences(prev => prev.filter(e => e.id !== id));
  };

  // Handle "Next" button
  const handleNext = async () => {
    setLoading(true);
    try {
      await advanceStep(2);
      navigate('/wizard/step-3');
    } catch (error: any) {
      alert(error.message);
    } finally {
      setLoading(false);
    }
  };

  // Handle "Skip" button (same as Next for Steps 2-4)
  const handleSkip = () => handleNext();

  return (
    <div>
      <h2>Professional Background</h2>

      {/* Experience List */}
      {experiences.map(exp => (
        <ExperienceCard
          key={exp.id}
          experience={exp}
          onDelete={() => handleDeleteExperience(exp.id)}
        />
      ))}

      {/* Add Form */}
      <ExperienceForm onSubmit={handleAddExperience} />

      {/* Navigation */}
      <div className="flex gap-4 mt-6">
        <button onClick={handleSkip} disabled={loading}>
          Skip
        </button>
        <button onClick={handleNext} disabled={loading}>
          {loading ? 'Saving...' : 'Next Step'}
        </button>
      </div>
    </div>
  );
};
```

---

### Wizard Router Guard

```tsx
// src/components/WizardGuard.tsx
import { Navigate, useLocation } from 'react-router-dom';
import { useWizard } from '../context/WizardContext';

interface Props {
  children: React.ReactNode;
  requiredStep: number;
}

const WizardGuard: React.FC<Props> = ({ children, requiredStep }) => {
  const { status, loading } = useWizard();
  const location = useLocation();

  if (loading) return <div>Loading...</div>;

  if (!status) return <Navigate to="/login" />;

  // Profile complete, redirect to dashboard
  if (status.isComplete) {
    return <Navigate to="/dashboard" />;
  }

  // User is behind this step, redirect to their current step
  if (status.currentStep < requiredStep - 1) {
    return <Navigate to={`/wizard/step-${status.currentStep + 1}`} />;
  }

  return <>{children}</>;
};

// Usage in routes:
<Route
  path="/wizard/step-2"
  element={
    <WizardGuard requiredStep={2}>
      <ProfessionalStep />
    </WizardGuard>
  }
/>
```

---

## Error Handling Patterns

### Unified Error Handler

```typescript
// src/utils/errorHandler.ts
import { AxiosError } from 'axios';

export interface ApiError {
  message: string;
  fieldErrors?: Record<string, string[]>;
}

export const parseApiError = (error: AxiosError): ApiError => {
  const data = error.response?.data as any;

  // ASP.NET validation errors
  if (data?.errors) {
    return {
      message: data.title || 'Validation failed',
      fieldErrors: data.errors,
    };
  }

  // Business logic errors
  if (data?.message) {
    return { message: data.message };
  }

  // Network or unknown errors
  if (error.message === 'Network Error') {
    return { message: 'Unable to connect to server. Please check your internet connection.' };
  }

  return { message: 'An unexpected error occurred. Please try again.' };
};
```

### Form Error Display

```tsx
// src/components/FormError.tsx
interface Props {
  error: ApiError | null;
  field?: string;
}

const FormError: React.FC<Props> = ({ error, field }) => {
  if (!error) return null;

  // Field-specific errors
  if (field && error.fieldErrors?.[field]) {
    return (
      <span className="text-red-500 text-sm">
        {error.fieldErrors[field][0]}
      </span>
    );
  }

  // General error (no field specified)
  if (!field && !error.fieldErrors) {
    return (
      <div className="bg-red-100 text-red-700 p-3 rounded">
        {error.message}
      </div>
    );
  }

  return null;
};
```

---

## Troubleshooting

### "User not authenticated" (401)

```typescript
// Check 1: Token exists and is being sent
console.log('Token:', localStorage.getItem('token'));

// Check 2: Token is in correct header format
axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;

// Check 3: Token hasn't expired (JWT valid for 24h)
const payload = JSON.parse(atob(token.split('.')[1]));
const expiresAt = new Date(payload.exp * 1000);
console.log('Expires at:', expiresAt);
```

### "Cannot skip steps" (400)

```typescript
// You're trying to advance to Step 3 while still on Step 0
// Solution: Complete each step sequentially
await api.post('/jobseeker/wizard/advance/1'); // First
await api.post('/jobseeker/wizard/advance/2'); // Then
await api.post('/jobseeker/wizard/advance/3'); // Then
```

### Validation Errors on Personal Info

```typescript
// Most common causes:
// 1. jobTitleId doesn't exist in database
// 2. countryId doesn't exist in database
// 3. secondLanguageId === firstLanguageId
// 4. secondLanguageId provided without secondLanguageProficiency

// Always fetch reference data first and validate IDs
const validJobTitleIds = jobTitles.map(j => j.id);
if (!validJobTitleIds.includes(selectedJobTitleId)) {
  // Show error
}
```

### File Upload "Invalid format"

```typescript
// Accepted formats:
// Profile Picture: JPEG, PNG, WebP (max 2MB)
// Resume: PDF only (max 5MB)
// Certificate: PDF, DOCX (max 10MB)

// Validate before upload
const validateFile = (file: File, type: 'picture' | 'resume' | 'certificate') => {
  const limits = {
    picture: { types: ['image/jpeg', 'image/png', 'image/webp'], size: 2 * 1024 * 1024 },
    resume: { types: ['application/pdf'], size: 5 * 1024 * 1024 },
    certificate: { types: ['application/pdf', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'], size: 10 * 1024 * 1024 },
  };

  const config = limits[type];
  if (!config.types.includes(file.type)) return 'Invalid file type';
  if (file.size > config.size) return 'File too large';
  return null;
};
```

---

## API Summary Table

| Step | Action | Method | Endpoint |
|------|--------|--------|----------|
| **All** | Get wizard status | GET | `/jobseeker/wizard-status` |
| **All** | Advance step | POST | `/jobseeker/wizard/advance/{step}` |
| **1** | Save personal info | POST | `/jobseeker/personal-info` |
| **1** | Get personal info | GET | `/jobseeker/personal-info` |
| **1** | Upload picture | POST | `/jobseeker/picture` |
| **1** | Get picture | GET | `/jobseeker/picture` |
| **1** | Delete picture | DELETE | `/jobseeker/picture` |
| **1** | Upload resume | POST | `/jobseeker/resume/upload` |
| **1** | Get resume | GET | `/jobseeker/resume` |
| **1** | Download resume | GET | `/jobseeker/resume/download` |
| **1** | Delete resume | DELETE | `/jobseeker/resume` |
| **2** | List experiences | GET | `/jobseeker/experience` |
| **2** | Add experience | POST | `/jobseeker/experience` |
| **2** | Update experience | PUT | `/jobseeker/experience/{id}` |
| **2** | Delete experience | DELETE | `/jobseeker/experience/{id}` |
| **2** | Reorder experiences | POST | `/jobseeker/experience/reorder` |
| **2** | List education | GET | `/jobseeker/education` |
| **2** | Add education | POST | `/jobseeker/education` |
| **2** | Update education | PUT | `/jobseeker/education/{id}` |
| **2** | Delete education | DELETE | `/jobseeker/education/{id}` |
| **2** | Reorder education | POST | `/jobseeker/education/reorder` |
| **3** | List projects | GET | `/jobseeker/projects` |
| **3** | Add project | POST | `/jobseeker/projects` |
| **3** | Update project | PUT | `/jobseeker/projects/{id}` |
| **3** | Delete project | DELETE | `/jobseeker/projects/{id}` |
| **4** | Get available skills | GET | `/jobseeker/skills/available` |
| **4** | Get user skills | GET | `/jobseeker/skills` |
| **4** | Update skills | PUT | `/jobseeker/skills` |
| **4** | Clear skills | DELETE | `/jobseeker/skills` |
| **4** | Get social links | GET | `/jobseeker/social-accounts` |
| **4** | Update social links | PUT | `/jobseeker/social-accounts` |
| **4** | Delete social links | DELETE | `/jobseeker/social-accounts` |
| **4** | List certificates | GET | `/jobseeker/certificates` |
| **4** | Add certificate | POST | `/jobseeker/certificates` |
| **4** | Get certificate | GET | `/jobseeker/certificates/{id}` |
| **4** | Update certificate | PUT | `/jobseeker/certificates/{id}` |
| **4** | Delete certificate | DELETE | `/jobseeker/certificates/{id}` |
| **4** | Download cert file | GET | `/jobseeker/certificates/{id}/download` |
| **Ref** | Get job titles | GET | `/jobseeker/job-titles` |
| **Ref** | Get countries | GET | `/locations/countries` |
| **Ref** | Get languages | GET | `/locations/languages` |
