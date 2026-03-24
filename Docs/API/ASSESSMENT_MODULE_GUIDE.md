# Assessment Module - Comprehensive Technical Guide

**Version:** 1.0
**Last Updated:** March 2026
**Author:** JobIntel Backend Team

---

## Table of Contents

1. [Overview](#1-overview)
2. [Purpose and Business Value](#2-purpose-and-business-value)
3. [Architecture](#3-architecture)
4. [Database Schema](#4-database-schema)
5. [Configuration](#5-configuration)
6. [Eligibility System](#6-eligibility-system)
7. [Question Selection Algorithm](#7-question-selection-algorithm)
8. [Assessment Flow](#8-assessment-flow)
9. [Scoring System](#9-scoring-system)
10. [API Reference](#10-api-reference)
11. [Exam Mode Design](#11-exam-mode-design)
12. [State Machine](#12-state-machine)
13. [Error Handling](#13-error-handling)
14. [Frontend Integration Guide](#14-frontend-integration-guide)
15. [Testing Scenarios](#15-testing-scenarios)

---

## 1. Overview

The Assessment Module is a skill verification system that allows **Job Seekers** to take standardized quizzes to validate their technical and soft skills. The assessment produces a normalized score (0-100) that becomes part of the candidate's profile, helping recruiters evaluate candidates more effectively.

### Key Features

- **30-question assessments** (21 technical + 9 soft skills)
- **45-minute time limit** per assessment
- **Role-targeted questions** based on job title
- **Seniority-adjusted difficulty** based on years of experience
- **60-day cooldown** between attempts
- **18-month score validity**
- **Exam mode** - no feedback during test, full results after completion

---

## 2. Purpose and Business Value

### For Job Seekers
- **Skill Verification**: Prove competency beyond resume claims
- **Profile Enhancement**: Stand out with verified assessment scores
- **Self-Assessment**: Identify strength and improvement areas
- **Competitive Edge**: Higher scores increase visibility to recruiters

### For Recruiters
- **Objective Screening**: Filter candidates by verified skill scores
- **Quality Assurance**: Reduce interview time with pre-validated skills
- **Role Matching**: Scores aligned to specific job families (Backend, Frontend, etc.)
- **Risk Reduction**: Quantified skill levels reduce hiring mistakes

### Platform Value
- **Trust Building**: Assessments add credibility to the platform
- **Engagement**: Encourages active participation and profile completion
- **Data Quality**: Structured skill data improves matching algorithms

---

## 3. Architecture

### Component Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                        AssessmentController                         │
│         (API Layer - Route handling, Auth, Response formatting)     │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        IAssessmentService                           │
│           (Business Logic - Eligibility, Scoring, Flow)             │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                          AppDbContext                               │
│              (Data Access - EF Core, SQL Server)                    │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         Database Tables                             │
│   AssessmentQuestion │ AssessmentAttempt │ AssessmentAnswer         │
└─────────────────────────────────────────────────────────────────────┘
```

### File Structure

```
RecruitmentPlatformAPI/
├── Controllers/
│   └── AssessmentController.cs          # 9 API endpoints
├── Services/
│   └── Assessment/
│       ├── IAssessmentService.cs        # Service interface
│       └── AssessmentService.cs         # Business logic (~850 lines)
├── DTOs/
│   └── Assessment/
│       └── AssessmentDtos.cs            # Request/Response DTOs
├── Models/
│   └── Assessment/
│       ├── AssessmentQuestion.cs        # Question entity
│       ├── AssessmentAttempt.cs         # Attempt tracking entity
│       └── AssessmentAnswer.cs          # Answer recording entity
├── Enums/
│   ├── AssessmentStatus.cs              # InProgress, Completed, etc.
│   ├── QuestionCategory.cs              # Technical, SoftSkill
│   ├── QuestionDifficulty.cs            # Easy, Medium, Hard
│   └── ExperienceSeniorityLevel.cs      # Junior, Mid, Senior
├── Configuration/
│   └── AssessmentSettings.cs            # Constants and thresholds
└── Data/
    └── Seed/
        └── AssessmentQuestionSeed.cs    # 73 seed questions
```

---

## 4. Database Schema

### Entity Relationship Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                      AssessmentQuestion                          │
├──────────────────────────────────────────────────────────────────┤
│ Id (PK)              │ int                                       │
│ QuestionText         │ nvarchar(500)                             │
│ Category             │ int (Technical=1, SoftSkill=2)            │
│ RoleFamily           │ int (Backend=2, Frontend=1, etc.)         │
│ SkillId (FK)         │ int? (nullable for soft skills)           │
│ Difficulty           │ int (Easy=1, Medium=2, Hard=3)            │
│ SeniorityLevel       │ int (Junior=1, Mid=2, Senior=3)           │
│ Options              │ nvarchar(1000) (JSON array)               │
│ CorrectAnswerIndex   │ int (0-3)                                 │
│ TimePerQuestion      │ int? (seconds, default 60)                │
│ IsActive             │ bit                                       │
│ Explanation          │ nvarchar(1000)                            │
│ CreatedAt            │ datetime2                                 │
│ UpdatedAt            │ datetime2                                 │
└──────────────────────────────────────────────────────────────────┘
          │
          │ (questions selected per attempt)
          ▼
┌──────────────────────────────────────────────────────────────────┐
│                      AssessmentAttempt                           │
├──────────────────────────────────────────────────────────────────┤
│ Id (PK)              │ int                                       │
│ JobSeekerId (FK)     │ int                                       │
│ JobTitleId (FK)      │ int (snapshot at assessment time)         │
│ OverallScore         │ decimal(5,2)?                             │
│ TechnicalScore       │ decimal(5,2)?                             │
│ SoftSkillsScore      │ decimal(5,2)?                             │
│ Status               │ int (InProgress=1, Completed=2, etc.)     │
│ StartedAt            │ datetime2                                 │
│ CompletedAt          │ datetime2?                                │
│ TimeLimitMinutes     │ int (default 45)                          │
│ TotalQuestions       │ int                                       │
│ QuestionsAnswered    │ int                                       │
│ ExpiresAt            │ datetime2                                 │
│ ScoreExpiresAt       │ datetime2?                                │
│ IsActive             │ bit (only one active per job seeker)      │
│ RetakeNumber         │ int (1st, 2nd, 3rd attempt)               │
│ QuestionIdsJson      │ nvarchar(500) (ordered question list)     │
└──────────────────────────────────────────────────────────────────┘
          │
          │ 1:N
          ▼
┌──────────────────────────────────────────────────────────────────┐
│                      AssessmentAnswer                            │
├──────────────────────────────────────────────────────────────────┤
│ Id (PK)              │ int                                       │
│ AssessmentAttemptId  │ int (FK)                                  │
│ QuestionId           │ int (FK)                                  │
│ SelectedAnswerIndex  │ int (0-3)                                 │
│ IsCorrect            │ bit                                       │
│ TimeSpentSeconds     │ int                                       │
│ AnsweredAt           │ datetime2                                 │
└──────────────────────────────────────────────────────────────────┘
```

### Key Indexes

```sql
-- Efficient question filtering
IX_AssessmentQuestion_Filtering
  ON AssessmentQuestion(RoleFamily, Category, Difficulty, SeniorityLevel, IsActive)

-- Find active attempts for a job seeker
IX_AssessmentAttempt_JobSeeker_Active
  ON AssessmentAttempt(JobSeekerId, IsActive)

-- Query attempts by status
IX_AssessmentAttempt_JobSeeker_Status
  ON AssessmentAttempt(JobSeekerId, Status, StartedAt)

-- Enforce single in-progress assessment
IX_AssessmentAttempt_SingleInProgress (UNIQUE, FILTERED)
  ON AssessmentAttempt(JobSeekerId) WHERE Status = 1

-- Prevent duplicate answers
IX_AssessmentAnswer_Attempt_Question (UNIQUE)
  ON AssessmentAnswer(AssessmentAttemptId, QuestionId)
```

---

## 5. Configuration

All assessment parameters are centralized in `AssessmentSettings.cs`:

```csharp
public static class AssessmentSettings
{
    // Timing
    public const int CooldownDays = 60;              // Days between attempts
    public const int ScoreValidityMonths = 18;       // How long scores are valid
    public const int DefaultTimeLimitMinutes = 45;   // Total assessment time
    public const int DefaultTimePerQuestionSeconds = 60;

    // Question Distribution
    public const int TotalQuestionsPerAssessment = 30;
    public const int TechnicalQuestionsCount = 21;   // 70% of questions
    public const int SoftSkillQuestionsCount = 9;    // 30% of questions

    // Scoring Weights
    public const decimal TechnicalWeight = 0.70m;    // 70% of final score
    public const decimal SoftSkillWeight = 0.30m;    // 30% of final score

    // Thresholds
    public const decimal MinimumPassingScore = 50.0m;
}
```

### Why These Values?

| Setting | Value | Rationale |
|---------|-------|-----------|
| **60-day cooldown** | Prevents gaming the system; allows genuine skill improvement |
| **18-month validity** | Skills evolve; ensures scores reflect current abilities |
| **45-minute limit** | Long enough for thoughtful answers; short enough to maintain focus |
| **70/30 technical/soft split** | Technical skills are primary; soft skills differentiate candidates |
| **50% passing score** | Floor for "verified" status; still shows competency |

---

## 6. Eligibility System

Before starting an assessment, the system verifies multiple conditions:

### Eligibility Checks (in order)

```
1. ROLE CHECK
   └── User must be a JobSeeker (not Recruiter)

2. PROFILE EXISTENCE
   └── JobSeeker record must exist in database

3. PROFILE COMPLETION
   └── ProfileCompletionStep must be >= 4 (completed wizard)

4. JOB TITLE SET
   └── JobTitleId must be populated (for question targeting)

5. NO IN-PROGRESS ASSESSMENT
   └── Cannot have Status = InProgress attempt

6. COOLDOWN PERIOD
   └── LastAssessmentDate + 60 days must be in the past
```

### Eligibility Response DTO

```json
{
  "isEligible": false,
  "reason": "Please wait 45 days before taking another assessment",
  "hasCompletedProfile": true,
  "hasJobTitle": true,
  "hasInProgressAssessment": false,
  "isInCooldownPeriod": true,
  "cooldownEndsAt": "2026-05-15T10:30:00Z",
  "daysUntilEligible": 45,
  "previousAttempts": 2,
  "currentScore": 78.50,
  "scoreExpiresAt": "2027-09-15T10:30:00Z"
}
```

### Decision Flow

```
                    ┌─────────────────┐
                    │ Check Eligibility│
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │ Is JobSeeker?   │───No──→ "Only job seekers can take assessments"
                    └────────┬────────┘
                             │Yes
                    ┌────────▼────────┐
                    │ Profile Step≥4? │───No──→ "Please complete your profile"
                    └────────┬────────┘
                             │Yes
                    ┌────────▼────────┐
                    │ Has Job Title?  │───No──→ "Please set your job title"
                    └────────┬────────┘
                             │Yes
                    ┌────────▼────────┐
                    │ In-Progress?    │───Yes─→ "Complete or abandon current assessment"
                    └────────┬────────┘
                             │No
                    ┌────────▼────────┐
                    │ In Cooldown?    │───Yes─→ "Wait N days before retaking"
                    └────────┬────────┘
                             │No
                    ┌────────▼────────┐
                    │   ELIGIBLE ✓    │
                    └─────────────────┘
```

---

## 7. Question Selection Algorithm

The algorithm selects questions tailored to the job seeker's **role family** and **seniority level**.

### Step 1: Determine Seniority from Experience

```csharp
SeniorityLevel = YearsOfExperience switch
{
    null or <= 2  => Junior,   // 0-2 years
    >= 3 and <= 5 => Mid,      // 3-5 years
    _             => Senior    // 6+ years
};
```

### Step 2: Define Difficulty Distribution

The distribution varies by seniority to match expected competency levels:

| Seniority | Easy | Medium | Hard | Total |
|-----------|------|--------|------|-------|
| **Junior** | 10 | 8 | 3 | 21 technical |
| | 4 | 4 | 1 | 9 soft skill |
| **Mid** | 5 | 11 | 5 | 21 technical |
| | 2 | 5 | 2 | 9 soft skill |
| **Senior** | 3 | 8 | 10 | 21 technical |
| | 1 | 4 | 4 | 9 soft skill |

### Step 3: Select Technical Questions

```
For each difficulty (Easy, Medium, Hard):
    1. Query questions WHERE:
       - Category = Technical
       - RoleFamily = JobSeeker's RoleFamily (e.g., Backend)
       - SeniorityLevel = Calculated Seniority
       - Difficulty = Current Difficulty Level
       - IsActive = true
    2. Shuffle results
    3. Take required count
```

### Step 4: Fallback for Insufficient Questions

If not enough questions match the exact criteria:

```
Query questions WHERE:
    - Category = Technical
    - RoleFamily = JobSeeker's RoleFamily
    - IsActive = true
    - NOT already selected

Take remaining needed to reach 21 technical questions
```

### Step 5: Select Soft Skill Questions

Soft skills are **not role-specific** (apply to all roles):

```
For each difficulty level:
    Select from Category = SoftSkill
    Matching SeniorityLevel
    (RoleFamily not filtered for soft skills)
```

### Step 6: Final Shuffle and Store

```csharp
// Combine all selected questions
var allQuestionIds = technicalIds.Concat(softSkillIds);

// Shuffle to randomize order
var shuffled = allQuestionIds.OrderBy(_ => Guid.NewGuid()).ToList();

// Store in attempt for consistent ordering
attempt.QuestionIdsJson = JsonSerializer.Serialize(shuffled);
```

### Why Store Question IDs?

The `QuestionIdsJson` field ensures:
1. **Consistent ordering** - Same question sequence on page refresh
2. **Validation** - Verify submitted answers belong to this attempt
3. **Reproducibility** - Reconstruct exact assessment for auditing

---

## 8. Assessment Flow

### Complete User Journey

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         ASSESSMENT LIFECYCLE                            │
└─────────────────────────────────────────────────────────────────────────┘

[1] PRE-ASSESSMENT
    │
    ├── GET /eligibility    → Check if user can start
    │   └── Returns: { isEligible: true/false, reason, currentScore, etc. }
    │
    └── POST /start         → Begin assessment
        └── Creates AssessmentAttempt with Status = InProgress
        └── Selects and stores questions
        └── Returns: { attemptId, totalQuestions, timeLimitMinutes, expiresAt }

[2] DURING ASSESSMENT (45-minute window)
    │
    ├── GET /current        → Check progress (optional, for reconnect)
    │   └── Returns: { status, questionsAnswered, timeRemaining, progress% }
    │
    ├── GET /question       → Get next unanswered question
    │   └── Returns: { questionId, questionText, options[], category, difficulty }
    │   └── Returns null when all answered
    │
    └── POST /answer        → Submit answer (repeat 30 times)
        └── Request: { questionId, selectedAnswerIndex, timeSpentSeconds }
        └── Response: { success, questionsAnswered, questionsRemaining, progress% }
        └── NO correctness feedback (exam mode)

[3] COMPLETION
    │
    ├── POST /complete      → Finish and calculate scores
    │   └── Calculates TechnicalScore, SoftSkillsScore, OverallScore
    │   └── Sets Status = Completed
    │   └── Updates JobSeeker.CurrentAssessmentScore
    │   └── Returns full results with per-question breakdown
    │
    └── POST /abandon       → Give up (alternative path)
        └── Sets Status = Abandoned
        └── Still triggers 60-day cooldown

[4] POST-ASSESSMENT
    │
    ├── GET /history        → View all past attempts
    │   └── Returns: { attempts[], totalAttempts, bestScore, currentActiveScore }
    │
    └── GET /result/{id}    → View detailed result for specific attempt
        └── Returns: { scores, questionResults[] with correct answers }
```

### Time Expiration Handling

The assessment expires automatically after 45 minutes:

```csharp
// Calculated at start
attempt.ExpiresAt = DateTime.UtcNow.AddMinutes(45);

// Checked on every request
if (DateTime.UtcNow > attempt.ExpiresAt)
{
    attempt.Status = AssessmentStatus.Expired;
    await _context.SaveChangesAsync();
    return null; // or appropriate error
}
```

**Important**: There is no background job for expiration. Expiration is checked **lazily** when the user makes a request. This simplifies infrastructure and handles the common case where users simply close the browser.

---

## 9. Scoring System

### Score Calculation Formula

```
TechnicalScore = (TechnicalCorrect / TechnicalTotal) × 100
SoftSkillScore = (SoftSkillCorrect / SoftSkillTotal) × 100

OverallScore = (TechnicalScore × 0.70) + (SoftSkillScore × 0.30)
```

### Example Calculation

```
Scenario: User answers 15/21 technical, 7/9 soft skills

TechnicalScore = (15 / 21) × 100 = 71.43%
SoftSkillScore = (7 / 9) × 100 = 77.78%

OverallScore = (71.43 × 0.70) + (77.78 × 0.30)
             = 50.00 + 23.33
             = 73.33%
```

### Performance Levels

```csharp
PerformanceLevel = OverallScore switch
{
    >= 90 => "Excellent",      // Top tier
    >= 75 => "Good",           // Above average
    >= 50 => "Average",        // Passing
    _     => "Needs Improvement" // Below passing
};
```

### Score Persistence

When an assessment completes:

1. **AssessmentAttempt** is updated with scores
2. **Previous active attempt** is marked `IsActive = false`
3. **Current attempt** is marked `IsActive = true`
4. **JobSeeker** denormalized fields are updated:
   - `CurrentAssessmentScore = OverallScore`
   - `LastAssessmentDate = Now`
   - `AssessmentJobTitleId = Attempt.JobTitleId`

---

## 10. API Reference

### Endpoint Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/assessment/eligibility` | Check if user can start |
| `POST` | `/api/assessment/start` | Start new assessment |
| `GET` | `/api/assessment/current` | Get in-progress status |
| `GET` | `/api/assessment/question` | Get next question |
| `POST` | `/api/assessment/answer` | Submit answer |
| `POST` | `/api/assessment/complete` | Finish and get score |
| `POST` | `/api/assessment/abandon` | Abandon attempt |
| `GET` | `/api/assessment/history` | Get all attempts |
| `GET` | `/api/assessment/result/{id}` | Get detailed result |

### Authentication

All endpoints require JWT Bearer token with JobSeeker role:

```http
Authorization: Bearer <jwt_token>
```

### Detailed Endpoint Documentation

#### GET /eligibility

Check if the authenticated user can start an assessment.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "isEligible": true,
    "reason": null,
    "hasCompletedProfile": true,
    "hasJobTitle": true,
    "hasInProgressAssessment": false,
    "isInCooldownPeriod": false,
    "cooldownEndsAt": null,
    "daysUntilEligible": null,
    "previousAttempts": 1,
    "currentScore": 72.50,
    "scoreExpiresAt": "2027-09-15T10:30:00Z"
  }
}
```

#### POST /start

Start a new assessment. Fails if not eligible.

**Response 200:**
```json
{
  "success": true,
  "message": "Assessment started successfully",
  "data": {
    "attemptId": 42,
    "totalQuestions": 30,
    "technicalQuestions": 21,
    "softSkillQuestions": 9,
    "timeLimitMinutes": 45,
    "startedAt": "2026-03-22T14:00:00Z",
    "expiresAt": "2026-03-22T14:45:00Z",
    "jobTitle": "Senior Backend Developer",
    "roleFamily": "Backend",
    "seniorityLevel": "Senior",
    "retakeNumber": 2
  }
}
```

**Response 400:**
```json
{
  "success": false,
  "message": "Cannot start assessment. Please check your eligibility first."
}
```

#### GET /question

Get the next unanswered question in sequence.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "questionId": 15,
    "questionNumber": 5,
    "totalQuestions": 30,
    "questionText": "What is the time complexity of binary search?",
    "category": "Technical",
    "difficulty": "Medium",
    "options": [
      "O(n)",
      "O(log n)",
      "O(n²)",
      "O(1)"
    ],
    "timeAllowedSeconds": 60,
    "timeRemainingInAssessmentSeconds": 2340
  }
}
```

**Response 404:** (all questions answered or no active assessment)
```json
{
  "success": false,
  "message": "No more questions or assessment not found"
}
```

#### POST /answer

Submit an answer for a question.

**Request:**
```json
{
  "questionId": 15,
  "selectedAnswerIndex": 1,
  "timeSpentSeconds": 45
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "success": true,
    "questionsAnswered": 5,
    "questionsRemaining": 25,
    "isAssessmentComplete": false,
    "timeRemainingSeconds": 2295,
    "progressPercentage": 16.7
  }
}
```

**Note:** No `isCorrect` field - this is exam mode!

#### POST /complete

Complete the assessment and get results.

**Response 200:**
```json
{
  "success": true,
  "message": "Assessment completed successfully",
  "data": {
    "attemptId": 42,
    "status": "Completed",
    "overallScore": 73.33,
    "technicalScore": 71.43,
    "softSkillsScore": 77.78,
    "totalQuestions": 30,
    "correctAnswers": 22,
    "technicalCorrect": 15,
    "technicalTotal": 21,
    "softSkillCorrect": 7,
    "softSkillTotal": 9,
    "startedAt": "2026-03-22T14:00:00Z",
    "completedAt": "2026-03-22T14:32:15Z",
    "timeTakenMinutes": 32,
    "scoreExpiresAt": "2027-09-22T14:32:15Z",
    "jobTitle": "Senior Backend Developer",
    "performanceLevel": "Good",
    "isPassing": true,
    "questionResults": [
      {
        "questionId": 15,
        "questionText": "What is the time complexity of binary search?",
        "category": "Technical",
        "difficulty": "Medium",
        "options": ["O(n)", "O(log n)", "O(n²)", "O(1)"],
        "selectedAnswerIndex": 1,
        "correctAnswerIndex": 1,
        "isCorrect": true,
        "explanation": "Binary search divides the search space in half...",
        "timeSpentSeconds": 45
      }
      // ... more questions
    ]
  }
}
```

#### GET /result/{attemptId}

Get detailed results for a past attempt. Only shows completed/abandoned/expired attempts.

**Response 200:** Same format as POST /complete

**Response 404:** Attempt not found or still in progress

---

## 11. Exam Mode Design

### Philosophy

The assessment uses **"exam mode"** - no immediate feedback during the test. This design choice was deliberate:

### Benefits of Exam Mode

1. **Prevents Gaming**: Users can't use feedback to guess answers
2. **Realistic Testing**: Mirrors real exam conditions
3. **Consistent Experience**: All users face same uncertainty
4. **Score Validity**: Results reflect actual knowledge, not iterative guessing

### What Users See During Test

```
After submitting answer:
┌────────────────────────────────────────┐
│ ✓ Answer recorded                      │
│ Progress: 5/30 (16.7%)                 │
│ Time remaining: 38:15                  │
│                                        │
│       [Continue to Next Question]      │
└────────────────────────────────────────┘
```

### What Users See After Completion

```
Assessment Complete!

Overall Score: 73.33% (Good)
Technical: 71.43% (15/21 correct)
Soft Skills: 77.78% (7/9 correct)

┌─ Question 1 ─────────────────────────┐
│ What is the time complexity of...    │
│                                      │
│ ○ O(n)                               │
│ ● O(log n)  ← Your answer ✓ Correct  │
│ ○ O(n²)                              │
│ ○ O(1)                               │
│                                      │
│ Explanation: Binary search divides...│
└──────────────────────────────────────┘
```

---

## 12. State Machine

### Assessment Status Transitions

```
                    ┌───────────────┐
                    │   (initial)   │
                    └───────┬───────┘
                            │ POST /start
                            ▼
                    ┌───────────────┐
          ┌────────│  InProgress   │────────┐
          │        └───────┬───────┘        │
          │                │                │
    POST /abandon    POST /complete    Time expires
          │                │                │
          ▼                ▼                ▼
    ┌───────────┐    ┌───────────┐    ┌───────────┐
    │ Abandoned │    │ Completed │    │  Expired  │
    └───────────┘    └───────────┘    └───────────┘
         │                │                │
         └────────────────┴────────────────┘
                          │
                   60-day cooldown
                          │
                          ▼
                   Can start new
                   assessment
```

### Status Definitions

| Status | Value | Description |
|--------|-------|-------------|
| `InProgress` | 1 | Currently taking the assessment |
| `Completed` | 2 | All questions answered, scores calculated |
| `Abandoned` | 3 | User explicitly gave up |
| `Expired` | 4 | Time ran out before completion |

### Constraints

- Only **one InProgress** assessment per job seeker (enforced by unique filtered index)
- Only **one IsActive** completed assessment per job seeker (managed by service logic)
- **All terminal states** (Completed, Abandoned, Expired) trigger the 60-day cooldown

---

## 13. Error Handling

### Service Layer Patterns

```csharp
public async Task<SomeDto?> SomeOperationAsync(int userId)
{
    try
    {
        // 1. Validate user and role
        var user = await _context.Users.FindAsync(userId);
        if (user?.AccountType != AccountType.JobSeeker)
            return null;

        // 2. Get domain entity
        var jobSeeker = await _context.JobSeekers
            .FirstOrDefaultAsync(js => js.UserId == userId);
        if (jobSeeker == null)
            return null;

        // 3. Business logic with validation
        // ...

        // 4. Log success
        _logger.LogInformation("Operation succeeded for user {UserId}", userId);
        return result;
    }
    catch (Exception ex)
    {
        // 5. Log error with context
        _logger.LogError(ex, "Error in operation for user {UserId}", userId);
        return null;
    }
}
```

### Controller Layer Patterns

```csharp
var result = await _assessmentService.SomeOperationAsync(userId);

if (result == null)
{
    return BadRequest(new ApiErrorResponse("Descriptive error message"));
}

return Ok(new ApiResponse<SomeDto>(result, "Success message"));
```

### Common Error Scenarios

| Scenario | HTTP Status | Message |
|----------|-------------|---------|
| Not authenticated | 401 | "User not authenticated" |
| Not a job seeker | 400 | "Only job seekers can take assessments" |
| Profile incomplete | 400 | "Please complete your profile" |
| No job title | 400 | "Please set your job title" |
| In cooldown | 400 | "Please wait N days before retaking" |
| Has in-progress | 400 | "Complete or abandon current assessment" |
| Assessment expired | 400 | "Assessment has expired" |
| Question not found | 400 | "Question is invalid" |
| Already answered | 400 | "Question already answered" |
| No active assessment | 404 | "No assessment in progress" |

---

## 14. Frontend Integration Guide

### Typical UI Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                    ASSESSMENT LANDING PAGE                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   [Call GET /eligibility on page load]                              │
│                                                                     │
│   IF isEligible:                                                    │
│     Show "Start Assessment" button                                  │
│     Display: "30 questions, 45 minutes, 70% technical"              │
│                                                                     │
│   IF in cooldown:                                                   │
│     Show countdown: "Available in 45 days"                          │
│     Show current score if exists                                    │
│                                                                     │
│   IF has in-progress:                                               │
│     Show "Resume Assessment" button                                 │
│     [Call GET /current to get status]                               │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                    QUESTION PAGE                                     │
├─────────────────────────────────────────────────────────────────────┤
│   Timer: 38:15 remaining                    Question 5/30           │
│   ─────────────────────────────────────────────────────────────     │
│                                                                     │
│   Technical • Medium Difficulty                                     │
│                                                                     │
│   What is the time complexity of binary search?                     │
│                                                                     │
│   ○ O(n)                                                            │
│   ● O(log n)     [selected]                                         │
│   ○ O(n²)                                                           │
│   ○ O(1)                                                            │
│                                                                     │
│   Progress: ████████░░░░░░░░░░░░░░░░░░░░░░ 16.7%                    │
│                                                                     │
│   [Previous] [Skip]        [Submit Answer]   [Abandon Assessment]   │
└─────────────────────────────────────────────────────────────────────┘
```

### Recommended State Management

```typescript
interface AssessmentState {
  // Eligibility
  eligibility: EligibilityResponse | null;

  // Active attempt
  attemptId: number | null;
  currentQuestion: QuestionResponse | null;
  questionsAnswered: number;
  totalQuestions: number;
  expiresAt: Date | null;

  // Timer
  timeRemainingSeconds: number;

  // UI state
  selectedAnswer: number | null;
  isSubmitting: boolean;
}
```

### Timer Implementation

```typescript
// Start timer after POST /start or GET /current
const startTimer = (expiresAt: Date) => {
  const interval = setInterval(() => {
    const remaining = Math.max(0,
      Math.floor((expiresAt.getTime() - Date.now()) / 1000)
    );

    if (remaining === 0) {
      clearInterval(interval);
      handleExpiration();
    } else {
      setTimeRemaining(remaining);
    }
  }, 1000);

  return () => clearInterval(interval);
};
```

### Handling Connection Loss

```typescript
// On reconnect or page refresh
const resumeAssessment = async () => {
  const status = await api.get('/assessment/current');

  if (status.isExpired) {
    showMessage('Assessment expired');
    redirectToResults();
    return;
  }

  // Restore timer from server time
  startTimer(new Date(status.expiresAt));

  // Get next question
  const question = await api.get('/assessment/question');
  if (question) {
    showQuestion(question);
  } else {
    // All answered, prompt to complete
    promptComplete();
  }
};
```

---

## 15. Testing Scenarios

### Happy Path Test

```
1. Create JobSeeker with ProfileCompletionStep = 4 and JobTitle set
2. GET /eligibility → isEligible = true
3. POST /start → Get attemptId, 30 questions
4. Loop 30 times:
   - GET /question → Get question details
   - POST /answer with valid questionId and selectedAnswerIndex
5. POST /complete → Get scores and question breakdown
6. GET /history → See attempt in list
7. GET /result/{attemptId} → See detailed breakdown
```

### Edge Case Tests

| Scenario | Expected Behavior |
|----------|-------------------|
| **Double start** | Second POST /start fails (already in progress) |
| **Answer wrong question** | POST /answer returns 400 (question not in attempt) |
| **Answer twice** | POST /answer returns 400 (already answered) |
| **Complete early** | POST /complete works with partial answers |
| **Timeout during test** | Next request returns expired status |
| **Abandon and restart** | Must wait 60 days |
| **Recruiter tries** | All endpoints return 400 (wrong role) |

### Load Test Considerations

- Question selection involves multiple DB queries - ensure indexes are used
- Score calculation is CPU-bound - test with concurrent completions
- JSON serialization of question IDs - keep within 500 char limit

---

## Summary

The Assessment Module provides a robust, fair, and scalable system for skill verification. Key design decisions:

1. **Exam mode** ensures score validity
2. **Role-based targeting** provides relevant questions
3. **Seniority-adjusted difficulty** creates appropriate challenge
4. **Lazy expiration** simplifies infrastructure
5. **Comprehensive audit trail** via QuestionIdsJson and AssessmentAnswer records

For questions or contributions, please refer to the project's GitHub repository or contact the backend team.
