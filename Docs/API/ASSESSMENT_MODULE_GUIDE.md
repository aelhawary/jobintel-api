# Assessment Module — Comprehensive Technical Guide

**Last Updated:** May 2026  
**Base URL:** `api/assessment`

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

The Assessment Module is a skill-verification system that lets **Job Seekers** take targeted quizzes to validate their technical and soft skills. Questions are drawn from the candidate's claimed-skill profile and filtered by role family and seniority level. The resulting score (0–100) is attached to the candidate's public profile for recruiter visibility.

### Key Features

| Feature | Detail |
|---|---|
| 30-question assessments | 21 technical + 9 soft-skill questions |
| 45-minute time limit | Hard expiry, auto-submitted on next request |
| Claimed-skill targeting | Questions matched to the job seeker's stated skills |
| Per-skill allocation | Start response shows how questions are distributed per skill |
| Flexible navigation | Jump to any question by 1-based number at any time |
| Draft answer overwrite | Re-answer any question before submitting; answered count unchanged |
| Partial submission | Call `POST /complete` at any time; unanswered questions count as incorrect |
| Auto-submit on expiry | Expired attempts are finalised on the next API call |
| 60-day cooldown | Between attempts (applies after completion, abandonment, or expiry) |
| 18-month score validity | Scores expire and the cycle can restart |
| Exam mode | No correctness feedback during the test |
| Full review results | `/result/{id}` returns correct answers, explanations, and per-skill breakdown |

---

## 2. Purpose and Business Value

### For Job Seekers
- **Skill Verification** — prove competency beyond resume claims
- **Profile Enhancement** — stand out with a verified assessment score
- **Self-Assessment** — identify strengths and improvement areas

### For Recruiters
- **Objective Screening** — filter candidates by verified skill scores
- **Role Matching** — scores are scoped to specific role families (Backend, Frontend, etc.)
- **Risk Reduction** — quantified skill levels reduce costly mis-hires

---

## 3. Architecture

### Component Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                      AssessmentController                           │
│         (11 routes, auth, request/response formatting)              │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       IAssessmentService                            │
│       (eligibility, question selection, scoring, finalization)      │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                          AppDbContext                               │
│                   (EF Core, SQL Server)                             │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│   AssessmentQuestion  │  AssessmentAttempt  │  AssessmentAnswer     │
└─────────────────────────────────────────────────────────────────────┘
```

### File Structure

```
RecruitmentPlatformAPI/
├── Controllers/
│   └── AssessmentController.cs          # 11 endpoints, single service injection
├── Services/
│   └── Assessment/
│       ├── IAssessmentService.cs        # Service contract (11 methods)
│       └── AssessmentService.cs        # Full implementation (~1,200 lines)
├── DTOs/
│   └── Assessment/
│       └── AssessmentDtos.cs            # All request/response DTOs
├── Models/
│   └── Assessment/
│       ├── AssessmentQuestion.cs
│       ├── AssessmentAttempt.cs
│       └── AssessmentAnswer.cs
├── Enums/
│   ├── AssessmentStatus.cs              # InProgress, Completed, Abandoned, Expired
│   ├── QuestionCategory.cs              # Technical, SoftSkill
│   ├── QuestionDifficulty.cs            # Easy, Medium, Hard
│   └── ExperienceSeniorityLevel.cs      # Junior, Mid, Senior
├── Configuration/
│   └── AssessmentSettings.cs            # All tuneable constants
└── Data/
    └── Seed/
        └── AssessmentQuestionSeed.cs    # Seed question bank
