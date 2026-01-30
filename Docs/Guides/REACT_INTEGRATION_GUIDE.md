# React Frontend Integration Guide

**API Version:** 1.1.0  
**Last Updated:** December 2025  
**Status:** ✅ Ready for Integration

---

## ⚠️ Important Notes

### Registration Flow Changes
The registration endpoint has been simplified:
- ✅ **Removed:** Phone number and company name fields from registration
- ✅ **Reason:** These fields will be collected during the profile completion wizard (after email verification)
- ✅ **Registration now requires:** First name, Last name, Email, Password, Confirm password, Account type only
- ✅ **Next steps:** After successful email verification, users will be redirected to profile completion

### Email Validation
Strong email validation is now enforced:
- ✅ Format must be valid (e.g., `user@domain.com`)
- ❌ Invalid formats like `user@.com` or `@domain.com` will be rejected
- ✅ Validation uses regex pattern: `^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$`

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Project Setup](#project-setup)
3. [API Configuration](#api-configuration)
4. [Authentication Flow](#authentication-flow)
5. [Security Features Overview](#security-features-overview)
6. [API Service Implementation](#api-service-implementation)
7. [React Components Examples](#react-components-examples)
8. [Error Handling](#error-handling)
9. [Best Practices](#best-practices)
10. [Complete Integration Examples](#complete-integration-examples)
11. [Troubleshooting](#troubleshooting)

---

## Quick Start

### Prerequisites
- Node.js 16+ and npm/yarn
- React 18+
- TypeScript (recommended)
- Backend API running on `http://localhost:5217`

### Installation

```bash
# Create React app with TypeScript
npx create-react-app jobintel-frontend --template typescript
cd jobintel-frontend

# Install dependencies
npm install axios
```

---

## Project Setup

### 1. API Configuration File

Create `src/config/api.ts`:

```typescript
export const API_CONFIG = {
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5217',
  timeout: 10000,
};

export const API_ENDPOINTS = {
  auth: {
    register: '/api/auth/register',
    login: '/api/auth/login',
    verifyEmail: '/api/auth/verify-email',
    resendVerification: '/api/auth/resend-verification',
    forgotPassword: '/api/auth/forgot-password',
    verifyResetOtp: '/api/auth/verify-reset-otp',
    resetPassword: '/api/auth/reset-password',
    me: '/api/auth/me',
  },
};
```

### 2. Environment Variables

Create `.env` file:

```env
REACT_APP_API_URL=http://localhost:5217
```

---

## API Configuration

### CORS Setup

The backend is already configured for React on port 3000. No additional CORS setup needed.

**Allowed Origins:**
- `http://localhost:3000` (React)
- `http://localhost:4200` (Angular)
- `http://localhost:5173` (Vite)
- `http://localhost:8080` (Vue)

---

## Security Features Overview

The backend API implements multiple security layers that your frontend must handle:

### 🔐 Password Policy
- **Minimum length:** 8 characters
- **Maximum length:** 100 characters
- **Required:** At least one uppercase letter (A-Z)
- **Required:** At least one lowercase letter (a-z)
- **Required:** At least one digit (0-9)
- **Examples:** `SecurePass123`, `Welcome2024`, `MyPassword1`

###  Account Lockout
After 5 failed login attempts, accounts lock for 15 minutes:
- **Lockout response** includes `lockoutEnd` and `remainingMinutes`
- **UI must show** countdown timer and unlock options
- **Password reset** immediately unlocks the account
- **Email notification** sent to user when locked

### ⏱️ Token Expiry
- **Auth tokens:** 24 hours (for normal sessions)
- **Reset tokens:** 5 minutes (for password reset flow)
- **OTP codes:** 15 minutes (for email verification and password reset)

### ✉️ Email Verification
- Required before first login
- 6-digit OTP codes
- Single-use codes
- Resend limited to 3/hour
- **Users must login after verification** (no automatic token issuance)

### 🔄 Password Reset Flow
1. Request OTP (3 requests/hour limit)
2. Verify OTP (10 attempts/hour, 15min expiry)
3. Reset password with token (5min token expiry)

---

## Authentication Flow

### Complete User Journey

```
1. Register → 2. Verify Email → 3. Login → 4. Access Protected Routes
```

**Key Points:**
- After registration, users receive a verification email
- Email verification succeeds but does NOT return a JWT token
- Users must then login with their credentials to receive the token
- This ensures proper session management and security

**Alternative:**
```
Forgot Password → Verify OTP → Reset Password → Login
```

---

## API Service Implementation

### 1. Create API Client

Create `src/services/apiClient.ts`:

```typescript
import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';
import { API_CONFIG } from '../config/api';

class ApiClient {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: API_CONFIG.baseURL,
      timeout: API_CONFIG.timeout,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor to add auth token
    this.client.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor for error handling
    this.client.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Token expired or invalid
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  async get<T>(url: string, config?: AxiosRequestConfig) {
    const response = await this.client.get<T>(url, config);
    return response.data;
  }

  async post<T>(url: string, data?: any, config?: AxiosRequestConfig) {
    const response = await this.client.post<T>(url, data, config);
    return response.data;
  }

  async put<T>(url: string, data?: any, config?: AxiosRequestConfig) {
    const response = await this.client.put<T>(url, data, config);
    return response.data;
  }

  async delete<T>(url: string, config?: AxiosRequestConfig) {
    const response = await this.client.delete<T>(url, config);
    return response.data;
  }
}

export const apiClient = new ApiClient();
```

### 2. JSON Property Naming

The backend is configured to return camelCase JSON property names (`success`, `message`, `token`), which is standard for REST APIs and works seamlessly with React/TypeScript.

### 3. Create Types

Create `src/types/auth.ts`:

```typescript
export enum AccountType {
  JobSeeker = 'JobSeeker',
  Recruiter = 'Recruiter',
}

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  accountType: AccountType;
  isEmailVerified: boolean;
  isActive: boolean;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  resetToken?: string;
  user?: User;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  accountType: string; // "JobSeeker" or "Recruiter"
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface EmailVerificationRequest {
  email: string;
  verificationCode: string;
}

export interface ResendVerificationRequest {
  email: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface VerifyResetOtpRequest {
  email: string;
  otpCode: string;
}

export interface ResetPasswordRequest {
  email: string;
  resetToken: string;
  newPassword: string;
  confirmNewPassword: string;
}
```

### 3. Create Auth Service

Create `src/services/authService.ts`:

```typescript
import { apiClient } from './apiClient';
import { API_ENDPOINTS } from '../config/api';
import type {
  AuthResponse,
  RegisterRequest,
  LoginRequest,
  EmailVerificationRequest,
  ResendVerificationRequest,
  ForgotPasswordRequest,
  VerifyResetOtpRequest,
  ResetPasswordRequest,
  User,
} from '../types/auth';

export const authService = {
  /**
   * Register a new user
   */
  async register(data: RegisterRequest): Promise<AuthResponse> {
    return apiClient.post<AuthResponse>(API_ENDPOINTS.auth.register, data);
  },

  /**
   * Login user
   * Note: Email must be verified before login
   */
  async login(data: LoginRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>(API_ENDPOINTS.auth.login, data);
    
    // Store token and user info on successful login
    if (response.success && response.token && response.user) {
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
    }
    
    return response;
  },

  /**
   * Verify email with OTP code
   */
  async verifyEmail(data: EmailVerificationRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>(API_ENDPOINTS.auth.verifyEmail, data);
    
    // Store token and user info on successful verification
    if (response.success && response.token && response.user) {
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(response.user));
    }
    
    return response;
  },

  /**
   * Resend verification code
   */
  async resendVerification(data: ResendVerificationRequest): Promise<AuthResponse> {
    return apiClient.post<AuthResponse>(API_ENDPOINTS.auth.resendVerification, data);
  },

  /**
   * Request password reset OTP
   */
  async forgotPassword(data: ForgotPasswordRequest): Promise<AuthResponse> {
    return apiClient.post<AuthResponse>(API_ENDPOINTS.auth.forgotPassword, data);
  },

  /**
   * Verify password reset OTP and get reset token
   */
  async verifyResetOtp(data: VerifyResetOtpRequest): Promise<AuthResponse> {
    return apiClient.post<AuthResponse>(API_ENDPOINTS.auth.verifyResetOtp, data);
  },

  /**
   * Reset password with token
   */
  async resetPassword(data: ResetPasswordRequest): Promise<AuthResponse> {
    return apiClient.post<AuthResponse>(API_ENDPOINTS.auth.resetPassword, data);
  },

  /**
   * Get current user information (requires authentication)
   */
  async getCurrentUser(): Promise<{ success: boolean; user: User }> {
    return apiClient.get<{ success: boolean; user: User }>(API_ENDPOINTS.auth.me);
  },

  /**
   * Logout user (clear local storage)
   */
  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return !!localStorage.getItem('token');
  },

  /**
   * Get stored user info
   */
  getStoredUser(): User | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  /**
   * Get stored token
   */
  getToken(): string | null {
    return localStorage.getItem('token');
  },
};
```

---

## React Components Examples

### 1. Registration Component

Create `src/components/Register.tsx`:

```typescript
import React, { useState } from 'react';
import { authService } from '../services/authService';
import { AccountType } from '../types/auth';

const Register: React.FC = () => {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    accountType: AccountType.JobSeeker,
  });

  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      const response = await authService.register(formData);

      if (response.success) {
        setSuccess('Registration successful! Please check your email for verification code.');
        // Redirect to verification page
        setTimeout(() => {
          window.location.href = `/verify-email?email=${encodeURIComponent(formData.email)}`;
        }, 2000);
      } else {
        setError(response.message);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="register-container">
      <h2>Create Account</h2>
      
      <form onSubmit={handleSubmit}>
        <div>
          <label>Account Type</label>
          <select
            value={formData.accountType}
            onChange={(e) => setFormData({ ...formData, accountType: e.target.value as AccountType })}
            required
          >
            <option value={AccountType.JobSeeker}>Job Seeker</option>
            <option value={AccountType.Recruiter}>Recruiter</option>
          </select>
        </div>

        <div>
          <label>First Name</label>
          <input
            type="text"
            value={formData.firstName}
            onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
            required
            maxLength={50}
          />
        </div>

        <div>
          <label>Last Name</label>
          <input
            type="text"
            value={formData.lastName}
            onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
            required
            maxLength={50}
          />
        </div>

        <div>
          <label>Email</label>
          <input
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            required
          />
        </div>

        <div>
          <label>Password</label>
          <input
            type="password"
            value={formData.password}
            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            required
            minLength={8}
          />
          <small>Minimum 8 characters</small>
        </div>

        <div>
          <label>Confirm Password</label>
          <input
            type="password"
            value={formData.confirmPassword}
            onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
            required
          />
        </div>

        {error && <div className="error">{error}</div>}
        {success && <div className="success">{success}</div>}

        <button type="submit" disabled={loading}>
          {loading ? 'Registering...' : 'Register'}
        </button>
      </form>
    </div>
  );
};

export default Register;
```

### 2. Email Verification Component

Create `src/components/VerifyEmail.tsx`:

```typescript
import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';

const VerifyEmail: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const email = searchParams.get('email') || '';

  const [code, setCode] = useState(['', '', '', '', '', '']);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [resending, setResending] = useState(false);

  // Handle OTP input with auto-focus
  const handleCodeChange = (index: number, value: string) => {
    if (value.length > 1) return; // Only single digit
    if (!/^\d*$/.test(value)) return; // Only numbers

    const newCode = [...code];
    newCode[index] = value;
    setCode(newCode);

    // Auto-focus next input
    if (value && index < 5) {
      const nextInput = document.getElementById(`code-${index + 1}`);
      nextInput?.focus();
    }

    // Auto-submit when all 6 digits entered
    if (index === 5 && value && newCode.every(digit => digit !== '')) {
      handleVerify(newCode.join(''));
    }
  };

  const handleVerify = async (verificationCode?: string) => {
    const codeToVerify = verificationCode || code.join('');
    
    if (codeToVerify.length !== 6) {
      setError('Please enter 6-digit verification code');
      return;
    }

    setError('');
    setLoading(true);

    try {
      const response = await authService.verifyEmail({
        email,
        verificationCode: codeToVerify,
      });

      if (response.success) {
        // Show success message and redirect to login
        alert('Email verified successfully! Please log in to continue.');
        navigate('/login', { state: { email } }); // Pass email to pre-fill login form
      } else {
        setError(response.message);
        // Clear code on error
        setCode(['', '', '', '', '', '']);
        document.getElementById('code-0')?.focus();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Verification failed. Please try again.');
      setCode(['', '', '', '', '', '']);
      document.getElementById('code-0')?.focus();
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async () => {
    setResending(true);
    setError('');

    try {
      const response = await authService.resendVerification({ email });
      if (response.success) {
        alert('Verification code resent! Please check your email.');
      } else {
        setError(response.message);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to resend code.');
    } finally {
      setResending(false);
    }
  };

  return (
    <div className="verify-email-container">
      <h2>Verify Your Email</h2>
      <p>We've sent a 6-digit verification code to <strong>{email}</strong></p>

      <div className="otp-input-container">
        {code.map((digit, index) => (
          <input
            key={index}
            id={`code-${index}`}
            type="text"
            maxLength={1}
            value={digit}
            onChange={(e) => handleCodeChange(index, e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Backspace' && !digit && index > 0) {
                const prevInput = document.getElementById(`code-${index - 1}`);
                prevInput?.focus();
              }
            }}
            className="otp-input"
            disabled={loading}
          />
        ))}
      </div>

      {error && <div className="error">{error}</div>}

      <button
        onClick={() => handleVerify()}
        disabled={loading || code.some(digit => !digit)}
        className="verify-button"
      >
        {loading ? 'Verifying...' : 'Verify Email'}
      </button>

      <div className="resend-section">
        <p>Didn't receive the code?</p>
        <button
          onClick={handleResend}
          disabled={resending}
          className="resend-button"
        >
          {resending ? 'Sending...' : 'Resend Code'}
        </button>
      </div>
    </div>
  );
};

export default VerifyEmail;
```

### 3. Login Component

Create `src/components/Login.tsx`:

```typescript
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';

const Login: React.FC = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    email: '',
    password: '',
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await authService.login(formData);

      if (response.success) {
        // Redirect based on account type or to dashboard
        const user = response.user;
        if (user?.accountType === 'JobSeeker') {
          navigate('/jobseeker/dashboard');
        } else {
          navigate('/recruiter/dashboard');
        }
      } else {
        setError(response.message);
        
        // If email not verified, offer to resend verification
        if (response.message.includes('verified')) {
          // Show option to resend verification code
        }
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <h2>Login</h2>
      
      <form onSubmit={handleSubmit}>
        <div>
          <label>Email</label>
          <input
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            required
          />
        </div>

        <div>
          <label>Password</label>
          <input
            type="password"
            value={formData.password}
            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
            required
          />
        </div>

        {error && <div className="error">{error}</div>}

        <button type="submit" disabled={loading}>
          {loading ? 'Logging in...' : 'Login'}
        </button>

        <div className="login-links">
          <a href="/forgot-password">Forgot Password?</a>
          <a href="/register">Don't have an account? Register</a>
        </div>
      </form>
    </div>
  );
};

export default Login;
```

### 4. Protected Route Component

Create `src/components/ProtectedRoute.tsx`:

```typescript
import React from 'react';
import { Navigate } from 'react-router-dom';
import { authService } from '../services/authService';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  if (!authService.isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};

export default ProtectedRoute;
```

### 5. Password Reset Flow

Create `src/components/ForgotPassword.tsx`:

```typescript
import React, { useState } from 'react';
import { authService } from '../services/authService';

const ForgotPassword: React.FC = () => {
  const [email, setEmail] = useState('');
  const [step, setStep] = useState<'request' | 'verify-otp' | 'reset'>('request');
  const [otpCode, setOtpCode] = useState('');
  const [resetToken, setResetToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const handleRequestOtp = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await authService.forgotPassword({ email });
      if (response.success) {
        setSuccess('If your email is registered, you will receive a password reset code shortly.');
        setStep('verify-otp');
      } else {
        setError(response.message);
      }
    } catch (err: any) {
      // Always show success message (security: prevents email enumeration)
      setSuccess('If your email is registered, you will receive a password reset code shortly.');
      setStep('verify-otp');
    } finally {
      setLoading(false);
    }
  };

  const handleVerifyOtp = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    
    if (otpCode.length !== 6) {
      setError('Please enter 6-digit OTP code');
      return;
    }

    setLoading(true);

    try {
      const response = await authService.verifyResetOtp({
        email,
        otpCode,
      });

      if (response.success && response.resetToken) {
        setResetToken(response.resetToken);
        setStep('reset');
      } else {
        setError(response.message);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Invalid OTP code. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    // Password validation matching backend policy
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;
    
    if (!passwordRegex.test(newPassword)) {
      setError('Password must be 8+ characters with uppercase, lowercase, and digit');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    setLoading(true);

    try {
      const response = await authService.resetPassword({
        email,
        resetToken,
        newPassword,
        confirmNewPassword: confirmPassword,
      });

      if (response.success) {
        setSuccess('Password reset successful! Redirecting to login...');
        setTimeout(() => {
          window.location.href = '/login';
        }, 2000);
      } else {
        setError(response.message);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Password reset failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="forgot-password-container">
      <h2>Reset Password</h2>

      {step === 'request' && (
        <form onSubmit={handleRequestOtp}>
          <p>Enter your email address to receive a password reset code.</p>
          <div>
            <label>Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          {error && <div className="error">{error}</div>}
          {success && <div className="success">{success}</div>}
          <button type="submit" disabled={loading}>
            {loading ? 'Sending...' : 'Send Reset Code'}
          </button>
        </form>
      )}

      {step === 'verify-otp' && (
        <form onSubmit={handleVerifyOtp}>
          <p>Enter the 6-digit code sent to {email}</p>
          <div>
            <label>OTP Code</label>
            <input
              type="text"
              value={otpCode}
              onChange={(e) => {
                const value = e.target.value.replace(/\D/g, '').slice(0, 6);
                setOtpCode(value);
              }}
              maxLength={6}
              required
              placeholder="123456"
            />
          </div>
          {error && <div className="error">{error}</div>}
          <button type="submit" disabled={loading || otpCode.length !== 6}>
            {loading ? 'Verifying...' : 'Verify Code'}
          </button>
          <button type="button" onClick={() => setStep('request')}>
            Back
          </button>
        </form>
      )}

      {step === 'reset' && (
        <form onSubmit={handleResetPassword}>
          <p>Enter your new password</p>
          <div>
            <label>New Password</label>
            <input
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
              minLength={8}
            />
            <small>Minimum 8 characters</small>
          </div>
          <div>
            <label>Confirm New Password</label>
            <input
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />
          </div>
          {error && <div className="error">{error}</div>}
          {success && <div className="success">{success}</div>}
          <button type="submit" disabled={loading}>
            {loading ? 'Resetting...' : 'Reset Password'}
          </button>
        </form>
      )}
    </div>
  );
};

export default ForgotPassword;
```

### 6. Custom Hook for Authentication

Create `src/hooks/useAuth.ts`:

```typescript
import { useState, useEffect } from 'react';
import { authService } from '../services/authService';
import { User } from '../types/auth';

export const useAuth = () => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initAuth = async () => {
      // Check if user is authenticated
      if (authService.isAuthenticated()) {
        // Try to get user from API
        try {
          const response = await authService.getCurrentUser();
          if (response.success && response.user) {
            setUser(response.user);
          } else {
            // Token might be invalid, clear auth
            authService.logout();
          }
        } catch {
          // Token invalid, clear auth
          authService.logout();
        }
      } else {
        // Try to get user from local storage
        const storedUser = authService.getStoredUser();
        if (storedUser) {
          setUser(storedUser);
        }
      }
      setLoading(false);
    };

    initAuth();
  }, []);

  const login = async (email: string, password: string) => {
    const response = await authService.login({ email, password });
    if (response.success && response.user) {
      setUser(response.user);
    }
    return response;
  };

  const logout = () => {
    authService.logout();
    setUser(null);
  };

  return {
    user,
    isAuthenticated: !!user,
    loading,
    login,
    logout,
  };
};
```

---

## Error Handling

### Error Response Format

All API endpoints return errors in this format:

```typescript
{
  success: false,
  message: "Error message here"
}
```

### HTTP Status Codes

- `200 OK` - Success (even for forgot-password to prevent enumeration)
- `400 Bad Request` - Validation errors or business logic errors
- `401 Unauthorized` - Authentication required or failed
- `500 Internal Server Error` - Server errors

### Error Handling Best Practices

```typescript
try {
  const response = await authService.register(data);
  if (!response.success) {
    // Handle business logic error
    setError(response.message);
    return;
  }
  // Handle success
} catch (error: any) {
  // Handle network or server errors
  if (error.response) {
    // Server responded with error
    setError(error.response.data?.message || 'An error occurred');
  } else if (error.request) {
    // Request made but no response
    setError('Network error. Please check your connection.');
  } else {
    // Something else happened
    setError('An unexpected error occurred.');
  }
}
```

---

## Best Practices

### 1. Token Storage
- ✅ Store JWT token in `localStorage` (acceptable for this project)
- ✅ Store user info in `localStorage` for quick access
- ✅ Always validate token with API call on app start

### 2. Token Expiry
- JWT tokens expire in **24 hours**
- Implement token refresh logic if needed
- Redirect to login on 401 errors

### 3. Email Verification
- Users **must** verify email before login
- Show clear message if login fails due to unverified email
- Provide "Resend verification" option

### 4. Form Validation
- Client-side validation for better UX
- Server-side validation is the source of truth
- Show server error messages to user

**Password Policy (Client-Side Validation):**
```typescript
const validatePassword = (password: string): string | null => {
  if (password.length < 8 || password.length > 100) {
    return 'Password must be between 8 and 100 characters';
  }
  if (!/[a-z]/.test(password)) {
    return 'Password must contain at least one lowercase letter';
  }
  if (!/[A-Z]/.test(password)) {
    return 'Password must contain at least one uppercase letter';
  }
  if (!/\d/.test(password)) {
    return 'Password must contain at least one digit';
  }
  return null; // Valid
};
```

### 5. Loading States
- Always show loading indicators during API calls
- Disable buttons during submission
- Prevent duplicate submissions

### 6. Security
- Never store passwords in state or localStorage
- Clear sensitive data on logout
- Use HTTPS in production

### 7. Account Lockout Handling

After 5 failed login attempts, accounts are locked for 15 minutes:

**Lockout Response (HTTP 401):**
```json
{
  "success": false,
  "message": "Account is locked due to multiple failed login attempts. Please try again after 15 minutes or reset your password.",
  "lockoutEnd": "2025-12-07T15:45:00Z",
  "remainingMinutes": 12
}
```

**UI Implementation:**
```typescript
interface LoginResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: User;
  lockoutEnd?: string;
  remainingMinutes?: number;
}

const handleLogin = async () => {
  try {
    const response = await authService.login(credentials);
    if (response.success) {
      // Success - store token and redirect
      localStorage.setItem('token', response.token!);
      navigate('/dashboard');
    }
  } catch (error: any) {
    const errorData = error.response?.data;
    
    if (errorData?.lockoutEnd) {
      // Account is locked
      setError(errorData.message);
      setIsLocked(true);
      setLockoutMinutes(errorData.remainingMinutes);
      
      // Optional: Show countdown timer
      startLockoutTimer(errorData.lockoutEnd);
    } else if (errorData?.message?.includes('remaining')) {
      // Show warning before lockout (e.g., "2 attempts remaining")
      setError(errorData.message);
      setWarningAttemptsRemaining(true);
    } else {
      setError(errorData?.message || 'Login failed');
    }
  }
};

// Optional: Countdown timer component
const startLockoutTimer = (lockoutEnd: string) => {
  const interval = setInterval(() => {
    const now = new Date().getTime();
    const end = new Date(lockoutEnd).getTime();
    const remaining = Math.max(0, Math.ceil((end - now) / 60000));
    
    setLockoutMinutes(remaining);
    
    if (remaining === 0) {
      clearInterval(interval);
      setIsLocked(false);
      setError('');
    }
  }, 1000);
};
```

**Lockout UI Example:**
```tsx
{isLocked && (
  <div className="alert alert-danger" role="alert">
    <strong>Account Locked</strong>
    <p>{error}</p>
    <p>Time remaining: <strong>{lockoutMinutes} minutes</strong></p>
    <button onClick={() => navigate('/forgot-password')}>
      Reset Password to Unlock
    </button>
  </div>
)}

{warningAttemptsRemaining && (
  <div className="alert alert-warning">
    <strong>Warning:</strong> {error}
  </div>
)}
```

**Important Notes:**
- ✅ Password reset **clears the lockout** immediately
- ✅ Lockout auto-expires after 15 minutes
- ✅ Show user the remaining time
- ✅ Suggest password reset as alternative
- ✅ Lockout email is sent to user's email address

### 9. Routing

Example with React Router:

```typescript
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import ProtectedRoute from './components/ProtectedRoute';
import Login from './components/Login';
import Register from './components/Register';
import VerifyEmail from './components/VerifyEmail';
import Dashboard from './components/Dashboard';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/verify-email" element={<VerifyEmail />} />
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
```

---

## Complete Example: App.tsx Structure

```typescript
import React, { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './hooks/useAuth';
import ProtectedRoute from './components/ProtectedRoute';

// Components
import Login from './components/Login';
import Register from './components/Register';
import VerifyEmail from './components/VerifyEmail';
import ForgotPassword from './components/ForgotPassword';
import Dashboard from './components/Dashboard';

function App() {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <BrowserRouter>
      <Routes>
        {/* Public Routes */}
        <Route
          path="/login"
          element={isAuthenticated ? <Navigate to="/dashboard" /> : <Login />}
        />
        <Route
          path="/register"
          element={isAuthenticated ? <Navigate to="/dashboard" /> : <Register />}
        />
        <Route path="/verify-email" element={<VerifyEmail />} />
        <Route path="/forgot-password" element={<ForgotPassword />} />

        {/* Protected Routes */}
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          }
        />

        {/* Default redirect */}
        <Route
          path="/"
          element={
            <Navigate to={isAuthenticated ? '/dashboard' : '/login'} replace />
          }
        />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
```

---

## Complete Integration Examples

### Full-Featured Login Component

```typescript
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';

interface LoginForm {
  email: string;
  password: string;
}

const Login: React.FC = () => {
  const navigate = useNavigate();
  const [form, setForm] = useState<LoginForm>({ email: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [isLocked, setIsLocked] = useState(false);
  const [lockoutMinutes, setLockoutMinutes] = useState(0);
  const [attemptsRemaining, setAttemptsRemaining] = useState<number | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await authService.login(form);
      
      if (response.success && response.token) {
        // Store token and user data
        localStorage.setItem('token', response.token);
        localStorage.setItem('user', JSON.stringify(response.user));
        
        // Redirect to dashboard
        navigate('/dashboard');
      }
    } catch (err: any) {
      const errorData = err.response?.data;
      
      // Handle account lockout
      if (errorData?.lockoutEnd) {
        setIsLocked(true);
        setLockoutMinutes(errorData.remainingMinutes || 15);
        setError(errorData.message);
        startCountdown(errorData.lockoutEnd);
      }
      // Handle remaining attempts warning
      else if (errorData?.message?.includes('remaining')) {
        const match = errorData.message.match(/(\d+)\s+attempt/);
        if (match) {
          setAttemptsRemaining(parseInt(match[1]));
        }
        setError(errorData.message);
      }
      // Handle other errors
      else {
        setError(errorData?.message || 'Login failed. Please check your credentials.');
      }
    } finally {
      setLoading(false);
    }
  };

  const startCountdown = (lockoutEnd: string) => {
    const interval = setInterval(() => {
      const now = new Date().getTime();
      const end = new Date(lockoutEnd).getTime();
      const remaining = Math.max(0, Math.ceil((end - now) / 60000));
      
      setLockoutMinutes(remaining);
      
      if (remaining === 0) {
        clearInterval(interval);
        setIsLocked(false);
        setError('');
      }
    }, 60000); // Update every minute
  };

  return (
    <div className="login-container">
      <h2>Login</h2>
      
      {/* Account Lockout Alert */}
      {isLocked && (
        <div className="alert alert-danger">
          <h4>🔒 Account Locked</h4>
          <p>{error}</p>
          <p>Time remaining: <strong>{lockoutMinutes} minutes</strong></p>
          <button
            className="btn btn-link"
            onClick={() => navigate('/forgot-password')}
          >
            Reset Password to Unlock Immediately
          </button>
        </div>
      )}

      {/* Attempts Warning */}
      {attemptsRemaining !== null && attemptsRemaining <= 2 && !isLocked && (
        <div className="alert alert-warning">
          <strong>⚠️ Warning:</strong> {error}
        </div>
      )}

      {/* General Error */}
      {error && !isLocked && attemptsRemaining === null && (
        <div className="alert alert-danger">{error}</div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input
            type="email"
            id="email"
            className="form-control"
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
            required
            disabled={loading || isLocked}
          />
        </div>

        <div className="form-group">
          <label htmlFor="password">Password</label>
          <input
            type="password"
            id="password"
            className="form-control"
            value={form.password}
            onChange={(e) => setForm({ ...form, password: e.target.value })}
            required
            disabled={loading || isLocked}
          />
        </div>

        <button
          type="submit"
          className="btn btn-primary"
          disabled={loading || isLocked}
        >
          {loading ? 'Logging in...' : 'Login'}
        </button>

        <div className="links">
          <button
            type="button"
            className="btn-link"
            onClick={() => navigate('/forgot-password')}
          >
            Forgot Password?
          </button>
          <button
            type="button"
            className="btn-link"
            onClick={() => navigate('/register')}
          >
            Create Account
          </button>
        </div>
      </form>
    </div>
  );
};

export default Login;
```

### Full-Featured Registration Component

```typescript
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';

interface RegisterForm {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  accountType: 'JobSeeker' | 'Recruiter';
}

const Register: React.FC = () => {
  const navigate = useNavigate();
  const [form, setForm] = useState<RegisterForm>({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    accountType: 'JobSeeker'
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);

  // Real-time password validation
  const validatePassword = (password: string): string[] => {
    const errors: string[] = [];
    if (password.length < 8) errors.push('At least 8 characters');
    if (password.length > 100) errors.push('Maximum 100 characters');
    if (!/[a-z]/.test(password)) errors.push('One lowercase letter');
    if (!/[A-Z]/.test(password)) errors.push('One uppercase letter');
    if (!/\d/.test(password)) errors.push('One digit');
    return errors;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});
    setLoading(true);

    // Client-side validation
    const validationErrors: Record<string, string> = {};
    
    if (form.password !== form.confirmPassword) {
      validationErrors.confirmPassword = 'Passwords do not match';
    }

    const passwordErrors = validatePassword(form.password);
    if (passwordErrors.length > 0) {
      validationErrors.password = `Password must contain: ${passwordErrors.join(', ')}`;
    }

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      setLoading(false);
      return;
    }

    try {
      const response = await authService.register(form);
      
      if (response.success) {
        // Registration successful, redirect to email verification
        navigate('/verify-email', { state: { email: form.email } });
      }
    } catch (err: any) {
      const errorData = err.response?.data;

      // Handle validation errors from backend
      if (errorData?.errors) {
        const backendErrors: Record<string, string> = {};
        Object.keys(errorData.errors).forEach((key) => {
          backendErrors[key.toLowerCase()] = errorData.errors[key][0];
        });
        setErrors(backendErrors);
      }
      // Handle other errors
      else {
        setErrors({ general: errorData?.message || 'Registration failed. Please try again.' });
      }
    } finally {
      setLoading(false);
    }
  };

  const passwordRequirements = validatePassword(form.password);
  const showPasswordHints = form.password.length > 0;

  return (
    <div className="register-container">
      <h2>Create Account</h2>

      {errors.general && (
        <div className="alert alert-danger">
          {errors.general}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Account Type Selection */}
        <div className="form-group">
          <label>Account Type</label>
          <div className="btn-group">
            <button
              type="button"
              className={`btn ${form.accountType === 'JobSeeker' ? 'btn-primary' : 'btn-outline-primary'}`}
              onClick={() => setForm({ ...form, accountType: 'JobSeeker' })}
            >
              Job Seeker
            </button>
            <button
              type="button"
              className={`btn ${form.accountType === 'Recruiter' ? 'btn-primary' : 'btn-outline-primary'}`}
              onClick={() => setForm({ ...form, accountType: 'Recruiter' })}
            >
              Recruiter
            </button>
          </div>
        </div>

        {/* Name Fields */}
        <div className="row">
          <div className="col-md-6">
            <div className="form-group">
              <label>First Name</label>
              <input
                type="text"
                className={`form-control ${errors.firstname ? 'is-invalid' : ''}`}
                value={form.firstName}
                onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                required
                disabled={loading}
              />
              {errors.firstname && <div className="invalid-feedback">{errors.firstname}</div>}
            </div>
          </div>
          <div className="col-md-6">
            <div className="form-group">
              <label>Last Name</label>
              <input
                type="text"
                className={`form-control ${errors.lastname ? 'is-invalid' : ''}`}
                value={form.lastName}
                onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                required
                disabled={loading}
              />
              {errors.lastname && <div className="invalid-feedback">{errors.lastname}</div>}
            </div>
          </div>
        </div>

        {/* Email */}
        <div className="form-group">
          <label>Email</label>
          <input
            type="email"
            className={`form-control ${errors.email ? 'is-invalid' : ''}`}
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
            required
            disabled={loading}
          />
          {errors.email && <div className="invalid-feedback">{errors.email}</div>}
        </div>

        {/* Password */}
        <div className="form-group">
          <label>Password</label>
          <input
            type="password"
            className={`form-control ${errors.password ? 'is-invalid' : ''}`}
            value={form.password}
            onChange={(e) => setForm({ ...form, password: e.target.value })}
            required
            disabled={loading}
          />
          {errors.password && <div className="invalid-feedback">{errors.password}</div>}
          
          {/* Password Strength Indicator */}
          {showPasswordHints && (
            <div className="password-requirements mt-2">
              <small className="text-muted">Password must contain:</small>
              <ul className="list-unstyled">
                {passwordRequirements.map((req, idx) => (
                  <li key={idx} className="text-danger">
                    <small>✗ {req}</small>
                  </li>
                ))}
                {passwordRequirements.length === 0 && (
                  <li className="text-success">
                    <small>✓ Strong password!</small>
                  </li>
                )}
              </ul>
            </div>
          )}
        </div>

        {/* Confirm Password */}
        <div className="form-group">
          <label>Confirm Password</label>
          <input
            type="password"
            className={`form-control ${errors.confirmpassword ? 'is-invalid' : ''}`}
            value={form.confirmPassword}
            onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })}
            required
            disabled={loading}
          />
          {errors.confirmpassword && <div className="invalid-feedback">{errors.confirmpassword}</div>}
        </div>

        <button
          type="submit"
          className="btn btn-primary btn-block"
          disabled={loading}
        >
          {loading ? 'Creating Account...' : 'Create Account'}
        </button>

        <div className="text-center mt-3">
          <span>Already have an account? </span>
          <button
            type="button"
            className="btn-link"
            onClick={() => navigate('/login')}
          >
            Login
          </button>
        </div>
      </form>
    </div>
  );
};

export default Register;
```

### Account Lockout Handler Hook

```typescript
// src/hooks/useAccountLockout.ts
import { useState, useEffect, useCallback } from 'react';

interface LockoutState {
  isLocked: boolean;
  lockoutEnd: Date | null;
  remainingMinutes: number;
  message: string;
}

export const useAccountLockout = () => {
  const [lockoutState, setLockoutState] = useState<LockoutState>({
    isLocked: false,
    lockoutEnd: null,
    remainingMinutes: 0,
    message: ''
  });

  const handleLockoutResponse = useCallback((error: any) => {
    const errorData = error.response?.data;
    
    if (errorData?.lockoutEnd) {
      const lockoutEnd = new Date(errorData.lockoutEnd);
      
      setLockoutState({
        isLocked: true,
        lockoutEnd,
        remainingMinutes: errorData.remainingMinutes || 15,
        message: errorData.message || 'Account is locked'
      });

      return true;
    }
    return false;
  }, []);

  // Countdown timer
  useEffect(() => {
    if (!lockoutState.isLocked || !lockoutState.lockoutEnd) return;

    const interval = setInterval(() => {
      const now = new Date().getTime();
      const end = lockoutState.lockoutEnd!.getTime();
      const remaining = Math.max(0, Math.ceil((end - now) / 60000));

      if (remaining === 0) {
        setLockoutState({
          isLocked: false,
          lockoutEnd: null,
          remainingMinutes: 0,
          message: ''
        });
      } else {
        setLockoutState((prev) => ({
          ...prev,
          remainingMinutes: remaining
        }));
      }
    }, 60000); // Update every minute

    return () => clearInterval(interval);
  }, [lockoutState.isLocked, lockoutState.lockoutEnd]);

  const clearLockout = useCallback(() => {
    setLockoutState({
      isLocked: false,
      lockoutEnd: null,
      remainingMinutes: 0,
      message: ''
    });
  }, []);

  return {
    lockoutState,
    handleLockoutResponse,
    clearLockout
  };
};
```

---

## Troubleshooting

### CORS Errors
- ✅ Backend is already configured for React on port 3000
- If issues persist, check browser console for exact error
- Ensure backend is running on `http://localhost:5217`

### 401 Unauthorized
- Check if token is stored correctly
- Verify token hasn't expired (24 hours)
- Check Authorization header format: `Bearer {token}`

### Email Verification Issues
- OTP codes expire in 15 minutes
- Use "Resend verification" if code expired
- Only the most recent code is valid

### Password Reset Issues
- OTP codes expire in 15 minutes
- Reset tokens expire in 5 minutes
- Complete the flow within time limits

---

## Next Steps After Authentication

Once authentication is complete, you can:

1. ✅ Redirect to profile completion wizard
2. ✅ Build job seeker dashboard
3. ✅ Build recruiter dashboard
4. ✅ Implement profile management
5. ✅ Implement job posting (for recruiters)

---

**Ready to integrate?** Start with the Register component and follow the authentication flow step by step.

**Questions?** Check the API Reference guide for detailed endpoint documentation.

---

## Frontend Integration Checklist

Use this checklist to ensure complete integration with the backend API:

### ✅ Authentication Features

- [ ] **User Registration**
  - [ ] Form with firstName, lastName, email, password, confirmPassword, accountType
  - [ ] Client-side password validation (8+ chars, uppercase, lowercase, digit)
  - [ ] Real-time password strength indicator
  - [ ] Handle validation errors from backend
  - [ ] Redirect to email verification on success

- [ ] **Email Verification**
  - [ ] Input for 6-digit OTP code
  - [ ] Handle 15-minute expiry
  - [ ] Resend verification button (limited to 3/hour)
  - [ ] Show countdown timer for resend
  - [ ] Redirect to login after successful verification

- [ ] **User Login**
  - [ ] Email and password form
  - [ ] Remember failed attempt count
  - [ ] Show warning at 3-4 failed attempts
  - [ ] Handle account lockout (5 attempts = 15min lock)
  - [ ] Display lockout countdown timer
  - [ ] Offer password reset when locked
  - [ ] Store JWT token securely
  - [ ] Redirect to dashboard on success

- [ ] **Password Reset Flow**
  - [ ] Forgot password: Request OTP (3 requests/hour)
  - [ ] Verify OTP: 6-digit code input (10 attempts/hour, 15min expiry)
  - [ ] Reset password: New password form with validation
  - [ ] Handle 5-minute reset token expiry
  - [ ] Show success message and redirect to login
  - [ ] Inform user that reset clears account lockout

- [ ] **Google OAuth Integration**
  - [ ] Google Sign-In button
  - [ ] Handle Google ID token
  - [ ] Select account type (JobSeeker/Recruiter)
  - [ ] Store JWT token on success
  - [ ] Handle both login and registration flows

### ✅ Security Implementations

- [ ] **Account Lockout**
  - [ ] Detect lockout response (`lockoutEnd`, `remainingMinutes`)
  - [ ] Display lockout alert with countdown
  - [ ] Show "Reset Password to Unlock" option
  - [ ] Handle lockout expiry (auto-unlock after 15min)
  - [ ] Display warning for remaining attempts (3-4 attempts)

- [ ] **Password Validation**
  - [ ] Client-side regex: `/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,100}$/`
  - [ ] Real-time validation feedback
  - [ ] Password strength indicator
  - [ ] Match backend validation rules exactly
  - [ ] Handle backend validation errors

- [ ] **Token Management**
  - [ ] Store JWT in localStorage/sessionStorage
  - [ ] Add `Authorization: Bearer {token}` header to requests
  - [ ] Handle 401 unauthorized (token expired/invalid)
  - [ ] Auto-logout on token expiry
  - [ ] Clear tokens on logout
  - [ ] Validate token on app start

### ✅ Error Handling

- [ ] **HTTP Status Codes**
  - [ ] 200: Success responses
  - [ ] 400: Validation errors (show field-specific messages)
  - [ ] 401: Unauthorized (redirect to login, handle lockout)
  - [ ] 500: Server error (generic error message)

- [ ] **Network Errors**
  - [ ] Handle connection timeout
  - [ ] Handle network unavailable
  - [ ] Retry logic for failed requests
  - [ ] User-friendly error messages

- [ ] **Validation Errors**
  - [ ] Parse backend error response format
  - [ ] Display field-specific errors
  - [ ] Highlight invalid fields
  - [ ] Clear errors on re-submission

### ✅ User Experience

- [ ] **Loading States**
  - [ ] Show spinners during API calls
  - [ ] Disable buttons during submission
  - [ ] Prevent double submissions

- [ ] **Navigation**
  - [ ] Protected routes require authentication
  - [ ] Redirect to login if not authenticated
  - [ ] Redirect authenticated users away from login/register
  - [ ] Persist user session across page refreshes

- [ ] **Feedback**
  - [ ] Success messages for actions
  - [ ] Clear error messages
  - [ ] Progress indicators for multi-step flows
  - [ ] Confirmation dialogs for destructive actions

### ✅ API Integration

- [ ] **Base Configuration**
  - [ ] API base URL configurable via environment variable
  - [ ] Default timeout (10 seconds recommended)
  - [ ] Request/response interceptors
  - [ ] Automatic token injection

- [ ] **Endpoints Implemented**
  - [ ] POST `/api/auth/register`
  - [ ] POST `/api/auth/login`
  - [ ] POST `/api/auth/google`
  - [ ] POST `/api/auth/verify-email`
  - [ ] POST `/api/auth/resend-verification`
  - [ ] POST `/api/auth/forgot-password`
  - [ ] POST `/api/auth/verify-reset-otp`
  - [ ] POST `/api/auth/reset-password`
  - [ ] GET `/api/auth/me` (requires authentication)

### ✅ Testing

- [ ] **Manual Testing**
  - [ ] Complete registration flow
  - [ ] Email verification flow
  - [ ] Login with correct credentials
  - [ ] Login with wrong password (test lockout)
  - [ ] Password reset flow
  - [ ] Google OAuth flow
  - [ ] Token expiry handling
  - [ ] Network error scenarios

- [ ] **Edge Cases**
  - [ ] Expired OTP codes
  - [ ] Expired reset tokens
  - [ ] Already verified email
  - [ ] Unverified email login attempt
  - [ ] Account already exists
  - [ ] Invalid email format
  - [ ] Weak password
  - [ ] Mismatched passwords

### 📋 Quick Reference

**Backend API URL:** `http://localhost:5217`  
**API Documentation:** Swagger UI at `http://localhost:5217/swagger`  
**Token Expiry:** 24 hours  
**OTP Expiry:** 15 minutes  
**Reset Token Expiry:** 5 minutes  
**Lockout Duration:** 15 minutes  
**Lockout Trigger:** 5 failed login attempts  

**Key Documentation Files:**
- [API_REFERENCE.md](API_REFERENCE.md) - Complete endpoint documentation
- [AUTH_API_INTEGRATION.md](AUTH_API_INTEGRATION.md) - Authentication flow details
- [GOOGLE_AUTH_GUIDE.md](GOOGLE_AUTH_GUIDE.md) - Google OAuth setup

**Need Help?**
- Check Swagger UI for live API testing
- Review example components in this guide
- Verify backend is running on correct port
- Check browser console for detailed errors

---

**Last Updated:** December 30, 2025  
**Backend Version:** 1.4.0  
**Status:** ✅ Production Ready