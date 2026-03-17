# JobIntel API Reference (AI Handoff)

## Scope
This is a concise operational reference for currently exposed backend endpoints.

## Base URL
- Development HTTP: `http://localhost:5217`
- Development HTTPS: `https://localhost:7113`

## Auth Module (`/api/auth`)

### Public endpoints
- `POST /register`: create new user and role profile
- `POST /login`: email/password login
- `POST /google`: Google OAuth auth/login
- `POST /verify-email`: verify code from email
- `POST /resend-verification`: resend verification code
- `POST /forgot-password`: request reset link
- `POST /validate-reset-token`: verify reset token validity
- `POST /reset-password`: apply new password

### Protected endpoint
- `GET /me`: current user claims-derived profile

### Primary DTOs
- Inputs: `RegisterDto`, `LoginDto`, `GoogleAuthDto`, `EmailVerificationDto`, `ResendVerificationDto`, `ForgotPasswordDto`, `ValidateResetTokenDto`, `ResetPasswordDto`
- Outputs: `AuthResponseDto`, `CurrentUserResponse`

## Job Seeker Core (`/api/jobseeker`)

### Personal info + wizard
- `POST /personal-info`
- `GET /personal-info`
- `GET /wizard-status`
- `POST /wizard/advance/{stepNumber}`
- `GET /job-titles` (public)

### Profile picture
- `POST /picture` (multipart)
- `GET /picture/info`
- `GET /picture`
- `DELETE /picture`
- `GET /picture/exists`

### Primary DTOs
- Inputs: `PersonalInfoRequestDto`
- Outputs: `PersonalInfoDto`, `WizardStatusDto`, `ProfileResponseDto`, `ProfilePictureResponseDto`, `ProfilePictureUploadResultDto`

## Experience (`/api/jobseeker/experience`)
- `GET /`
- `GET /{id}`
- `POST /`
- `PUT /{id}`
- `DELETE /{id}`
- `POST /reorder`
- `GET /exists`

DTOs:
- Input: `ExperienceRequestDto`
- Output: `ExperienceResponseDto`, `ExperienceListResponseDto`

## Education (`/api/jobseeker/education`)
- `GET /`
- `GET /{id}`
- `POST /`
- `PUT /{id}`
- `DELETE /{id}`
- `POST /reorder`
- `GET /exists`

DTOs:
- Input: `EducationRequestDto`
- Output: `EducationResponseDto`, `EducationListResponseDto`

## Projects (`/api/jobseeker/projects`)
- `GET /`
- `POST /`
- `PUT /{projectId}`
- `DELETE /{projectId}`

DTOs:
- Inputs: `AddProjectDto`, `UpdateProjectDto`
- Outputs: `ProjectResponseDto`, `ProjectDto`

## Skills (`/api/jobseeker/skills`)
- `GET /`
- `PUT /`
- `DELETE /`
- `GET /available` (public)

DTOs:
- Input: `UpdateSkillsRequestDto`
- Output: `SkillsResponseDto`, `SkillDto`

## Social Accounts (`/api/jobseeker/social-accounts`)
- `PUT /`
- `GET /`
- `DELETE /`

DTOs:
- Input: `UpdateSocialAccountDto`
- Output: `SocialAccountResponseDto`

## Resume (`/api/jobseeker/resume`)
- `POST /upload` (multipart)
- `GET /`
- `GET /download`
- `DELETE /`
- `GET /exists`

DTOs:
- Output wrappers: `ResumeResponseDto`, `ResumeDto`

## Certificates (`/api/jobseeker/certificates`)
- `GET /`
- `GET /{id}`
- `POST /` (multipart)
- `PUT /{id}` (multipart)
- `DELETE /{id}`
- `GET /{id}/download`

DTOs:
- Input: `CertificateRequestDto`
- Outputs: `CertificateResponseDto`, `CertificateListResponseDto`, `CertificateDto`

## Recruiter Profile (`/api/recruiter`)

### Company info + wizard
- `POST /company-info`
- `GET /company-info`
- `GET /wizard-status`
- `POST /wizard/advance/{stepNumber}`

### Reference lists
- `GET /industries`
- `GET /company-sizes`

### Profile picture
- `POST /picture` (multipart)
- `GET /picture/info`
- `GET /picture`
- `DELETE /picture`
- `GET /picture/exists`

DTOs:
- Input: `RecruiterCompanyInfoRequestDto`
- Outputs: `RecruiterCompanyInfoDto`, `IndustryDto`, `CompanySizeDto`

## Jobs (`/api/jobs`)
- `GET /skills` (searchable skills list)
- `GET /` (recruiter-owned jobs)
- `GET /{id}`
- `POST /`
- `PUT /{id}`
- `PATCH /{id}/deactivate`
- `PATCH /{id}/reactivate`
- `DELETE /{id}`

DTOs:
- Input: `JobRequestDto`
- Outputs: `JobResponseDto`, `JobListResponseDto`, `SkillOptionDto`

## Locations (`/api/locations`)
- `GET /countries?lang=en|ar`
- `GET /languages?lang=en|ar`

DTOs:
- `CountryDto`, `LanguageDto`

## Response Conventions
- Success wrapper generally uses `ApiResponse<T>`
- Error wrapper generally uses `ApiErrorResponse`
- Some modules return specialized response DTOs (`ResumeResponseDto`, `SkillsResponseDto`, etc.)

## Auth and Ownership Notes
- Most profile and jobs routes require Bearer token
- Controllers use user id from JWT claims (`NameIdentifier` / sometimes fallback `sub`)
- Recruiter jobs APIs enforce ownership in service layer

## Known API Consistency Notes
- Profile wizard now uses explicit advance endpoints; entity save no longer implies step advancement
- A minor stored URL inconsistency exists in profile picture service URL formatting vs actual route prefixes