```

---

## 4. Database Schema

### AssessmentQuestion

| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `QuestionText` | nvarchar(500) | |
| `Category` | int | Technical = 1, SoftSkill = 2 |
| `RoleFamily` | int | Backend, Frontend, FullStack, Other, etc. |
| `SkillId` | int FK | Required — all questions are attributed to a skill |
| `Difficulty` | int | Easy = 1, Medium = 2, Hard = 3 |
| `SeniorityLevel` | int | Junior = 1, Mid = 2, Senior = 3 |
| `Options` | nvarchar(1000) | JSON array of 4 strings |
| `CorrectAnswerIndex` | int | 0–3 |
| `TimePerQuestion` | int? | Seconds; falls back to `AssessmentSettings.DefaultTimePerQuestionSeconds` |
| `IsActive` | bit | Inactive questions are excluded from selection |
| `Explanation` | nvarchar(1000) | Shown after completion |
| `CreatedAt` | datetime2 | |
| `UpdatedAt` | datetime2 | |

### AssessmentAttempt

| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `JobSeekerId` | int FK | |
| `JobTitleId` | int FK | Snapshot of job title at start time |
| `OverallScore` | decimal(5,2)? | Set on completion |
| `TechnicalScore` | decimal(5,2)? | Set on completion |
| `SoftSkillsScore` | decimal(5,2)? | Set on completion |
| `Status` | int | InProgress=1, Completed=2, Abandoned=3, Expired=4 |
| `StartedAt` | datetime2 | |
| `CompletedAt` | datetime2? | |
| `TimeLimitMinutes` | int | Default 45 |
| `TotalQuestions` | int | |
| `QuestionsAnswered` | int | Incremented on first answer; unaffected by overwrites |
| `ExpiresAt` | datetime2 | StartedAt + TimeLimitMinutes |
| `ScoreExpiresAt` | datetime2? | CompletedAt + ScoreValidityMonths |
| `IsActive` | bit | True on the most recent completed attempt only |
| `RetakeNumber` | int | 1 on first attempt, increments per attempt |
| `QuestionIdsJson` | nvarchar(500) | Ordered JSON int array — frozen at start |
| `ClaimedSkillIdsJson` | nvarchar(1000) | Snapshot of claimed skill IDs at start |
| `AlgorithmVersion` | int | Always 2 for current attempts |

### AssessmentAnswer

| Column | Type | Notes |
|---|---|---|
| `Id` | int PK | |
| `AssessmentAttemptId` | int FK | |
| `QuestionId` | int FK | |
| `SelectedAnswerIndex` | int | 0–3; updated in place on overwrite |
| `IsCorrect` | bit | Re-evaluated on each overwrite |
| `TimeSpentSeconds` | int | Updated on each overwrite |
| `AnsweredAt` | datetime2 | Updated on each overwrite |

### Key Indexes

```sql
-- Efficient question filtering during selection
IX_AssessmentQuestion_Filtering
  ON AssessmentQuestion(RoleFamily, Category, Difficulty, SeniorityLevel, IsActive)

-- Fast active-attempt lookup
IX_AssessmentAttempt_JobSeeker_Active
  ON AssessmentAttempt(JobSeekerId, IsActive)

-- History queries
IX_AssessmentAttempt_JobSeeker_Status
  ON AssessmentAttempt(JobSeekerId, Status, StartedAt)

-- Enforce single in-progress attempt per job seeker (unique filtered)
UX_AssessmentAttempt_JobSeeker_InProgress
  ON AssessmentAttempt(JobSeekerId) WHERE Status = 1

-- Prevent duplicate answers per attempt
IX_AssessmentAnswer_Attempt_Question (UNIQUE)
  ON AssessmentAnswer(AssessmentAttemptId, QuestionId)
```

---

## 5. Configuration

All assessment parameters are centralised in `AssessmentSettings.cs`:

```csharp
public static class AssessmentSettings
{
    public const int CooldownDays                  = 60;    // Days between attempts
    public const int ScoreValidityMonths           = 18;    // Months a score remains valid
    public const int DefaultTimeLimitMinutes       = 45;    // Total exam time
    public const int DefaultTimePerQuestionSeconds = 60;    // Per-question guidance time

    public const int TotalQuestionsPerAssessment   = 30;
    public const int TechnicalQuestionsCount       = 21;    // 70 % of total
    public const int SoftSkillQuestionsCount       = 9;     // 30 % of total

