# JobSeeker Profile Wizard — Frontend Integration Guide

**Last Updated:** March 2026  
**Base URL:** `http://localhost:5217/api/jobseeker`

---

## Table of Contents

1. [Overview](#overview)
2. [Wizard Flow & State](#wizard-flow--state)
3. [Step Tracking Logic](#step-tracking-logic)
4. [Endpoints Reference](#endpoints-reference)
   - [Wizard Status](#1-get-wizard-status)
   - [Advance Step](#2-advance-wizard-step)
   - [Step 1: Personal Info & Files](#step-1-personal-info--files)
   - [Step 2: Professional Background](#step-2-professional-background)
   - [Step 3: Projects](#step-3-projects)
   - [Step 4: Skills & Extras](#step-4-skills--extras)
5. [React Integration](#react-integration)
   - [Wizard Context](#wizard-context)
   - [Step Advancement Pattern](#step-advancement-pattern)
   - [Handling "Skip"](#handling-skip)

---

## Overview

The Profile Completion Wizard guides newly registered Job Seekers through 4 mandatory steps to complete their profile.

| Step | Title | Key Actions |
|------|-------|-------------|
| **1** | **Personal Info** | Basic info, Profile Picture, Resume upload |
| **2** | **Professional** | Work Experience, Education |
| **3** | **Portfolio** | Projects |
| **4** | **Skills & Extras** | Skills, Certificates, Social Links |

---

## Wizard Flow & State

### The "Paging vs Saving" Model

A key concept in this API is that **saving data does NOT automatically advance the wizard step**.

- **Saving Data**: Use entity-specific endpoints (e.g., `POST /experience`) to save data as the user types or clicks "Add".
- **Moving Next**: You must **explicitly call** the `POST /wizard/advance/{step}` endpoint when the user clicks "Next" or "Skip".

This decoupling allows users to:
1. Save multiple items (e.g., 3 jobs) without moving to the next step.
2. Skip optional steps (e.g., Projects) without saving any data.
3. Advance the wizard even if they haven't added any optional data.

---

## Step Tracking Logic

The backend maintains a `ProfileCompletionStep` integer (0-4) for each user.

- **0**: New user, hasn't started.
- **1**: Completed Personal Info.
- **2**: Completed Professional Background.
- **3**: Completed Portfolio.
- **4**: **Fully Completed Profile**.

The wizard determines which screen to show based on this number. Calls to `POST /wizard/advance/X` will update this number to `X` (if X is greater than the current step).

---

## Endpoints Reference

### 1. Get Wizard Status

Call this on app load to determine where to redirect the user.

```http
GET /api/jobseeker/wizard-status
```

**Response:**

```json
{
  "success": true,
  "data": {
    "currentStep": 2,
    "completedSteps": [1, 2],
    "isComplete": false,
    "nextStepName": "Projects"
  }
}
```

### 2. Advance Wizard Step

Call this when the user clicks **"Next"** or **"Skip"**.

```http
POST /api/jobseeker/wizard/advance/{stepNumber}
```

**Parameters:**
- `stepNumber` (int): The step number needed to be marked as completed (1, 2, 3, or 4).

**Example:** To finish Step 2 and move to Step 3:
```
POST /api/jobseeker/wizard/advance/2
```

**Response:**
```json
{
  "success": true,
  "message": "Wizard step advanced successfully.",
  "data": {
    "profileCompletionStep": 2,
    "isProfileComplete": false
    // ...other profile fields
  }
}
```

---

### Step 1: Personal Info & Files

**Goal:** Complete basic profile details.

#### Save Personal Info
```http
POST /api/jobseeker/personal-info
```
**Body:**
```json
{
  "jobTitleId": 5,
  "countryId": 1,
  "city": "Cairo",
  "phone": "+201000000000",
  "birthDate": "1995-05-20",
  "gender": 1, // 0=Male, 1=Female
  "about": "Software Engineer with 5 years experience..."
  // languageIds, etc.
}
```

#### Upload Profile Picture
```http
POST /api/jobseeker/picture
Content-Type: multipart/form-data
```
**Form Data:** `file` (Image)

#### Upload Resume (PDF)
```http
POST /api/jobseeker/resume/upload
Content-Type: multipart/form-data
```
**Form Data:** `file` (PDF, max 5MB)

> **Action:** When user clicks "Next", call `POST /wizard/advance/1`.

---

### Step 2: Professional Background

**Goal:** Add multiple education and experience entries.

#### Add Experience
```http
POST /api/jobseeker/experience
```
**Body:**
```json
{
  "companyName": "Tech Corp",
  "jobTitle": "Senior Dev",
  "startDate": "2020-01-01",
  "endDate": null, // null = Currently working
  "isCurrent": true,
  "description": "Built cool things."
  // ...
}
```

#### Add Education
```http
POST /api/jobseeker/education
```
**Body:**
```json
{
  "institution": "University of Cairo",
  "degree": 1, // Enum value
  "fieldOfStudy": "Computer Science",
  "startDate": "2015-09-01",
  "endDate": "2019-06-30"
}
```

> **Action:** User can add multiple. When clicked "Next" (or "Skip"), call `POST /wizard/advance/2`.

---

### Step 3: Projects

**Goal:** Add portfolio projects (Optional).

#### Add Project
```http
POST /api/jobseeker/projects
```
**Body:**
```json
{
  "name": "E-commerce App",
  "description": "Built with React and .NET",
  "link": "https://github.com/user/repo",
  "role": "Lead Developer",
  "skillsUsed": ["C#", "React", "SQL"]
}
```

> **Action:** When user clicks "Next" (or "Skip"), call `POST /wizard/advance/3`.

---

### Step 4: Skills & Extras

**Goal:** Finalize profile details.

#### Get Available Skills
```http
GET /api/jobseeker/skills/available
```
*Returns list of all selectable skills.*

#### Update Skills
```http
PUT /api/jobseeker/skills
```
**Body:**
```json
{
  "skillIds": [1, 4, 15, 22] // List of Selected Skill IDs
}
```

#### Update Social Links
```http
PUT /api/jobseeker/social-accounts
```
**Body:**
```json
{
  "linkedIn": "https://linkedin.com/in/user",
  "gitHub": "https://github.com/user",
  "portfolio": "https://user.dev"
}
```

#### Add Certificate
```http
POST /api/jobseeker/certificates
```
**Body:**
```json
{
  "name": "Azure Fundamentals",
  "issuingOrganization": "Microsoft",
  "issueDate": "2023-01-01",
  "credentialId": "AZ-900",
  "credentialUrl": "https://..."
}
```

> **Action:** When user clicks "Finish", call `POST /wizard/advance/4`.

---

## React Integration

### Wizard Context

Maintain the current step globally to handle redirects.

```typescript
// src/context/WizardContext.tsx
import React, { createContext, useContext, useState, useEffect } from "react";
import api from "../api/axios";

interface WizardContextType {
  currentStep: number;
  refreshWizardStatus: () => Promise<void>;
  advanceStep: (stepNumber: number) => Promise<void>;
}

const WizardContext = createContext<WizardContextType>(null!);

export const WizardProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [currentStep, setCurrentStep] = useState(0);

  const refreshWizardStatus = async () => {
    try {
      const { data } = await api.get("/jobseeker/wizard-status");
      setCurrentStep(data.data.currentStep);
    } catch (err) {
      console.error("Failed to fetch wizard status", err);
    }
  };

  const advanceStep = async (stepNumber: number) => {
    try {
      await api.post(`/jobseeker/wizard/advance/${stepNumber}`);
      await refreshWizardStatus(); // Sync state
    } catch (err) {
       console.error("Failed to advance step", err);
       throw err; 
    }
  };

  useEffect(() => {
    refreshWizardStatus();
  }, []);

  return (
    <WizardContext.Provider value={{ currentStep, refreshWizardStatus, advanceStep }}>
      {children}
    </WizardContext.Provider>
  );
};

export const useWizard = () => useContext(WizardContext);
```

### Step Component Example (Step 2)

```tsx
import { useState } from "react";
import { useWizard } from "../context/WizardContext";
import api from "../api/axios";

const ProfessionalBackgroundStep = () => {
  const { advanceStep } = useWizard();
  const [loading, setLoading] = useState(false);

  // Function to handle "Add Experience" button
  const handleAddExperience = async (data: any) => {
    // 1. Save data ONLY
    await api.post("/jobseeker/experience", data);
    // 2. Refresh list locally (don't advance step)
  };

  // Function to handle "Next" button
  const handleNext = async () => {
    setLoading(true);
    try {
      // Advance to step 3 explicitly
      // Note: We pass '2' because we are completing Step 2
      await advanceStep(2); 
    } catch (error) {
      alert("Failed to move next");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h2>Professional Background</h2>
      {/* Forms to add experience/education */}
      <ExperienceForm onSave={handleAddExperience} />
      
      <div className="actions">
        <button onClick={handleNext} disabled={loading}>
          {loading ? "Saving..." : "Next Step"}
        </button>
      </div>
    </div>
  );
};
```

### Handling "Skip"

Since the advance endpoint doesn't validate data presence for optional steps, "Skip" and "Next" perform the exact same API call.

```tsx
const handleSkip = () => {
  // Just advance without saving any form data
  advanceStep(3); 
};
```
