# JobIntel Backend Development Roadmap

## Goal
Prioritize the next backend work so a new AI agent can continue implementation with minimal ambiguity.

## Current Baseline
Implemented and stable:
- Authentication and account security flows
- Job seeker profile modules and wizard state API
- Recruiter profile and recruiter job CRUD
- Core file upload/download modules (resume/certificate/picture)

Partially modeled but not productized:
- Assessment runtime
- Recommendation/matching runtime

## Priority Plan

## Phase 1 - Consistency and Technical Hardening (Short)

### 1.1 Response and route consistency
- Align profile picture generated URL paths with actual controller routes
- Standardize response wrappers where mixed wrappers exist

### 1.2 Wizard hardening
- Add optional guardrails in `wizard/advance/{step}` to enforce minimum completion criteria before advancing
- Add tests for skip/advance edge cases

### 1.3 API hygiene
- Clean outdated step labels/comments in controllers/services/DTO docs
- Ensure endpoint docs match actual behavior after wizard decoupling

Deliverables:
- Consistent route URLs
- Updated docs/comments
- Regression tests for wizard flows

## Phase 2 - Assessment Module (Medium)

### 2.1 Service and controller surface
Build APIs for:
- Start assessment attempt
- Get next question / question set
- Submit answer
- Complete/expire attempt
- Get result summary

### 2.2 Business constraints to enforce
- Cooldown period between attempts
- One in-progress attempt per user (already indexed at DB)
- Role-family-aware question selection
- Score computation using configured weights

### 2.3 Persistence and mapping
- Use existing entities: `AssessmentQuestion`, `AssessmentAttempt`, `AssessmentAnswer`
- Add DTO set for request/response contracts

Deliverables:
- Full assessment API module with integration tests

## Phase 3 - Recommendation Module (Medium)

### 3.1 Recommendation generation pipeline
- Implement score calculation between job requirements and job seeker profile
- Persist to `Recommendation` table

### 3.2 API endpoints
- Recruiter endpoint: get recommended candidates for a job
- Job seeker endpoint: get recommended jobs
- Recompute/regenerate endpoint (admin or internal trigger)

### 3.3 Ranking and explainability
- Include score breakdown fields in response DTOs
- Add deterministic ordering and pagination

Deliverables:
- Functional recommendations API with documented scoring formula

## Phase 4 - Job Seeker Job Interaction (Medium/Long)

### 4.1 Missing product domain
Implement job seeker-facing job pipeline:
- Public/authorized job feed for seekers
- Apply/unapply workflow
- Application status tracking

### 4.2 Data model additions
Likely needed:
- `JobApplication` entity and migration
- Optional recruiter decision fields and timestamps

Deliverables:
- Job seeker job lifecycle APIs

## Phase 5 - Platform Hardening (Long)

### 5.1 Middleware and error strategy
- Add global exception middleware with unified error payloads
- Add request correlation IDs and structured logging enrichment

### 5.2 Test strategy
- Add dedicated test projects
- Cover service logic + key controller integration paths

### 5.3 Operational readiness
- Add production-oriented CORS policy strategy
- Move secrets strictly to environment/secret manager

Deliverables:
- Higher reliability and maintainability baseline

## Suggested Execution Order
1. Phase 1 (consistency fixes)
2. Phase 2 (assessment runtime)
3. Phase 3 (recommendations)
4. Phase 4 (job application lifecycle)
5. Phase 5 (platform hardening)

## AI Agent Starter Tasks (First 3 Tasks)
1. Implement and test profile picture URL consistency fix across service responses
2. Add wizard advancement guard checks and unit tests
3. Scaffold assessment controller/service/DTO contracts using existing entities and `AssessmentSettings`

## Assumptions
- Roadmap prioritization is based on current code presence and inferred product progression.
- If product priorities changed externally, reorder phases but keep module dependency order (assessment before recommendation is preferred).