    public const decimal MinimumPassingScore       = 50.0m;
}
```

| Setting | Value | Rationale |
|---|---|---|
| 60-day cooldown | Prevents gaming; allows genuine improvement |
| 18-month validity | Skills evolve; keeps scores current |
| 45-minute limit | Long enough for thoughtful answers, short enough to maintain focus |
| 70/30 split | Technical skills are primary; soft skills differentiate candidates |
| 50 % passing | Minimum floor for "verified" status |

---

## 6. Eligibility System

### Checks (evaluated in order)

```
1. ACCOUNT TYPE        → Must be JobSeeker
2. PROFILE EXISTS      → JobSeeker record must exist
3. PROFILE COMPLETE    → ProfileCompletionStep ≥ 4
4. JOB TITLE SET       → JobTitleId must be populated
5. CLAIMED SKILLS      → At least one skill on the profile
6. NO IN-PROGRESS      → No attempt with Status = InProgress
7. COOLDOWN CLEAR      → LastAssessmentDate + 60 days in the past
```

If all checks pass, `isEligible = true` and the response also includes the current
active score and the claimed-skills snapshot.

### Eligibility Response

```json
{
  "isEligible": true,
  "reason": null,
  "hasCompletedProfile": true,
  "hasJobTitle": true,
  "hasClaimedSkills": true,
  "claimedSkillsCount": 3,
  "claimedSkills": [
    { "skillId": 101, "skillName": "ASP.NET Core" },
    { "skillId": 202, "skillName": "Entity Framework" }
  ],
  "hasInProgressAssessment": false,
  "isInCooldownPeriod": false,
  "cooldownEndsAt": null,
  "daysUntilEligible": null,
  "previousAttempts": 1,
  "currentScore": 72.50,
  "scoreExpiresAt": "2027-09-15T10:30:00Z"
}
```

When not eligible, `isEligible = false` and `reason` contains a human-readable explanation.

### Decision Flowchart

```
Check Eligibility
      │
      ├─ Not JobSeeker?          → "Only job seekers can take assessments"
      ├─ Profile step < 4?       → "Please complete your profile"
      ├─ No job title?           → "Please set your job title"
      ├─ No claimed skills?      → "Please select at least one skill"
      ├─ Assessment in progress? → "Complete or abandon current assessment"
      ├─ In cooldown?            → "Please wait N days before retaking"
      │
      └─ ELIGIBLE ✓
```

---

## 7. Question Selection Algorithm

Questions are chosen to cover the job seeker's claimed skills as broadly as possible.

### Step 1 — Derive seniority from experience

```csharp
SeniorityLevel = yearsOfExperience switch
{
    null or <= 2  => Junior,
    >= 3 and <= 5 => Mid,
    _             => Senior
};
```

### Step 2 — Build question pools

- **Technical pool**: active technical questions with a compatible role family  
  (same role family as the job seeker, or either side is `FullStack`).
- **Soft-skill pool**: all active soft-skill questions (role-independent).

### Step 3 — Distribute by claimed skill coverage

1. Distribute the target count evenly across distinct claimed skills;  
   remainder questions are assigned round-robin.
2. For each skill, prefer questions matching the derived seniority level.
3. If a skill has fewer questions than required, fall back to any seniority
   for that skill.
4. If the pool is still short, fill from role-compatible questions regardless
   of skill match.

Soft-skill questions follow the same logic against any claimed soft skills;
if none are claimed, questions are drawn from the general soft-skill pool.

### Step 4 — Finalise and persist

- Combine technical and soft-skill selections, deduplicate, then shuffle.
- Persist `QuestionIdsJson` (ordered, frozen) and `ClaimedSkillIdsJson`
  (snapshot) on the `AssessmentAttempt`.

**Why persist question IDs?**  
Freezing the list ensures consistent question order across page refreshes,
validates that submitted answers belong to this attempt, and enables
exact reconstruction for audit or review.

---

## 8. Assessment Flow

### Full Lifecycle

```
[1] PRE-ASSESSMENT
    GET  /eligibility     → Check requirements and claimed skills
    POST /start           → Create attempt; receive attemptId, expiresAt,
                            skillAllocations

