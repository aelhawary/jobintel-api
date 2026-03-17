# AI Bootstrap Prompt

Use this prompt as the first message for any new AI agent joining this repository.

## Prompt to Use

You are continuing development on the JobIntel backend API in this repository.

Start by reading these files in this exact order:
1. PROJECT_CONTEXT.md
2. Docs/Context/ARCHITECTURE.md
3. Docs/API/API_REFERENCE.md
4. Docs/Context/DEVELOPMENT_ROADMAP.md

After reading, summarize in 3 short blocks:
- What is already complete
- What is partially implemented
- What should be built next

Then execute work with these rules:
- Follow existing architecture: Controller -> Service -> AppDbContext
- Do not introduce a new architecture style unless explicitly requested
- Keep response and error patterns consistent with existing modules
- Use DTO validation + service business validation
- Preserve ownership and authorization checks
- Avoid breaking current API contracts unless asked

## Current High-Priority Tasks

1. Consistency fixes
- Align profile picture URL generation with actual routes
- Clean outdated wizard comments/labels where behavior has changed

2. Wizard hardening
- Add optional progression guard checks for explicit wizard advance endpoints
- Add tests for advance/skip edge cases

3. Assessment runtime module
- Implement service + controller endpoints for attempt lifecycle
- Reuse existing entities and assessment settings

## Coding and Safety Constraints

- Base all changes on current repository code
- Do not invent missing tables/endpoints without adding the required migration/code
- Keep edits minimal and localized
- Prefer additive changes over broad refactors
- Validate build and resolve introduced errors before finishing

## Suggested First Commands

- dotnet restore
- dotnet build RecruitmentPlatform.sln
- dotnet run --project RecruitmentPlatformAPI

## Expected Output Format from Agent

When done with any task, report:
1. Files changed
2. Behavior change summary
3. Validation performed
4. Risks or follow-up tasks
