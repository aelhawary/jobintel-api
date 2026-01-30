# Authentication API - Integration Guide

**Last Updated:** December 2025  
**API Version:** 1.1.0  
**Base URL (Development):** http://localhost:5217/api/auth

---

## Overview

This document provides comprehensive integration guidance for the RecruitmentPlatformAPI authentication system, including email/password authentication and Google OAuth 2.0.

## Authentication Methods

1. **Email/Password Authentication** - Traditional registration with email verification
2. **Google OAuth 2.0** - One-click sign-in with Google accounts

## JWT Bearer Tokens

- After successful login (email or Google), the backend returns a JWT in the `token` field
- Token expiry: **24 hours**
- Send JWT in protected endpoint requests:
  ```
  Authorization: Bearer {token}
  ```

## CORS Configuration

**Development:** Allows localhost origins (3000, 4200, 5173, 8080) and null origin for local file testing  
**Production:** Configure specific frontend domain origins only

## Response Format

### Success Response
```json
{
  "success": true,
  "message": "Operation successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",  // optional
  "user": {                                              // optional
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "accountType": "JobSeeker",
    "isEmailVerified": true,
    "isActive": true
  }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error message explaining the issue"
}
```

---

## Endpoints

### 1) Register (Email / Password)
- POST `/register`
- Body (JSON):
```
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123",
  "accountType": "JobSeeker"   // or "Recruiter"
}
```

**Password Requirements:**
- Length: 8-100 characters
- Must contain at least one uppercase letter (A-Z)
- Must contain at least one lowercase letter (a-z)
- Must contain at least one digit (0-9)

- Success (200):
```
{
  "success": true,
  "message": "Registration successful. Please check your email to verify your account.",
  "user": {
    "id": 10,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "accountType": "JobSeeker",
    "isEmailVerified": false
  }
}
```
- Errors: validation errors (400) or `An account with this email already exists`

Curl example
```bash
curl -X POST http://localhost:5217/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Doe","email":"john.doe@example.com","password":"SecurePass123","confirmPassword":"SecurePass123","accountType":"JobSeeker"}'
```

---

### 2) Login (Email)
- POST `/login`
- Body (JSON):
```
{
  "email": "john.doe@example.com",
  "password": "SecurePass123"
}
```
- Success (200):
```
{
  "success": true,
  "message": "Login successful.",
  "token": "<jwt-token>",
  "user": { ... }
}
```
- Errors: `Invalid email or password.` or `This account uses Google sign-in...`

Curl example
```bash
curl -X POST http://localhost:5217/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"john.doe@example.com","password":"SecurePass123"}'
```

---

### 3) Google OAuth Sign-in / Register
- POST `/google`
- **Description:** Authenticate with Google account. Creates new account if email doesn't exist, or logs in existing user.
- **Body (JSON):**
```json
{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjE4MmU0...",
  "accountType": "JobSeeker"   // or "Recruiter"
}
```

**Field Requirements:**
- `idToken` (required): Google ID token from frontend (obtained via Google Identity Services)
- `accountType` (required): Either "JobSeeker" or "Recruiter" (case-insensitive)

**How It Works:**
1. Frontend obtains Google ID token using Google Sign-In library
2. Frontend sends `idToken` + `accountType` to this endpoint
3. Backend validates token with Google's servers (server-side verification)
4. If email exists: user is logged in
5. If email is new: account is created with Google profile info
6. Returns JWT token + user info (same as email login)

**Success Response (Existing User):**
```json
{
  "success": true,
  "message": "Login successful.",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 5,
    "firstName": "Ahmed",
    "lastName": "Mohamed",
    "email": "ahmed@gmail.com",
    "accountType": "JobSeeker",
    "isEmailVerified": true,
    "isActive": true
  }
}
```

**Success Response (New User Created):**
```json
{
  "success": true,
  "message": "Account created successfully. Welcome!",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 7,
    "firstName": "Sara",
    "lastName": "Ali",
    "email": "sara.ali@gmail.com",
    "accountType": "Recruiter",
    "isEmailVerified": true,
    "isActive": true
  }
}
```

**Error Responses:**
- **Invalid Token (400):**
  ```json
  { "success": false, "message": "Invalid Google token. Please try again." }
  ```
- **Unverified Google Email (400):**
  ```json
  { "success": false, "message": "Your Google email is not verified. Please verify your email with Google first." }
  ```
- **Deactivated Account (400):**
  ```json
  { "success": false, "message": "Your account is deactivated. Please contact support." }
  ```

**Frontend Implementation Notes:**
- Install `@react-oauth/google` or use Google Identity Services
- Obtain ID token from Google Sign-In
- Send token to backend with selected account type
- Store returned JWT token for subsequent requests
- **No email verification needed** - Google users are auto-verified
- Profile pictures are automatically imported from Google