[2] DURING ASSESSMENT (45-minute window)
    GET  /current         → Resume / reconnect; get remaining time
    GET  /questions       → Load overview panel (answered flags per question)
    GET  /question/{n}    → Fetch question n (returns selectedAnswerIndex
                            if previously answered)
    GET  /question        → Fetch next unanswered question
    POST /answer          → Save or overwrite an answer (no correctness shown)

[3] COMPLETION
    POST /complete        → Finalise; receive overall score and skill breakdown
                            (no question-level detail here — use /result/{id})
    POST /abandon         → Give up; triggers 60-day cooldown

[4] POST-ASSESSMENT
    GET  /history         → All past attempts with scores
    GET  /result/{id}     → Full review: per-question breakdown with correct
                            answers, explanations, and skill attribution
```

### Auto-Submit on Expiry

There is no background job. Expiry is handled **lazily** on the next incoming
request:

```
Request arrives for any assessment endpoint
      │
      └─ now > attempt.ExpiresAt?
              │
              Yes → FinalizeAttemptAsync() runs synchronously
                    attempt.Status = Completed
                    Scores are calculated and persisted
                    GetCurrentStatus → returns Completed status
                    GetNextQuestion  → returns null
```

This keeps infrastructure simple and handles the common case of users
simply closing the browser. The attempt is still queryable via `/result/{id}`
after auto-submit.

---

## 9. Scoring System

### Formula

Scores are computed as simple ratios over the question counts for each category.
Because the technical/soft split is fixed at 21/9, this is mathematically
equivalent to a 70/30 weighted average.

```
TechnicalScore  = (technicalCorrect  / technicalTotal)  × 100
SoftSkillsScore = (softSkillCorrect  / softSkillTotal)   × 100
OverallScore    = (totalCorrect      / totalQuestions)   × 100
```

Unanswered questions count as incorrect in all three calculations.  
All scores are rounded to 2 decimal places.

### Example

```
Answers: 15 / 21 technical correct, 7 / 9 soft-skill correct

TechnicalScore  = 15 / 21 × 100 = 71.43
SoftSkillsScore =  7 /  9 × 100 = 77.78
OverallScore    = 22 / 30 × 100 = 73.33
```

### Performance Bands

| Score | Level |
|---|---|
| ≥ 90 | Excellent |
| ≥ 75 | Good |
| ≥ 50 | Average |
| < 50 | Needs Improvement |

Passing threshold: **50 %** (`AssessmentSettings.MinimumPassingScore`).

### Per-Skill Breakdown

In addition to the category totals, every completed attempt carries a
`skillScores` list — one entry per skill that appeared in the question set,
plus any claimed skills that had no questions (score = 0). Each entry includes:

```json
{
  "skillId": 101,
  "skillName": "ASP.NET Core",
  "category": "Technical",
  "correctAnswers": 6,
  "totalQuestions": 8,
  "score": 75.00,
  "isClaimedSkill": true
}
```

### Score Persistence

On completion the service:

1. Writes `OverallScore`, `TechnicalScore`, `SoftSkillsScore` to the attempt.
2. Sets `ScoreExpiresAt = CompletedAt + 18 months`.
3. Sets all other completed attempts for this job seeker to `IsActive = false`.
4. Sets the current attempt to `IsActive = true`.
5. Denormalises onto `JobSeeker`: `CurrentAssessmentScore`, `LastAssessmentDate`,
   `AssessmentJobTitleId`.

---

## 10. API Reference

### Endpoint Summary

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/assessment/eligibility` | Check if user can start |
| `POST` | `/api/assessment/start` | Start new attempt |
| `GET` | `/api/assessment/current` | Get in-progress status |
| `GET` | `/api/assessment/questions` | Overview panel (answered flags) |
| `GET` | `/api/assessment/question/{number}` | Get question by 1-based number |
| `GET` | `/api/assessment/question` | Get next unanswered question |
| `POST` | `/api/assessment/answer` | Save or overwrite an answer |
| `POST` | `/api/assessment/complete` | Finalise and receive scores |
| `POST` | `/api/assessment/abandon` | Abandon in-progress attempt |
| `GET` | `/api/assessment/history` | All past attempts |
| `GET` | `/api/assessment/result/{attemptId}` | Full review for completed attempt |

