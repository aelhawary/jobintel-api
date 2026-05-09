# Assessment Module — Frontend Integration Guide

**Last Updated:** May 2026  
**Base URL:** `http://localhost:5217/api/assessment`

---

## Table of Contents

1. [Overview](#1-overview)
2. [Flow Summary](#2-flow-summary)
3. [Response Format](#3-response-format)
4. [Endpoints Reference](#4-endpoints-reference)
5. [Navigation and Overview Panel](#5-navigation-and-overview-panel)
6. [Answer Submission and Overwrite](#6-answer-submission-and-overwrite)
7. [Timer and Auto-Submit](#7-timer-and-auto-submit)
8. [Review Mode](#8-review-mode)
9. [Frontend Implementation Guide](#9-frontend-implementation-guide)
10. [Error Handling and Edge Cases](#10-error-handling-and-edge-cases)
11. [Troubleshooting](#11-troubleshooting)

---

## 1. Overview

The Assessment module lets job seekers take targeted skill-verification quizzes. Questions are drawn from the candidate's claimed-skill profile and filtered by role family and seniority. The module is designed around these core behaviours:

- **Flexible navigation** — jump to any question by number at any time.
- **Draft answer overwrite** — re-answer any question before submitting; the answered count is not affected.
- **Overview panel** — `/questions` returns an answered/unanswered flag per question for the navigation sidebar.
- **Partial submission** — call `POST /complete` at any time; unanswered questions count as incorrect.
- **Auto-submit on expiry** — timed-out attempts are finalised on the next API call.
- **Full review results** — `GET /result/{attemptId}` returns correct answers, explanations, and per-skill scores after completion.

All endpoints require a valid JWT with the JobSeeker role.

---

## 2. Flow Summary

### Standard Flow

```
GET  /eligibility          → verify user can start
POST /start                → create attempt; receive attemptId + expiresAt
GET  /questions            → load overview panel
GET  /question/{number}    → navigate to a specific question
POST /answer               → save or overwrite an answer (repeat as needed)
POST /complete             → finalise and receive scores
GET  /result/{attemptId}   → review with correct answers and explanations
```

### Resume Flow (page refresh / reconnect)

```
GET  /current
  ├── status == "InProgress" →  GET /questions
  │                             GET /question/{number}  (resume where left off)
  │
  └── status == "Completed"  →  GET /result/{attemptId}  (redirect to review)
```

> Always drive the countdown timer from `expiresAt` returned by the server, not a local start timestamp.

---

## 3. Response Format

### Success

```json
{
  "success": true,
  "message": "Optional message",
  "data": { }
}
```

### Error

```json
{
  "success": false,
  "message": "Human-readable error description"
}
```

### Validation Error (400)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "selectedAnswerIndex": ["Answer index must be between 0 and 3"]
  }
}
```

---

## 4. Endpoints Reference

### GET /eligibility

Check whether the user can start a new assessment. Should be called on the
assessment landing page to determine which UI state to show.

**Response `data`:**

| Field | Type | Description |
|---|---|---|
| `isEligible` | bool | Whether a new assessment can be started |
| `reason` | string? | Why the user is not eligible (null when eligible) |
| `hasCompletedProfile` | bool | Profile wizard step ≥ 4 |
| `hasJobTitle` | bool | Job title is set |
| `hasClaimedSkills` | bool | At least one skill on the profile |
| `claimedSkillsCount` | int | Number of claimed skills |
| `claimedSkills` | array | `[{ skillId, skillName }]` |
| `hasInProgressAssessment` | bool | Whether an attempt is currently InProgress |
| `isInCooldownPeriod` | bool | Cooldown is active after a previous attempt |
| `cooldownEndsAt` | DateTime? | When the cooldown ends |
| `daysUntilEligible` | int? | Days remaining in cooldown |
| `previousAttempts` | int | Total prior attempts |
| `currentScore` | decimal? | Most recent active score |
| `scoreExpiresAt` | DateTime? | When the current active score expires |

```json
// 200 OK — eligible
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
      { "skillId": 101, "skillName": "ASP.NET Core" },
      { "skillId": 202, "skillName": "Entity Framework" }
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

// 200 OK — in cooldown
{
  "success": true,
  "data": {
    "isEligible": false,
    "reason": "Please wait 45 days before taking another assessment",
    "isInCooldownPeriod": true,
    "cooldownEndsAt": "2026-06-22T10:30:00Z",
    "daysUntilEligible": 45,
    "currentScore": 72.50,
    "scoreExpiresAt": "2027-11-08T10:32:15Z"
  }
}
```

---

### POST /start

Start a new assessment. The request body is optional.

**Request body (optional):**

```json
{
  "skillIds": [101, 202]
}
```

Providing `skillIds` restricts question targeting to those specific skills.
Omitting the body (or `skillIds`) causes the server to snapshot all skills
from the job-seeker profile. The server validates that provided IDs are
actually on the profile.

If the question bank cannot satisfy the required 30-question distribution
(for the selected role family, seniority level, and claimed skills), the
start request fails with 400 and no attempt is created.

**Response `data`:**

| Field | Type | Description |
|---|---|---|
| `attemptId` | int | Use this on all subsequent calls |
| `totalQuestions` | int | Total number of questions (always 30; start fails if insufficient questions exist) |
| `technicalQuestions` | int | Technical question count |
| `softSkillQuestions` | int | Soft-skill question count |
| `timeLimitMinutes` | int | Total time allowed |
| `startedAt` | DateTime | UTC start time |
| `expiresAt` | DateTime | UTC expiry — use this for the countdown timer |
| `jobTitle` | string | Job title at time of start |
| `roleFamily` | string | e.g. "Backend", "Frontend" |
| `seniorityLevel` | string | "Junior", "Mid", or "Senior" |
| `retakeNumber` | int | 1 on first attempt |
| `claimedSkillsCount` | int | Skills snapshotted for this attempt |
| `skillAllocations` | array | Per-skill question distribution |

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

Retrieve the in-progress status and remaining time. Use on reconnect or page
refresh to restore the timer.

If the attempt has already expired, the server auto-submits it before
responding and `status` will be `"Completed"`.

```json
// 200 OK — in progress
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

// 200 OK — auto-submitted (was expired)
{
  "success": true,
  "data": {
    "attemptId": 42,
    "status": "Completed",
    "isExpired": true,
    "timeRemainingSeconds": 0,
    ...
  }
}

// 404 Not Found — no in-progress attempt
```

---

### GET /questions

Return the answered/unanswered flag for every question. This drives the
navigation overview panel and should be refreshed after each `POST /answer`.

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

// 404 — no in-progress attempt (auto-submits if expired before returning null)
```

---

### GET /question/{number}

Fetch a specific question by its 1-based position.

If the question has a previously saved answer, `selectedAnswerIndex` is
populated so the UI can pre-select it and allow the user to change it.

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

// 404 — question number out of range or no in-progress attempt
```

> `selectedAnswerIndex` is `null` if the question has not been answered yet.

---

### GET /question

Return the next unanswered question. Returns `404` when all questions have a
saved answer — at that point the UI should prompt the user to submit.

```json
// 404 — all questions answered or no in-progress attempt
{
  "success": false,
  "message": "No more questions or assessment not found"
}
```

---

### POST /answer

Save a new answer or overwrite an existing one.

**Important behaviours:**
- Submitting to an already-answered question **overwrites** the saved answer.
- Overwriting **does not increment** `questionsAnswered`.
- No correctness feedback is returned (exam mode).

```json
// Request
{
  "questionId": 15,
  "selectedAnswerIndex": 1,
  "timeSpentSeconds": 42
}
```

**Validation constraints:**
- `questionId` — required; must belong to the current attempt
- `selectedAnswerIndex` — required; must be 0–3
- `timeSpentSeconds` — optional; must be 0–3600

```json
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

// 400 Bad Request — questionId not in attempt, or attempt expired
```

---

### POST /complete

Finalise the attempt and receive scores. Can be called at any time —
partial submission is allowed. Unanswered questions count as incorrect.

**Note:** `questionResults` is `null` in this response. To get the full
per-question review, call `GET /result/{attemptId}` after completion.

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

// 400 Bad Request — no in-progress attempt
```

---

### POST /abandon

Mark the in-progress attempt as abandoned. Triggers the 60-day cooldown.

```json
// 200 OK
{
  "success": true,
  "data": true,
  "message": "Assessment abandoned successfully"
}

// 400 Bad Request — no in-progress attempt
```

---

### GET /history

Retrieve all past attempts for the current user, ordered newest first.

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

Full review for a completed or abandoned attempt. Returns `404` if the
attempt is still `InProgress` or does not belong to the authenticated user.

`questionResults` is always populated here. `selectedAnswerIndex` is `null`
for questions that were left unanswered.

The `skillId` and `skillName` fields on each question result allow the
review UI to group questions by skill.

```json
// 200 OK
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
    "skillScores": [ /* same shape as POST /complete */ ],
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
      // selectedAnswerIndex is null for unanswered questions
    ]
  }
}

// 404 — not found, not owned by user, or still InProgress
```

---

## 5. Navigation and Overview Panel

The overview panel is driven by `GET /questions`. It returns one status object
per question and should be refreshed after every `POST /answer`.

**Recommended UI behaviour:**
- Render a numbered grid; colour each cell by `isAnswered`.
- Clicking a number calls `GET /question/{number}` and loads that question.
- Pre-select the answer radio button if `selectedAnswerIndex` is not null.
- After the user selects an answer and calls `POST /answer`, refresh the panel.

```
┌────────────────────────────────────────┐
│  Overview                              │
│                                        │
│  ● 1  ● 2  ● 3  ○ 4  ○ 5  ● 6  ○ 7   │
│  ○ 8  ● 9  ○10  ○11  ○12  ○13  ○14   │
│  ...                                   │
│                                        │
│  ● = answered    ○ = unanswered        │
└────────────────────────────────────────┘
```

---

## 6. Answer Submission and Overwrite

Answers work as drafts. The user can change any answer before submitting.

```
First answer for question N:
  POST /answer  →  questionsAnswered + 1

Subsequent answers for question N (overwrite):
  POST /answer  →  questionsAnswered unchanged
                   saved answer index is updated
                   isCorrect is re-evaluated server-side
```

This means `questionsAnswered` counts **distinct answered questions**, not
total submissions. The overview panel (`GET /questions`) remains the source
of truth for which questions have a saved answer.

---

## 7. Timer and Auto-Submit

### Client-Side Timer

Drive the countdown from `expiresAt` returned by the server, not a local
start time. This keeps the timer accurate across network delays, page
refreshes, and device clock drift.

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

  tick(); // run immediately — no 1-second blank on mount
  const id = setInterval(tick, 1000);
  return () => clearInterval(id);   // return cleanup fn for useEffect
};
```

### Auto-Submit Behaviour

There is no background job on the server. Auto-submit is triggered **lazily**
when the user makes any assessment request after expiry:

```
Client timer hits 0
       │
       ▼
handleTimerExpiry() called client-side
       │
       ▼
GET /current   ──→  server detects now > expiresAt
                    FinalizeAttempt() runs synchronously
                    response.data.status == "Completed"
       │
       ▼
redirect to GET /result/{attemptId}
```

> Do not keep calling other endpoints (answer, questions, etc.) once the
> timer hits zero. Call `GET /current` once to confirm auto-submit, then
> navigate to the results page.

### Timer Expiry Handler

```typescript
const handleTimerExpiry = async () => {
  showModal('Time is up. Submitting your answers…');

  const { data: status } = await api.get('/assessment/current');
  // status.status will be "Completed" here
  redirectToResults(status.attemptId);
};
```

---

## 8. Review Mode

After completion, call `GET /result/{attemptId}` to load the full review.

The `questionResults` array contains one entry per question in attempt order,
including questions that were left unanswered (`selectedAnswerIndex: null`).
Use `skillId` / `skillName` to group by skill if needed.

### Recommended Review UI

```
Assessment Complete

Overall Score: 73.33 %  —  Good
Technical:     71.43 %  (15 / 21 correct)
Soft Skills:   77.78 %  (7 / 9 correct)

─── ASP.NET Core (81.82 %) ──────────────────────────────

  Q7. What is the time complexity of binary search?
      ○ O(n)
      ● O(log n)    ← your answer  ✓  Correct
      ○ O(n²)
      ○ O(1)
      Explanation: Binary search halves the search space on each step…

  Q12. What does `IDisposable` clean up?
      ○ Memory allocated on the heap
      ○ Background threads
      ● Unmanaged resources              ← correct answer
      ○  TCP connections only
      Your answer: (not answered)  ✗  Incorrect
```

---

## 9. Frontend Implementation Guide

### Recommended Screen Set

| Screen | Trigger | Key calls |
|---|---|---|
| **Eligibility / Landing** | Navigate to /assessment | `GET /eligibility` |
| **Assessment Shell** | POST /start or resume | `GET /questions`, `GET /question/{n}`, `POST /answer` |
| **Submit Confirmation** | User clicks Submit | `POST /complete` |
| **Results Summary** | After /complete | Shows score; link to review |
| **Review** | User clicks Review | `GET /result/{attemptId}` |
| **History** | Navigate to /assessment/history | `GET /history` |

### Full TypeScript State Shape

```typescript
interface EligibilityResponse {
  isEligible: boolean;
  reason: string | null;
  hasCompletedProfile: boolean;
  hasJobTitle: boolean;
  hasClaimedSkills: boolean;
  claimedSkillsCount: number;
  claimedSkills: { skillId: number; skillName: string }[];
  hasInProgressAssessment: boolean;
  isInCooldownPeriod: boolean;
  cooldownEndsAt: string | null;
  daysUntilEligible: number | null;
  previousAttempts: number;
  currentScore: number | null;
  scoreExpiresAt: string | null;
}

interface QuestionStatus {
  questionNumber: number;
  isAnswered: boolean;
}

interface QuestionResponse {
  questionId: number;
  questionNumber: number;
  totalQuestions: number;
  questionText: string;
  category: 'Technical' | 'SoftSkill';
  difficulty: 'Easy' | 'Medium' | 'Hard';
  options: string[];
  selectedAnswerIndex: number | null;
  timeAllowedSeconds: number;
  timeRemainingInAssessmentSeconds: number;
}

interface SkillScore {
  skillId: number;
  skillName: string;
  category: 'Technical' | 'SoftSkill';
  correctAnswers: number;
  totalQuestions: number;
  score: number;
  isClaimedSkill: boolean;
}

interface QuestionResult {
  questionId: number;
  questionText: string;
  category: 'Technical' | 'SoftSkill';
  difficulty: 'Easy' | 'Medium' | 'Hard';
  options: string[];
  selectedAnswerIndex: number | null;
  correctAnswerIndex: number;
  isCorrect: boolean;
  explanation: string | null;
  timeSpentSeconds: number;
  skillId: number;
  skillName: string;
}

interface AssessmentResult {
  attemptId: number;
  status: string;
  overallScore: number;
  technicalSkillsTotalScore: number;
  softSkillsScore: number;
  totalQuestions: number;
  correctAnswers: number;
  technicalCorrect: number;
  technicalTotal: number;
  softSkillCorrect: number;
  softSkillTotal: number;
  startedAt: string;
  completedAt: string | null;
  timeTakenMinutes: number;
  scoreExpiresAt: string | null;
  jobTitle: string;
  performanceLevel: 'Excellent' | 'Good' | 'Average' | 'Needs Improvement';
  isPassing: boolean;
  skillScores: SkillScore[];
  questionResults: QuestionResult[] | null;
}

interface AssessmentState {
  attemptId: number | null;
  status: 'InProgress' | 'Completed' | 'Abandoned' | null;
  totalQuestions: number;
  expiresAt: string | null;
  questionsAnswered: number;
  questionStatuses: QuestionStatus[];
  currentQuestion: QuestionResponse | null;
  selectedAnswerIndex: number | null;
  timeRemainingSeconds: number;
  result: AssessmentResult | null;
}
```

### Recommended API Call Sequence

```typescript
// Landing page
const eligibility = await api.get<EligibilityResponse>('/assessment/eligibility');

// Start
const start = await api.post('/assessment/start');
// store start.data.attemptId, start.data.expiresAt

// Load overview panel
const statuses = await api.get<QuestionStatus[]>('/assessment/questions');

// Navigate to a question
const question = await api.get<QuestionResponse>(`/assessment/question/${number}`);

// Submit or overwrite answer
await api.post('/assessment/answer', {
  questionId: question.questionId,
  selectedAnswerIndex: selected,
  timeSpentSeconds: elapsed,
});

// Refresh overview panel after each answer
const updated = await api.get<QuestionStatus[]>('/assessment/questions');

// Finalise
const result = await api.post<AssessmentResult>('/assessment/complete');
// result.data.questionResults === null here

// Full review
const review = await api.get<AssessmentResult>(`/assessment/result/${attemptId}`);
// review.data.questionResults is populated
```

### Reconnect on Page Refresh

```typescript
const resumeAssessment = async () => {
  const { data: status } = await api.get('/assessment/current');

  if (!status) {
    // No active attempt
    return;
  }

  if (status.status === 'Completed') {
    // Auto-submitted while away — go straight to results
    redirectToResults(status.attemptId);
    return;
  }

  // Restore timer from server-authoritative value
  startTimer(new Date(status.expiresAt));

  // Reload overview panel
  const { data: statuses } = await api.get('/assessment/questions');
  setQuestionStatuses(statuses);

  // Load first unanswered question
  const { data: nextQuestion } = await api.get('/assessment/question');
  if (nextQuestion) {
    setCurrentQuestion(nextQuestion);
  } else {
    // All questions answered — prompt to submit
    promptToComplete();
  }
};
```

---

## 10. Error Handling and Edge Cases

### HTTP Status Reference

| Scenario | Status | Notes |
|---|---|---|
| Missing or expired JWT | 401 | Redirect to login |
| User is not a JobSeeker | 400 | Show role-mismatch message |
| Profile incomplete / no title / no skills | 400 | Returned via eligibility check |
| Already in cooldown | 400 | Returned via eligibility check |
| `POST /start` with in-progress attempt | 400 | Use `GET /current` to resume |
| `POST /start` with insufficient question bank | 400 | No attempt is created; verify the question pool for the role/seniority/skills |
| `POST /answer` questionId not in attempt | 400 | Always use IDs from `/question/{n}` |
| Invalid `selectedAnswerIndex` | 400 | Model validation error shape |
| `GET /result` while InProgress | 404 | Assessment not yet completed |
| `GET /current` with no attempt | 404 | No in-progress attempt exists |
| Question number out of range | 404 | Check `totalQuestions` from start |

### Edge Cases

**Overwriting an answer** — `POST /answer` on an already-answered question
succeeds with 200. `questionsAnswered` does not change. This is by design.
Do not treat a repeat submission as an error.

**Concurrent start** — If two tabs simultaneously call `POST /start` for the
same user, the second will return 400 because the server enforces a unique
filtered index on in-progress attempts.

**Insufficient question bank** — If the question bank cannot satisfy the full
30-question distribution for the selected role, seniority, or claimed skills,
`POST /start` returns 400 and no attempt is created.

**Auto-submit timing** — If the user's client timer reaches zero but the
network is offline, the attempt is still valid on the server until a request
arrives. The auto-submit fires on the next successful request, even if that
is minutes later.

**Partial results on `/complete`** — `POST /complete` does not return
`questionResults`. Store `attemptId` from the response and call
`GET /result/{attemptId}` to load the per-question review.

**All questions answered but not submitted** — `GET /question` returns 404
when every question has a saved answer. This is the signal to show the
submit prompt, not an error condition.

---

## 11. Troubleshooting

| Symptom | Likely Cause | Fix |
|---|---|---|
| `400` on `POST /start` | User already has an in-progress attempt | Call `GET /current` to check; resume or abandon before starting again |
| `400` on `POST /start` | Insufficient questions for the selected role/seniority/skills | Expand the question bank or adjust claimed skills |
| `400` on `POST /answer` | `questionId` not part of this attempt | Always use the `questionId` returned by `GET /question/{n}` |
| `404` on `GET /questions` | No in-progress attempt | Call `GET /current` first; if expired, it auto-submits |
| `404` on `GET /result/{id}` | Attempt is still `InProgress` | Call `POST /complete` or wait for auto-submit |
| `status == "Completed"` from `GET /current` unexpectedly | Auto-submit triggered after timer expired | Navigate to `GET /result/{attemptId}` |
| `questionResults` is `null` after `POST /complete` | Expected — `/complete` does not return per-question detail | Call `GET /result/{attemptId}` for the full review |
| Timer jumps on page refresh | Client timer derived from a local start time | Always re-seed the timer from `expiresAt` on `GET /current` |
| `selectedAnswerIndex` unexpectedly `null` | Question has not been answered yet in this session | Normal — `null` means unanswered; pre-select nothing |