**Security:**
- Backend validates token against configured Google Client ID
- Server-side verification prevents token spoofing
- Only verified Google emails accepted
- See [GOOGLE_AUTH_GUIDE.md](./GOOGLE_AUTH_GUIDE.md) for detailed integration guide

Curl example:
```bash
curl -X POST http://localhost:5217/api/auth/google \
  -H "Content-Type: application/json" \
  -d '{"idToken":"<google_id_token>","accountType":"JobSeeker"}'
```

---

### 4) Verify Email
- POST `/verify-email`
- **Description:** Verify email address with 6-digit OTP code. **Does NOT return JWT token** - users must login after verification.
- Body (JSON):
```
{ "email": "john.doe@example.com", "verificationCode": "123456" }
```
- Success Response (200):
```json
{
  "success": true,
  "message": "Email verified successfully! Please log in to continue.",
  "user": {
    "id": 10,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "accountType": "JobSeeker",
    "isEmailVerified": true,
    "isActive": true
  }
}
```
- **Next Step:** Use the login endpoint (`POST /login`) to receive your JWT token.

---

### 5) Resend Verification
- POST `/resend-verification`
- Body: `{ "email": "..." }`

---

### 6) Forgot Password (request OTP)
- POST `/forgot-password`
- Body: `{ "email": "..." }`
- Returns 200 OK even if email not found (prevents enumeration)

---

### 7) Verify Reset OTP
- POST `/verify-reset-otp`
- Body: `{ "email": "...", "otpCode": "123456" }`
- Success returns a short-lived `resetToken` used to reset the password.

---

### 8) Reset Password
- POST `/reset-password`
- Body:
```
{
  "email": "john.doe@example.com",
  "resetToken": "<token-from-verify-reset-otp>",
  "newPassword": "NewSecurePass123",
  "confirmPassword": "NewSecurePass123"
}
```

**Password Requirements:** Same as registration (8-100 chars, uppercase, lowercase, digit)

---

### 9) Get Current User
- GET `/me`
- Requires Authorization header: `Bearer {token}`
- Returns user id, email, name, role, firstName, lastName.

Curl example
```bash
curl -H "Authorization: Bearer <jwt>" http://localhost:5217/api/auth/me
```

---

---

## Frontend Integration Quick Start

### Email/Password Authentication Flow
```
1. Register → 2. Verify Email (OTP) → 3. Login → 4. Access Protected Resources
```

**Important:** After email verification, users must explicitly login to receive a JWT token. This ensures proper session management and security.

### Google OAuth Authentication Flow
```
1. Click "Sign in with Google" → 2. Select Google Account → 3. Instant Login/Register → 4. Access Protected Resources
```

### Key Implementation Points

**Email/Password Authentication:**
- Email verification is required but does NOT authenticate the user
- After successful verification, redirect user to login page
- Login endpoint returns the JWT token needed for protected resources
- Implement proper state management to handle verification → login flow

**Google Sign-In:**
- Install `@react-oauth/google` package
- Configure Google OAuth Provider with your Client ID
- Use returned `credential` as `idToken` in API request
- Always send `accountType` (JobSeeker or Recruiter)
- No email verification step needed for Google users
- JWT token returned immediately after Google authentication

**Token Management:**
- Store JWT token securely (localStorage for dev, httpOnly cookies for production)
- Include token in Authorization header: `Bearer {token}`
- Tokens expire after 24 hours
- Implement automatic logout on 401 responses

**User Flows:**
- **Email Registration:** Register → Check Email → Verify OTP → Login → Profile Completion
- **Google Registration:** Sign in with Google → Profile Completion (instant authentication)
- **Email Login:** Login → Dashboard
- **Google Login:** Sign in with Google → Dashboard

---

## Security & Production Notes

### For Production Deployment:

**Environment Variables:**
- Move `JwtSettings.SecretKey` to secure environment variable
- Move email credentials to secret manager (Azure Key Vault, AWS Secrets Manager)
- Configure Google OAuth Client ID via environment variable

**CORS:**
- Remove permissive localhost origins
- Configure specific production domain origins only
- Remove null origin support (used for local file:// testing)

**Security Enhancements:**
- Account lockout is already implemented for failed login attempts
- Enable comprehensive logging and monitoring
- Set up alerts for suspicious activity
- Use HTTPS only in production

**Best Practices:**
- Never trust frontend tokens - always validate server-side
- Keep JWT secret keys secure and rotate regularly
- Monitor authentication attempts and failures

---

## Additional Resources

- **[API Reference](./API_REFERENCE.md)** - Complete endpoint documentation with examples
- **[Google Auth Guide](./GOOGLE_AUTH_GUIDE.md)** - Detailed Google OAuth integration guide
- **[React Integration Guide](./REACT_INTEGRATION_GUIDE.md)** - React/TypeScript integration examples
- **[Setup Guide](./SETUP_GUIDE.md)** - Backend configuration and setup

---

**Document Version:** 1.1.0  
**Last Updated:** December 2025  
**Status:** ✅ Ready for Integration