**Authentication:** all endpoints require `Authorization: Bearer <jwt_token>` with JobSeeker role.

---

### GET /eligibility

```json
// 200 OK
{
  "success": true,
  "data": {
    "isEligible": true,
    "reason": null,
    "hasCompletedProfile": true,
    "hasJobTitle": true,
    "hasClaimedSkills": true,
    "claimedSkillsCount": 2,
    "claimedSkills": [
      { "skillId": 101, "skillName": "ASP.NET Core" }
    ],
    "hasInProgressAssessment": false,
    "isInCooldownPeriod": false,
    "cooldownEndsAt": null,
    "daysUntilEligible": null,
    "previousAttempts": 0,
    "currentScore": null,
    "scoreExpiresAt": null
  }
}
```

---

### POST /start

Optional request body:

```json
{
  "skillIds": [101, 202]
}
```

Omitting the body (or `skillIds`) causes the server to snapshot all skills
from the job-seeker profile. Provided IDs are validated for ownership.

```json
// 200 OK
{
  "success": true,
  "message": "Assessment started successfully",
  "data": {
    "attemptId": 42,
    "totalQuestions": 30,
    "technicalQuestions": 21,
    "softSkillQuestions": 9,
    "timeLimitMinutes": 45,
    "startedAt": "2026-05-08T10:00:00Z",
    "expiresAt": "2026-05-08T10:45:00Z",
    "jobTitle": "Senior Backend Developer",
    "roleFamily": "Backend",
    "seniorityLevel": "Senior",
    "retakeNumber": 1,
    "claimedSkillsCount": 2,
    "skillAllocations": [
      {
        "skillId": 101,
        "skillName": "ASP.NET Core",
        "technicalQuestions": 11,
        "softSkillQuestions": 5,
        "totalQuestions": 16
      },
      {
        "skillId": 202,
        "skillName": "Entity Framework",
        "technicalQuestions": 10,
        "softSkillQuestions": 4,
        "totalQuestions": 14
      }
    ]
  }
}

// 400 Bad Request — not eligible or concurrent start
{
  "success": false,
  "message": "Cannot start assessment. Check eligibility and claimed skills."
}
```

---

### GET /current

Returns in-progress status. If the attempt has expired, it is auto-submitted
before the response is returned and `status` will read `"Completed"`.

```json
// 200 OK
{
  "success": true,
  "data": {
    "attemptId": 42,
    "status": "InProgress",
    "totalQuestions": 30,
    "questionsAnswered": 12,
    "questionsRemaining": 18,
    "startedAt": "2026-05-08T10:00:00Z",
    "expiresAt": "2026-05-08T10:45:00Z",
    "timeRemainingSeconds": 1980,
    "progressPercentage": 40.0,
    "isExpired": false
  }
}

// 404 — no in-progress attempt
```

---

### GET /questions

Returns the answered/unanswered flag for every question. Use this to drive
the navigation overview panel.

```json
// 200 OK
{
  "success": true,
  "data": [
    { "questionNumber": 1, "isAnswered": true },
    { "questionNumber": 2, "isAnswered": false },
    { "questionNumber": 3, "isAnswered": true }
  ]
}

// 404 — no in-progress attempt (auto-submitted if expired)
```

---

### GET /question/{number} and GET /question

`GET /question/{number}` — fetch question at the given 1-based position.  
`GET /question` — fetch the next unanswered question.

If the question has a saved answer, `selectedAnswerIndex` is populated,
enabling in-exam review and answer changes.

```json
// 200 OK
{
  "success": true,
  "data": {
    "questionId": 15,
    "questionNumber": 7,
    "totalQuestions": 30,
    "questionText": "What is the time complexity of binary search?",
    "category": "Technical",
    "difficulty": "Medium",
    "options": ["O(n)", "O(log n)", "O(n²)", "O(1)"],
    "selectedAnswerIndex": 1,
    "timeAllowedSeconds": 60,
    "timeRemainingInAssessmentSeconds": 1940
  }
}

// 404 — question number out of range, all questions answered (GET /question),
//        or no in-progress attempt
```

