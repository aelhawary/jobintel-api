# API Reference

**Base URL:** `http://localhost:5217`  
**API Version:** 1.2.0  
**Last Updated:** December 28, 2025

---

## Table of Contents

1. [Authentication](#authentication)
2. [Response Format](#response-format)
3. [Error Handling](#error-handling)
4. [Status Codes](#status-codes)
5. [Endpoints](#endpoints)
   - [Register User](#1-register-user)
   - [Login](#2-login)
   - [Google OAuth](#3-google-oauth-authentication)
   - [Verify Email](#4-verify-email)
   - [Resend Verification Code](#5-resend-verification-code)
   - [Forgot Password](#6-forgot-password)
   - [Verify Reset OTP](#7-verify-reset-otp)
   - [Reset Password](#8-reset-password)
   - [Get Current User](#9-get-current-user)
6. [Profile Wizard Endpoints](#profile-wizard-endpoints)
   - [Save Personal Info](#10-save-personal-info)
   - [Get Personal Info](#11-get-personal-info)
   - [Get Wizard Status](#12-get-wizard-status)
   - [Get Job Titles](#13-get-job-titles)
   - [Get Countries](#14-get-countries)
   - [Get Languages](#15-get-languages)

---

## Authentication

All protected endpoints require a JWT token in the Authorization header:

```
Authorization: Bearer {token}
```

**Token Expiry:** 24 hours  
**Token Format:** JWT (JSON Web Token)

---

## Response Format

All endpoints return responses in this format:

**Note:** The API is configured to return camelCase JSON property names (standard for REST APIs).

```typescript
interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  resetToken?: string;
  user?: UserInfo;
}

interface UserInfo {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  accountType: "JobSeeker" | "Recruiter";
  isEmailVerified: boolean;
  isActive: boolean;
}
```

---

## Endpoints

### 1. Register User

**Endpoint:** `POST /api/auth/register`  
**Authentication:** Not required  
**Description:** Register a new JobSeeker or Recruiter account

#### Request Body

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "accountType": "JobSeeker"
}
```

**Field Requirements:**
- `firstName` (required, max 50 chars): Letters, spaces, hyphens, apostrophes, periods
- `lastName` (required, max 50 chars): Letters, spaces, hyphens, apostrophes, periods
- `email` (required, max 255 chars): Valid email format with proper structure (e.g., user@domain.com)
- `password` (required, 8-100 chars): Must contain uppercase, lowercase, and digit
- `confirmPassword` (required): Must match `password`
- `accountType` (required): "JobSeeker" or "Recruiter" (case-insensitive)

**Note:** Phone number and company name will be collected during the profile completion process.

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Registration successful. Please check your email to verify your account.",
  "user": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "accountType": "JobSeeker",
    "isEmailVerified": false,
    "isActive": true
  }
}
```

**Note:** No token is returned until email is verified.

#### Error Response (400 Bad Request)

```json
{
  "success": false,
  "message": "An account with this email already exists. Try logging in or use 'Forgot password'."
}
```

**Common Error Messages:**
- "An account with this email already exists..."
- "Invalid accountType. Allowed values: JobSeeker, Recruiter."
- "Passwords do not match"
- "Password must be at least 8 characters"
- "Invalid email format. Please provide a valid email address"
- Validation errors from DataAnnotations

---

### 5. Resend Verification Code

**Endpoint:** `POST /api/auth/resend-verification`  
**Authentication:** Not required  
**Description:** Resend email verification code to user's email

#### Request Body

```json
{
  "email": "john.doe@example.com"
}
```

**Field Requirements:**
- `email` (required, max 255 chars): Valid email format

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "A new verification code has been sent to your email."
}
```

#### Error Response (400 Bad Request)

```json
{
  "success": false,
  "message": "User not found."
}
```

**Common Error Messages:**
- "User not found."
- "Email is already verified."
- "Please wait a moment before requesting another code."

---

### 4. Verify Email

**Endpoint:** `POST /api/auth/verify-email`
**Authentication:** Not required  
**Description:** Verify email address with 6-digit OTP code. After successful verification, users must log in to receive a JWT token.

#### Request Body

```json
{
  "email": "john.doe@example.com",
  "verificationCode": "123456"
}
```

**Field Requirements:**
- `email` (required): Registered email address
- `verificationCode` (required): Exactly 6 digits (numeric only)

**OTP Details:**
- Expires in: 15 minutes
- Format: 6-digit numeric code
- Only the most recent code is valid

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Email verified successfully! Please log in to continue.",
  "user": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "accountType": "JobSeeker",
    "isEmailVerified": true,
    "isActive": true
  }
}
```

**Important:** After successful verification, use the login endpoint (`POST /api/auth/login`) to receive your JWT token.

#### Error Response (400 Bad Request)

```json
{
  "success": false,
  "message": "Invalid verification code. Please use the most recent code sent to your email."
}
```

**Common Error Messages:**
- "User not found."
- "No valid verification code found. Please request a new one."
- "Invalid verification code..."
- "Verification code has expired. Please request a new one."

---

### 3. Resend Verification Code

**Endpoint:** `POST /api/auth/resend-verification`  
**Authentication:** Not required  
**Description:** Resend email verification code to user's email

#### Request Body

```json
{
  "email": "john.doe@example.com"
}
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Verification code sent successfully. Please check your email."
}
```

#### Error Response (400 Bad Request)

```json
{
  "success": false,
  "message": "Email is already verified."
}
```

**Note:** All previous verification codes are invalidated when a new one is sent.

---

### 2. Login

**Endpoint:** `POST /api/auth/login`  
**Authentication:** Not required  
**Description:** Login with email and password

#### Request Body

```json
{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Note:** If your account was created with Google, you must use the Google OAuth endpoint instead.

---

### 3. Google OAuth Authentication

**Endpoint:** `POST /api/auth/google`  
**Authentication:** Not required  
**Description:** Authenticate with Google account. Creates new account if user doesn't exist, or logs in existing user.

#### Request Body

```json
{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjE4MmU0...",
  "accountType": "JobSeeker"
}
```

**Field Requirements:**
- `idToken` (required): Google ID token from frontend (@react-oauth/google)
- `accountType` (required): "JobSeeker" or "Recruiter" (case-insensitive - "jobseeker", "RECRUITER" also accepted)

#### Success Response (200 OK)

**New User:**
```json
{
  "success": true,
  "message": "Account created successfully. Welcome!",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "firstName": "Ahmed",
    "lastName": "Hassan",
    "email": "ahmed@gmail.com",
    "accountType": "JobSeeker",
    "isEmailVerified": true,
    "isActive": true
  }
}
```

**Existing User:**
```json
{
  "success": true,
  "message": "Login successful.",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "firstName": "Ahmed",
    "lastName": "Hassan",
    "email": "ahmed@gmail.com",
    "accountType": "JobSeeker",
    "isEmailVerified": true,
    "isActive": true
  }
}
```

#### Error Responses (400 Bad Request)

**Invalid Token:**
```json
{
  "success": false,
  "message": "Invalid Google token. Please try again."
}
```

**Unverified Google Email:**
```json
{
  "success": false,
  "message": "Your Google email is not verified. Please verify your email with Google first."
}
```

**Deactivated Account:**
```json
{
  "success": false,
  "message": "Your account is deactivated. Please contact support."
}
```

**Important Notes:**
- Google accounts automatically have `isEmailVerified: true`
- No email verification step required for Google users
- Profile pictures from Google are stored in `ProfilePictureUrl` field
- See [Google OAuth Integration Guide](./GOOGLE_AUTH_GUIDE.md) for frontend implementation

---

### 4. Verify Email

**Endpoint:** `POST /api/auth/login`  
**Authentication:** Not required  
**Description:** Login with email and password

**Important:** 
- Email must be verified before login
- If your account was created with Google, you must use the Google OAuth endpoint (`POST /api/auth/google`) instead

#### Request Body

```json
{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Login successful.",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "accountType": "JobSeeker",
    "isEmailVerified": true,
    "isActive": true
  }
}
```

#### Error Response (401 Unauthorized)

```json
{
  "success": false,
  "message": "Invalid email or password."
}
```

**Common Error Messages:**
- "Invalid email or password." (wrong credentials or user not found)
- "This account uses Google sign-in. Please use 'Continue with Google' instead." (OAuth user trying password login)
- "Your email address isn't verified yet. Please verify to continue or request a new verification code."
- "Your account is deactivated. Please contact support."

---

### 6. Forgot Password

**Endpoint:** `POST /api/auth/forgot-password`  
**Authentication:** Not required  
**Description:** Request password reset OTP code via email

**Security Note:** Always returns 200 OK to prevent email enumeration.

#### Request Body

```json
{
  "email": "john.doe@example.com"
}
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "If your email is associated with an account, you'll receive a password reset code shortly."
}
```

**OTP Details:**
- Expires in: 15 minutes
- Format: 6-digit numeric code

**Note:** Response is the same whether email exists or not (security feature).

---

### 7. Verify Reset OTP

**Endpoint:** `POST /api/auth/verify-reset-otp`  
**Authentication:** Not required  
**Description:** Verify password reset OTP and receive reset token

#### Request Body

```json
{
  "email": "john.doe@example.com",
  "otpCode": "123456"
}
```

**Field Requirements:**
- `email` (required): Registered email address
- `otpCode` (required): Exactly 6 digits

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "OTP verified successfully. You can now reset your password.",
  "resetToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Important:** Store `resetToken` for password reset. It expires in 5 minutes.

#### Error Response (400 Bad Request)

```json
{
  "success": false,
  "message": "Invalid OTP code. Please check and try again."
}
```

**Common Error Messages:**
- "User not found."
- "No valid OTP found. Please request a new one."
- "Invalid OTP code..."
- "OTP has expired. Please request a new one."

---

### 8. Reset Password

**Endpoint:** `POST /api/auth/reset-password`  
**Authentication:** Not required  
**Description:** Reset password using reset token from OTP verification

#### Request Body

```json
{
  "email": "john.doe@example.com",
  "resetToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "newPassword": "NewSecurePass123!",
  "confirmNewPassword": "NewSecurePass123!"
}
```

**Field Requirements:**
- `email` (required): Must match email from token
- `resetToken` (required): Token from verify-reset-otp endpoint
- `newPassword` (required, 8-100 chars): Must contain uppercase, lowercase, and digit
- `confirmNewPassword` (required): Must match `newPassword`

**Token Details:**
- Expires in: 5 minutes
- Single-use: Cannot be reused after successful reset

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Password reset successfully. You can now login with your new password."
}
```

#### Error Response (400 Bad Request)

```json
{
  "success": false,
  "message": "Invalid or expired reset token. Please verify your OTP again."
}
```

**Common Error Messages:**
- "Invalid or expired reset token..."
- "Email does not match reset token."
- "No valid OTP found. Please request a new one."
- "Passwords do not match"

---

### 9. Get Current User

**Endpoint:** `GET /api/auth/me`  
**Authentication:** Required (JWT token)  
**Description:** Get current authenticated user information

#### Headers

```
Authorization: Bearer {token}
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "user": {
    "id": "1",
    "email": "john.doe@example.com",
    "name": "John Doe",
    "role": "JobSeeker",
    "firstName": "John",
    "lastName": "Doe"
  }
}
```

**Note:** Response format differs slightly from other endpoints (returns object with `success` and `user`).

#### Error Response (401 Unauthorized)

No body returned. HTTP 401 status indicates:
- Missing token
- Invalid token
- Expired token

---

## Profile Wizard Endpoints

The profile wizard is a 6-step process for job seekers to complete their profile:
1. Personal Information
2. Projects
3. CV Upload
4. Experience
5. Education
6. Social Links

### 10. Save Personal Info

**Endpoint:** `POST /api/profile/personal-info`  
**Authentication:** Required (JWT token)  
**Description:** Save or update personal information (Step 1 of profile wizard)

#### Headers

```
Authorization: Bearer {token}
```

#### Request Body

```json
{
  "jobTitleId": 12,
  "yearsOfExperience": 3,
  "countryId": 1,
  "city": "Cairo",
  "phoneNumber": "+201234567890",
  "firstLanguageId": 1,
  "firstLanguageProficiency": "Native",
  "secondLanguageId": 2,
  "secondLanguageProficiency": "Advanced"
}
```

**Field Requirements:**
- `jobTitleId` (required): Integer - Must be valid ID from GET /api/profile/job-titles
- `yearsOfExperience` (required): Integer between 0 and 50
- `countryId` (required): Integer - Must be valid ID from GET /api/locations/countries
- `city` (required, max 100 chars): City of residence (auto-normalized to Title Case)
- `phoneNumber` (optional, max 20 chars): E.164 international format (e.g., +201234567890)
- `firstLanguageId` (required): Integer - Must be valid ID from GET /api/locations/languages
- `firstLanguageProficiency` (required): "Native", "Advanced", "Intermediate", or "Beginner"
- `secondLanguageId` (optional): Integer - Must be valid ID from GET /api/locations/languages and different from firstLanguageId
- `secondLanguageProficiency` (conditional): Required ONLY if secondLanguageId is provided

**Important Notes:**
- Client must send only IDs for jobTitle, country, and languages (not text values)
- Localized names are returned in GET response but not accepted in POST/PUT requests
- Validation ensures secondLanguageId != firstLanguageId

#### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Personal information saved successfully",
  "profileCompletionStep": 1
}
```

#### Error Responses

**400 Bad Request - Invalid Job Title:**
```json
{
  "success": false,
  "message": "Invalid job title. Please select from the provided list"
}
```

**400 Bad Request - Validation Error:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "SecondLanguageProficiency": ["Second language proficiency is required when a second language is provided"]
  }
}
```

**401 Unauthorized:**
Missing or invalid JWT token.

---

### 11. Get Personal Info

**Endpoint:** `GET /api/profile/personal-info`  
**Authentication:** Required (JWT token)  
**Description:** Get current personal information

#### Headers

```
Authorization: Bearer {token}
```

#### Query Parameters

- `lang` (optional): Language code for localized names ("en" or "ar", default: "en")

**Example:** `GET /api/profile/personal-info?lang=ar`

#### Success Response (200 OK)

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
    "firstLanguageProficiency": "Native",
    "secondLanguageId": 2,
    "secondLanguage": "English",
    "secondLanguageProficiency": "Advanced"
  }
}
```

**With Arabic localization (`?lang=ar`):**
```json
{
  "success": true,
  "data": {
    "jobTitleId": 12,
    "jobTitle": "Backend Developer",
    "yearsOfExperience": 3,
    "countryId": 1,
    "country": "مصر",
    "city": "Cairo",
    "phoneNumber": "+201234567890",
    "firstLanguageId": 1,
    "firstLanguage": "العربية",
    "firstLanguageProficiency": "Native",
    "secondLanguageId": 2,
    "secondLanguage": "الإنجليزية",
    "secondLanguageProficiency": "Advanced"
  }
}
```

#### Error Response (404 Not Found)

```json
{
  "success": false,
  "message": "Personal information not found"
}
```

**Note:** Returns 404 if user hasn't completed Step 1 yet.

---

### 12. Get Wizard Status

**Endpoint:** `GET /api/profile/wizard-status`  
**Authentication:** Required (JWT token)  
**Description:** Get profile completion wizard status

#### Headers

```
Authorization: Bearer {token}
```

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": {
    "currentStep": 1,
    "isComplete": false,
    "stepName": "Personal Information",
    "completedSteps": ["Personal Information"]
  }
}
```

**Field Descriptions:**
- `currentStep`: Integer 0-6 indicating progress
- `isComplete`: true when step >= 6
- `stepName`: Human-readable name of current step
- `completedSteps`: Array of completed step names

---

### 13. Get Job Titles

**Endpoint:** `GET /api/profile/job-titles`  
**Authentication:** Not required  
**Description:** Get list of all available job titles for selection

#### Success Response (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "title": "Software Engineer",
      "category": "Technology"
    },
    {
      "id": 2,
      "title": "Backend Developer",
      "category": "Technology"
    },
    {
      "id": 15,
      "title": "UI/UX Designer",
      "category": "Design"
    }
  ]
}
```

**Categories Available:**
- Technology (17 titles)
- Design (8 titles)
- Marketing (11 titles)
- Sales (10 titles)
- Finance (11 titles)
- HR (10 titles)
- Operations (13 titles)
- Executive (10 titles)

**Note:** Results are sorted by category, then by title alphabetically.

---

### 14. Get Countries

**Endpoint:** `GET /api/locations/countries`  
**Authentication:** Not required  
**Description:** Get list of all countries with localized names

#### Query Parameters

- `lang` (optional): Language code ("en" or "ar", default: "en")

**Examples:**
- `GET /api/locations/countries` - English names
- `GET /api/locations/countries?lang=ar` - Arabic names

#### Success Response (200 OK) - English

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "isoCode": "EG",
      "name": "Egypt",
      "phoneCode": "+20"
    },
    {
      "id": 2,
      "isoCode": "SA",
      "name": "Saudi Arabia",
      "phoneCode": "+966"
    }
  ]
}
```

#### Success Response (200 OK) - Arabic (`?lang=ar`)

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "isoCode": "EG",
      "name": "مصر",
      "phoneCode": "+20"
    },
    {
      "id": 2,
      "isoCode": "SA",
      "name": "المملكة العربية السعودية",
      "phoneCode": "+966"
    }
  ]
}
```

**Data Coverage:**
- 65 countries total
- Arab countries prioritized (displayed first)
- Major international countries included
- ISO 3166-1 alpha-2 codes

---

### 15. Get Languages

**Endpoint:** `GET /api/locations/languages`  
**Authentication:** Not required  
**Description:** Get list of all languages with localized names

#### Query Parameters

- `lang` (optional): Language code ("en" or "ar", default: "en")

**Examples:**
- `GET /api/locations/languages` - English names
- `GET /api/locations/languages?lang=ar` - Arabic names

#### Success Response (200 OK) - English

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "isoCode": "ara",
      "name": "Arabic"
    },
    {
      "id": 2,
      "isoCode": "eng",
      "name": "English"
    },
    {
      "id": 3,
      "isoCode": "tur",
      "name": "Turkish"
    }
  ]
}
```

#### Success Response (200 OK) - Arabic (`?lang=ar`)

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "isoCode": "ara",
      "name": "العربية"
    },
    {
      "id": 2,
      "isoCode": "eng",
      "name": "الإنجليزية"
    },
    {
      "id": 3,
      "isoCode": "tur",
      "name": "التركية"
    }
  ]
}
```

**Data Coverage:**
- 50 languages total
- Arabic and English prioritized (displayed first)
- Middle Eastern, European, Asian, and African languages
- ISO 639-3 codes

---

## Error Handling

### Standard Error Format

```json
{
  "success": false,
  "message": "Human-readable error message"
}
```

### Validation Errors (400 Bad Request)

For invalid input, ASP.NET Core returns validation errors in this format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is required."],
    "Password": ["The Password field is required."]
  }
}
```

### Common Error Messages

| Endpoint | Error Message | Cause |
|----------|---------------|-------|
| Register | "An account with this email already exists..." | Duplicate email |
| Register | "Invalid email format. Please provide a valid email address" | Invalid email format |
| Verify Email | "Invalid verification code..." | Wrong code |
| Verify Email | "Verification code has expired..." | Code older than 15min |
| Login | "Invalid email or password." | Wrong credentials |
| Login | "Your email address isn't verified yet..." | Unverified email |
| Reset Password | "Invalid or expired reset token..." | Token invalid/expired |

---

## Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 OK | Success | All successful operations |
| 400 Bad Request | Client Error | Validation errors, business logic errors |
| 401 Unauthorized | Authentication Required | Missing/invalid token, login failed |
| 500 Internal Server Error | Server Error | Unexpected server errors |

**Note:** `forgot-password` always returns 200 OK (even if email doesn't exist) to prevent email enumeration.

---

## Security Measures

**Account Lockout:**
- 5 failed login attempts = 15 minute lockout
- Password reset clears lockout

**Note:** Rate limiting is not currently implemented. Account lockout provides sufficient protection for authentication endpoints.

---

## CORS Configuration

The API accepts requests from these origins:
- `http://localhost:3000` (React)
- `http://localhost:4200` (Angular)
- `http://localhost:5173` (Vite)
- `http://localhost:8080` (Vue)

**Methods Allowed:** GET, POST, PUT, DELETE, OPTIONS  
**Headers Allowed:** All  
**Credentials:** Allowed

---

## Testing Endpoints

### Swagger UI
Access interactive API documentation at:
```
http://localhost:5217/swagger
```

### Using cURL

**Register:**
```bash
curl -X POST http://localhost:5217/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!",
    "accountType": "JobSeeker"
  }'
```

**Login:**
```bash
curl -X POST http://localhost:5217/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePass123!"
  }'
```

**Protected Endpoint:**
```bash
curl -X GET http://localhost:5217/api/auth/me \
  -H "Authorization: Bearer {your-token}"
```

---

## Data Types

### AccountType Enum
```typescript
"JobSeeker" | "Recruiter"
```
Case-insensitive on input, always returned in PascalCase.

### Email Normalization
All emails are normalized:
- Trimmed (whitespace removed)
- Converted to lowercase
- Stored in lowercase format

### Password Requirements
- **Length:** 8-100 characters
- **Uppercase:** At least one letter (A-Z)
- **Lowercase:** At least one letter (a-z)
- **Digit:** At least one number (0-9)
- **Format:** No special characters required

---

## Security Features

### Password Security
- Passwords hashed with BCrypt
- Never returned in responses
- Never logged

### Token Security
- JWT tokens signed with HS256
- Token expiry: 24 hours (auth), 5 minutes (reset)
- Single-use reset tokens
- Token validation on all protected endpoints

### OTP Security
- Cryptographically secure random generation
- 6-digit numeric codes
- Expiry: 15 minutes (verification), 15 minutes (reset)
- Single-use after successful verification

### Email Security
- Email verification required before login
- Email enumeration prevention (forgot-password always returns success)
- Email case normalization

---

**Last Updated:** December 2025  
**API Version:** 1.1.0