---

### POST /answer

Saves a new answer or overwrites an existing one.  
**Overwriting does not change `questionsAnswered`.** No correctness
information is returned while the assessment is in progress.

```json
// Request
{
  "questionId": 15,
  "selectedAnswerIndex": 1,
  "timeSpentSeconds": 42
}

// 200 OK
{
  "success": true,
  "data": {
    "success": true,
    "questionsAnswered": 7,
    "questionsRemaining": 23,
    "isAssessmentComplete": false,
    "timeRemainingSeconds": 1898,
    "progressPercentage": 23.3
  }
}

// 400 — questionId not part of this attempt, or attempt expired
```

---

### POST /complete

Finalises the attempt and calculates scores.  
Partial completion is supported — unanswered questions count as incorrect.  
Returns scores and per-skill breakdown. **Does not include per-question detail.**
Use `GET /result/{attemptId}` for the full review.

```json
// 200 OK
{
  "success": true,
  "message": "Assessment completed successfully",
  "data": {
    "attemptId": 42,
    "status": "Completed",
    "overallScore": 73.33,
    "technicalSkillsTotalScore": 71.43,
    "softSkillsScore": 77.78,
    "totalQuestions": 30,
    "correctAnswers": 22,
    "technicalCorrect": 15,
    "technicalTotal": 21,
    "softSkillCorrect": 7,
    "softSkillTotal": 9,
    "startedAt": "2026-05-08T10:00:00Z",
    "completedAt": "2026-05-08T10:32:15Z",
    "timeTakenMinutes": 32,
    "scoreExpiresAt": "2027-11-08T10:32:15Z",
    "jobTitle": "Senior Backend Developer",
    "performanceLevel": "Good",
    "isPassing": true,
    "skillScores": [
      {
        "skillId": 101,
        "skillName": "ASP.NET Core",
        "category": "Technical",
        "correctAnswers": 9,
        "totalQuestions": 11,
        "score": 81.82,
        "isClaimedSkill": true
      }
    ],
    "questionResults": null
  }
}

// 400 — no in-progress attempt
```

---

### POST /abandon

Marks the attempt as `Abandoned` and starts the 60-day cooldown.

```json
// 200 OK
{ "success": true, "data": true, "message": "Assessment abandoned successfully" }

// 400 — no in-progress attempt
```

---

### GET /history

```json
// 200 OK
{
  "success": true,
  "data": {
    "totalAttempts": 2,
    "bestScore": 73.33,
    "currentActiveScore": 73.33,
    "attempts": [
      {
        "attemptId": 42,
        "status": "Completed",
        "overallScore": 73.33,
        "jobTitle": "Senior Backend Developer",
        "startedAt": "2026-05-08T10:00:00Z",
        "completedAt": "2026-05-08T10:32:15Z",
        "retakeNumber": 1,
        "isActive": true,
        "isScoreExpired": false,
        "performanceLevel": "Good"
      }
    ]
  }
}
```

---

### GET /result/{attemptId}

Full review mode. Only available for completed or abandoned attempts —
returns 404 if the attempt is still `InProgress`.

`QuestionResultDto` includes `skillId` and `skillName` so the frontend can
group results by skill.

```json
// 200 OK — same shape as POST /complete, but questionResults is populated
{
  "success": true,
  "data": {
    "attemptId": 42,
    "status": "Completed",
    "overallScore": 73.33,
    "technicalSkillsTotalScore": 71.43,
    "softSkillsScore": 77.78,
    "totalQuestions": 30,
    "correctAnswers": 22,
    "technicalCorrect": 15,
    "technicalTotal": 21,
    "softSkillCorrect": 7,
    "softSkillTotal": 9,
    "startedAt": "2026-05-08T10:00:00Z",
    "completedAt": "2026-05-08T10:32:15Z",
    "timeTakenMinutes": 32,
    "scoreExpiresAt": "2027-11-08T10:32:15Z",
    "jobTitle": "Senior Backend Developer",
    "performanceLevel": "Good",
    "isPassing": true,
    "skillScores": [ /* ... */ ],
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
        "explanation": "Binary search halves the search space on each step, giving O(log n).",
        "timeSpentSeconds": 42,
        "skillId": 101,
        "skillName": "ASP.NET Core"
      }
      // ... one entry per question; selectedAnswerIndex is null for unanswered
    ]
  }
}

// 404 — attempt not found, not owned by user, or still InProgress
```

---

## 11. Exam Mode Design

No correctness feedback is given during the test. After `POST /answer` the
response only confirms the answer was recorded and reports progress:

```
✓ Answer saved (7 / 30 · 23.3 %)
  Time remaining: 31:38
```

Full details — correct answers, explanations, per-skill scores — are available
only after calling `POST /complete` (scores) or `GET /result/{id}` (full review).

This design prevents iterative guessing and ensures scores reflect genuine knowledge.

---

## 12. State Machine

### Status Transitions

```
                    ┌─────────────┐
                    │   (start)   │
                    └──────┬──────┘
                           │ POST /start
                           ▼
                    ┌─────────────┐
          ┌────────│  InProgress  │────────┐
          │        └──────┬──────┘        │
          │               │               │
    POST /abandon   POST /complete   Timer expires
          │               │         (auto-submit)
          ▼               ▼               ▼
    ┌──────────┐   ┌──────────┐   ┌──────────────┐
    │ Abandoned│   │ Completed│   │ Completed    │
    └──────────┘   └──────────┘   │(via auto-    │
         │              │         │ submit)      │
         └──────┬────── ┴ ────────┴──────────────┘
                │
         60-day cooldown
                │
         Can start again
```

> **Note on expiry:** The `Expired` enum value (4) exists but is not written by
> the service. Timed-out attempts are always finalised as `Completed` by the
> auto-submit path. The `Expired` value is reserved for possible future use.

### Status Definitions

| Status | Int | Description |
|---|---|---|
| `InProgress` | 1 | Currently in-flight |
| `Completed` | 2 | Scores calculated and persisted |
| `Abandoned` | 3 | Explicitly abandoned by the user |
| `Expired` | 4 | Reserved; not written by current service |

### Constraints

- **One in-progress per job seeker** — enforced by the `UX_AssessmentAttempt_JobSeeker_InProgress` unique filtered index.
- **One active completed per job seeker** — managed by the service on finalisation.
- **All terminal paths** (complete, abandon, auto-submit) set `LastAssessmentDate` and start the cooldown.

---

## 13. Error Handling

### Controller Pattern

```csharp
var result = await _assessmentService.SomeMethodAsync(userId, ...);
if (result == null)
    return BadRequest(new ApiErrorResponse("Descriptive message"));
return Ok(new ApiResponse<SomeDto>(result, "Success message"));
```

Service methods return `null` on business-logic failure and throw only on
unexpected exceptions (which are caught internally and logged).

### Common HTTP Responses

| Scenario | Status | Message |
|---|---|---|
| Missing / expired JWT | 401 | "User not authenticated" |
| Not a JobSeeker | 400 | (eligibility check) |
| Profile incomplete | 400 | (eligibility check) |
| No job title | 400 | (eligibility check) |
| No claimed skills | 400 | (eligibility check) |
| In cooldown | 400 | (eligibility check) |
| Assessment in progress | 400 | (eligibility check) |
| Question not in attempt | 400 | "Failed to submit answer …" |
| Invalid answer index | 400 | Model validation error |
| No in-progress attempt | 404 | "No assessment in progress" |
| Question out of range | 404 | "Question not found" |
| Result not found / in-progress | 404 | "Assessment result not found or not yet completed" |

---

## 14. Frontend Integration Guide

### Landing Page Logic

```
On page load → GET /eligibility

isEligible = true         → show "Start Assessment" button
hasInProgressAssessment   → show "Resume" button; call GET /current
isInCooldownPeriod        → show cooldown countdown from cooldownEndsAt
                            show current score if currentScore is set
```

### State Shape (TypeScript)

```typescript
interface AssessmentState {
  // Attempt
  attemptId: number | null;
  status: 'InProgress' | 'Completed' | 'Abandoned' | null;
  totalQuestions: number;
  expiresAt: Date | null;

  // Progress
  questionsAnswered: number;
  questionStatuses: { questionNumber: number; isAnswered: boolean }[];

  // Current question
  currentQuestion: QuestionResponse | null;
  selectedAnswerIndex: number | null;

  // Timer (client-side countdown from expiresAt)
  timeRemainingSeconds: number;

  // Review
  result: AssessmentResultResponse | null;
}
```

### Timer Implementation

Drive the countdown entirely from `expiresAt` returned by the server.
Never derive remaining time from a local start timestamp.

```typescript
const startTimer = (expiresAt: Date): (() => void) => {
  const tick = () => {
    const remaining = Math.max(
      0,
      Math.floor((expiresAt.getTime() - Date.now()) / 1000)
    );
    setTimeRemainingSeconds(remaining);
    if (remaining === 0) handleTimerExpiry();
  };

  tick(); // run immediately to avoid 1-second blank
  const id = setInterval(tick, 1000);
  return () => clearInterval(id);
};
```

### Reconnect / Page Refresh

```typescript
const resumeInProgressAssessment = async () => {
  const { data: status } = await api.get('/assessment/current');
  if (!status) return; // no attempt in progress

  // Restore server-authoritative timer
  startTimer(new Date(status.expiresAt));

  if (status.status === 'Completed') {
    // Auto-submitted while away
    redirectToResults(status.attemptId);
    return;
  }

  // Reload overview panel
  const { data: statuses } = await api.get('/assessment/questions');
  setQuestionStatuses(statuses);

  // Open at the first unanswered question
  const { data: question } = await api.get('/assessment/question');
  if (question) setCurrentQuestion(question);
  else promptToComplete(); // all questions answered
};
```

### Timer Expiry Handling

When the client timer reaches zero, the next API call will trigger auto-submit.
Do not make any assessment calls before showing the user a message.

```typescript
const handleTimerExpiry = async () => {
  showExpiryModal('Time is up. Submitting your answers…');
  const { data: status } = await api.get('/assessment/current');
  // status.status === 'Completed' at this point
  redirectToResults(status.attemptId);
};
```

---

## 15. Testing Scenarios

### Happy Path

```
1. JobSeeker with ProfileCompletionStep = 4, JobTitle, and ≥1 claimed skill
2. GET /eligibility → isEligible = true
3. POST /start → receive attemptId and 30 questions
4. Repeat 30 times:
     GET  /question/{n}
     POST /answer
5. POST /complete → scores and skillScores
6. GET  /result/{attemptId} → full question-level review
7. GET  /history → attempt appears in list
```

### Edge Cases

| Scenario | Expected Result |
|---|---|
| Double `POST /start` | 400 — attempt already in progress |
| `POST /answer` wrong questionId | 400 — question not part of attempt |
| `POST /answer` already-answered question | **200 — answer is overwritten** |
| `POST /complete` with partial answers | 200 — unanswered count as incorrect |
| Timer expires; next request | Auto-submit fires; status = Completed |
| `GET /result/{id}` while InProgress | 404 |
| Recruiter calls any endpoint | 400 (only JobSeekers can assess) |
| `POST /start` with `skillIds` subset | Only those skills are targeted |

### What to Verify in Unit Tests

- Overwrite does not increment `QuestionsAnswered`
- `GetQuestionByNumberAsync` returns `selectedAnswerIndex` for an answered question
- `CompleteAssessmentAsync` returns `OverallScore = 0` when no answers given
- `GetCurrentStatusAsync` on expired attempt triggers auto-submit and returns `Completed`
- `CheckEligibilityAsync` counts only `AlgorithmVersion = 2` attempts in `previousAttempts`
