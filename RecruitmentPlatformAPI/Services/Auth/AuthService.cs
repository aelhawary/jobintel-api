using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Google.Apis.Auth;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Auth;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Identity;
using RecruitmentPlatformAPI.Models.JobSeeker;

namespace RecruitmentPlatformAPI.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;

        public AuthService(
            AppDbContext context,
            IEmailService emailService,
            ITokenService tokenService,
            ILogger<AuthService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _tokenService = tokenService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Normalize email: trim and convert to lowercase
                var normalizedEmail = NormalizeEmail(registerDto.Email);
                
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
                if (existingUser != null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "An account with this email already exists. Try logging in or use 'Forgot password'."
                    };
                }

                // Validate account type (case-insensitive) and normalize value
                var accountTypeInput = registerDto.AccountType?.Trim();
                var isJobSeeker = string.Equals(accountTypeInput, "JobSeeker", StringComparison.OrdinalIgnoreCase);
                var isRecruiter = string.Equals(accountTypeInput, "Recruiter", StringComparison.OrdinalIgnoreCase);
                if (!isJobSeeker && !isRecruiter)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid accountType. Allowed values: JobSeeker, Recruiter."
                    };
                }

                // Begin atomic transaction for user creation, role assignment, and email verification
                await using var transaction = await _context.Database.BeginTransactionAsync();

                // Trim names
                var trimmedFirstName = registerDto.FirstName?.Trim() ?? string.Empty;
                var trimmedLastName = registerDto.LastName?.Trim() ?? string.Empty;

                // Create user
                var user = new User
                {
                    FirstName = trimmedFirstName,
                    LastName = trimmedLastName,
                    Email = normalizedEmail, // Use normalized email
                    PasswordHash = HashPassword(registerDto.Password),
                    AccountType = isRecruiter ? AccountType.Recruiter : AccountType.JobSeeker,
                    IsEmailVerified = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create role-specific record
                if (isJobSeeker)
                {
                    var jobSeeker = new JobSeeker
                    {
                        UserId = user.Id,
                        // Foreign key IDs will be set during profile completion wizard
                        JobTitleId = null,
                        YearsOfExperience = null,
                        CountryId = null,
                        City = null,
                        PhoneNumber = null,
                        FirstLanguageId = null,
                        FirstLanguageProficiency = null,
                        SecondLanguageId = null,
                        SecondLanguageProficiency = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.JobSeekers.Add(jobSeeker);
                }
                else
                {
                    var recruiter = new Recruiter
                    {
                        UserId = user.Id,
                        CompanyName = "Not Specified", // To be updated in profile completion
                        CompanySize = "Not Specified", // To be updated in profile completion
                        Industry = "Not Specified", // To be updated in profile completion
                        Location = "Not Specified", // To be updated in profile completion
                        Website = null, // To be updated in profile completion
                        CompanyDescription = null, // To be updated in profile completion
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Recruiters.Add(recruiter);
                }

                // Generate and save verification code
                var verificationCode = _emailService.GenerateVerificationCode();
                var emailVerification = new EmailVerification
                {
                    UserId = user.Id,
                    VerificationCode = verificationCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    IsUsed = false
                };
                _context.EmailVerifications.Add(emailVerification);

                await _context.SaveChangesAsync();

                // Commit the transaction to ensure atomicity
                await transaction.CommitAsync();

                // Send verification email
                await _emailService.SendVerificationEmailAsync(user.Email, user.FirstName, verificationCode);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Registration successful. Please check your email to verify your account.",
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        AccountType = user.AccountType,
                        IsEmailVerified = user.IsEmailVerified,
                        IsActive = user.IsActive
                    }
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Registration failed due to database error");
                
                // Check if it's a unique constraint violation on email
                if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true ||
                    ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return new AuthResponseDto { Success = false, Message = "An account with this email already exists. Try logging in or use 'Forgot password'." };
                }
                
                return new AuthResponseDto { Success = false, Message = "Registration failed. Please try again later." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed due to unexpected error");
                return new AuthResponseDto { Success = false, Message = "Registration failed. Please try again later." };
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Normalize email: trim and convert to lowercase
                var normalizedEmail = NormalizeEmail(loginDto.Email);
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid email or password."
                    };
                }

                // CHECK IF ACCOUNT IS LOCKED
                if (user.LockoutEnd.HasValue)
                {
                    if (DateTime.UtcNow < user.LockoutEnd.Value)
                    {
                        // Account is still locked
                        var remainingTime = user.LockoutEnd.Value - DateTime.UtcNow;
                        int remainingMinutes = (int)Math.Ceiling(remainingTime.TotalMinutes);
                        
                        _logger.LogWarning($"Login attempt on locked account: {user.Email}. Locked until {user.LockoutEnd.Value}");
                        
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = $"Account is locked due to multiple failed login attempts. Please try again in {remainingMinutes} minute{(remainingMinutes != 1 ? "s" : "")} or reset your password.",
                            LockoutEnd = user.LockoutEnd.Value,
                            RemainingMinutes = remainingMinutes
                        };
                    }
                    else
                    {
                        // Lockout period has expired - auto-unlock
                        user.LockoutEnd = null;
                        user.LockoutReason = null;
                        user.FailedLoginAttempts = 0;
                        user.LastFailedLoginAt = null;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Account auto-unlocked: {user.Email}");
                    }
                }

                // OAuth users don't have password
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "This account uses Google sign-in. Please use 'Continue with Google' instead."
                    };
                }

                // Verify password
                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    // FAILED LOGIN - Increment counter
                    user.FailedLoginAttempts++;
                    user.LastFailedLoginAt = DateTime.UtcNow;
                    
                    // Check if account should be locked
                    if (user.FailedLoginAttempts >= 5)
                    {
                        // LOCK THE ACCOUNT
                        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                        user.LockoutReason = "Too many failed login attempts";
                        await _context.SaveChangesAsync();
                        
                        // Send lockout notification email
                        await _emailService.SendAccountLockedEmailAsync(
                            user.Email,
                            user.FirstName,
                            user.LockoutEnd.Value
                        );
                        
                        _logger.LogWarning($"Account locked due to {user.FailedLoginAttempts} failed attempts: {user.Email}");
                        
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = "Account locked due to multiple failed login attempts. Check your email for instructions. The account will automatically unlock in 15 minutes.",
                            LockoutEnd = user.LockoutEnd.Value,
                            RemainingMinutes = 15
                        };
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    int attemptsLeft = 5 - user.FailedLoginAttempts;
                    _logger.LogWarning($"Failed login attempt {user.FailedLoginAttempts}/5 for: {user.Email}");
                    
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = $"Invalid email or password. {attemptsLeft} attempt{(attemptsLeft != 1 ? "s" : "")} remaining before account lockout."
                    };
                }

                if (!user.IsActive)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Your account is deactivated. Please contact support."
                    };
                }

                if (!user.IsEmailVerified)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Your email address isn't verified yet. Please verify to continue or request a new verification code."
                    };
                }

                // SUCCESSFUL LOGIN - Reset all lockout/failure counters
                user.FailedLoginAttempts = 0;
                user.LastFailedLoginAt = null;
                user.LockoutEnd = null;
                user.LockoutReason = null;
                user.LastSuccessfulLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                var token = GenerateJwtToken(user);

                _logger.LogInformation($"Successful login: {user.Email}");

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful.",
                    Token = token,
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        AccountType = user.AccountType,
                        IsEmailVerified = user.IsEmailVerified,
                        IsActive = user.IsActive
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed due to unexpected error");
                return new AuthResponseDto { Success = false, Message = "Login failed. Please try again later." };
            }
        }

        public async Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                var clientId = _configuration["GoogleOAuth:ClientId"];
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = string.IsNullOrEmpty(clientId) ? null : new[] { clientId }
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                return new GoogleUserInfo
                {
                    Sub = payload.Subject,
                    Email = payload.Email,
                    EmailVerified = payload.EmailVerified,
                    GivenName = payload.GivenName ?? "",
                    FamilyName = payload.FamilyName ?? "",
                    Picture = payload.Picture
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google token verification failed");
                return null;
            }
        }

        public async Task<AuthResponseDto> GoogleAuthAsync(GoogleAuthDto googleAuthDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verify Google token
                var googleUser = await VerifyGoogleTokenAsync(googleAuthDto.IdToken);
                if (googleUser == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid Google token. Please try again."
                    };
                }

                if (!googleUser.EmailVerified)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Your Google email is not verified. Please verify your email with Google first."
                    };
                }

                var normalizedEmail = NormalizeEmail(googleUser.Email);

                // Check if user exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

                if (existingUser != null)
                {
                    // User exists - handle login
                    if (!existingUser.IsActive)
                    {
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = "Your account is deactivated. Please contact support."
                        };
                    }

                    // Update OAuth info if not already set
                    if (existingUser.AuthProvider == AuthProvider.Email || existingUser.ProviderUserId == null)
                    {
                        existingUser.AuthProvider = AuthProvider.Google;
                        existingUser.ProviderUserId = googleUser.Sub;
                        existingUser.ProfilePictureUrl = googleUser.Picture;
                        existingUser.IsEmailVerified = true;
                        existingUser.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    var token = GenerateJwtToken(existingUser);

                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Login successful.",
                        Token = token,
                        User = new UserInfoDto
                        {
                            Id = existingUser.Id,
                            FirstName = existingUser.FirstName,
                            LastName = existingUser.LastName,
                            Email = existingUser.Email,
                            AccountType = existingUser.AccountType,
                            IsEmailVerified = existingUser.IsEmailVerified,
                            IsActive = existingUser.IsActive
                        }
                    };
                }

                // New user - create account
                var accountType = Enum.Parse<AccountType>(googleAuthDto.AccountType, ignoreCase: true);

                var newUser = new User
                {
                    FirstName = googleUser.GivenName,
                    LastName = googleUser.FamilyName,
                    Email = normalizedEmail,
                    AccountType = accountType,
                    AuthProvider = AuthProvider.Google,
                    ProviderUserId = googleUser.Sub,
                    ProfilePictureUrl = googleUser.Picture,
                    PasswordHash = null, // No password for OAuth users
                    IsEmailVerified = true, // Google emails are pre-verified
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Create corresponding profile
                if (accountType == AccountType.JobSeeker)
                {
                    var jobSeeker = new JobSeeker
                    {
                        UserId = newUser.Id,
                        // Foreign key IDs will be set during profile completion wizard
                        JobTitleId = null,
                        YearsOfExperience = null,
                        CountryId = null,
                        City = null,
                        PhoneNumber = null,
                        FirstLanguageId = null,
                        FirstLanguageProficiency = null,
                        SecondLanguageId = null,
                        SecondLanguageProficiency = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.JobSeekers.Add(jobSeeker);
                }
                else
                {
                    var recruiter = new Recruiter
                    {
                        UserId = newUser.Id,
                        CompanyName = "Not Specified", // To be updated in profile completion
                        CompanySize = "Not Specified", // To be updated in profile completion
                        Industry = "Not Specified", // To be updated in profile completion
                        Location = "Not Specified", // To be updated in profile completion
                        Website = null, // To be updated in profile completion
                        CompanyDescription = null, // To be updated in profile completion
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Recruiters.Add(recruiter);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send welcome email (consistent with email verification flow)
                await _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FirstName);

                var newToken = GenerateJwtToken(newUser);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Account created successfully. Welcome!",
                    Token = newToken,
                    User = new UserInfoDto
                    {
                        Id = newUser.Id,
                        FirstName = newUser.FirstName,
                        LastName = newUser.LastName,
                        Email = newUser.Email,
                        AccountType = newUser.AccountType,
                        IsEmailVerified = newUser.IsEmailVerified,
                        IsActive = newUser.IsActive
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Google authentication failed");
                return new AuthResponseDto { Success = false, Message = "Google authentication failed. Please try again later." };
            }
        }

        public async Task<AuthResponseDto> VerifyEmailAsync(EmailVerificationDto verificationDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(verificationDto.Email) || string.IsNullOrWhiteSpace(verificationDto.VerificationCode))
                {
                    _logger.LogWarning("Email verification attempted with missing email or code");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email and verification code are required."
                    };
                }

                // Normalize email: trim and convert to lowercase
                var normalizedEmail = NormalizeEmail(verificationDto.Email);
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
                if (user == null)
                {
                    _logger.LogWarning($"Email verification attempted for non-existent user: {normalizedEmail}");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                // Check if email is already verified
                if (user.IsEmailVerified)
                {
                    _logger.LogInformation($"Email verification attempted for already verified user: {user.Email}");
                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Email is already verified. You can log in now."
                    };
                }

                // Get only the most recent unused verification code
                var verification = await _context.EmailVerifications
                    .Where(e => e.UserId == user.Id && !e.IsUsed)
                    .OrderByDescending(e => e.CreatedAt)
                    .FirstOrDefaultAsync();

                if (verification == null)
                {
                    _logger.LogWarning($"No valid verification code found for user: {user.Email}");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "No valid verification code found. Please request a new one."
                    };
                }

                // Check expiration before code comparison
                if (verification.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Expired verification code used for user: {user.Email}");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Verification code has expired. Please request a new one."
                    };
                }

                // Use constant-time comparison to prevent timing attacks
                if (!ConstantTimeEquals(verification.VerificationCode, verificationDto.VerificationCode))
                {
                    _logger.LogWarning($"Invalid verification code attempt for user: {user.Email}");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid verification code. Please use the most recent code sent to your email."
                    };
                }

                // Invalidate ALL verification codes for this user BEFORE marking as verified
                var allVerifications = await _context.EmailVerifications
                    .Where(e => e.UserId == user.Id && !e.IsUsed)
                    .ToListAsync();

                foreach (var v in allVerifications)
                {
                    v.IsUsed = true;
                }

                // Mark user as email verified
                user.IsEmailVerified = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Email successfully verified for user: {user.Email}");

                // Send welcome email (outside transaction)
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);

                // Security improvement: Do NOT issue JWT token here
                // User should log in after verification for proper session management
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Email verified successfully! Please log in to continue.",
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        AccountType = user.AccountType,
                        IsEmailVerified = user.IsEmailVerified,
                        IsActive = user.IsActive
                    }
                };
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Email verification failed due to database error for email: {verificationDto.Email}");
                return new AuthResponseDto { Success = false, Message = "Email verification failed. Please try again later." };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Email verification failed due to unexpected error for email: {verificationDto.Email}");
                return new AuthResponseDto { Success = false, Message = "Email verification failed. Please try again later." };
            }
        }

        public async Task<AuthResponseDto> ResendVerificationCodeAsync(ResendVerificationDto resendDto)
        {
            try
            {
                // Normalize email: trim and convert to lowercase
                var normalizedEmail = NormalizeEmail(resendDto.Email);
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                if (user.IsEmailVerified)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email is already verified."
                    };
                }

                // Invalidate all previous verification codes for this user
                var previousVerifications = await _context.EmailVerifications
                    .Where(e => e.UserId == user.Id && !e.IsUsed)
                    .ToListAsync();

                foreach (var prev in previousVerifications)
                {
                    prev.IsUsed = true;
                }

                // Generate new verification code
                var verificationCode = _emailService.GenerateVerificationCode();
                var emailVerification = new EmailVerification
                {
                    UserId = user.Id,
                    VerificationCode = verificationCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    IsUsed = false
                };
                _context.EmailVerifications.Add(emailVerification);
                await _context.SaveChangesAsync();

                // Send verification email
                await _emailService.SendVerificationEmailAsync(user.Email, user.FirstName, verificationCode);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Verification code sent successfully. Please check your email."
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Resend verification code failed due to database error");
                return new AuthResponseDto { Success = false, Message = "Failed to resend verification code. Please try again later." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resend verification code failed due to unexpected error");
                return new AuthResponseDto { Success = false, Message = "Failed to resend verification code. Please try again later." };
            }
        }

        public string GenerateJwtToken(User user) => _tokenService.GenerateJwtToken(user);

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        /// <summary>
        /// Constant-time string comparison to prevent timing attacks on verification codes.
        /// </summary>
        private bool ConstantTimeEquals(string a, string b)
        {
            if (a == null || b == null)
                return a == b;

            if (a.Length != b.Length)
                return false;

            uint diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }

            return diff == 0;
        }

        public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                // Normalize email: trim and convert to lowercase
                var normalizedEmail = NormalizeEmail(forgotPasswordDto.Email);
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
                if (user == null)
                {
                    // Don't reveal if user exists or not for security
                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "If your email is associated with an account, you’ll receive a password reset link shortly."
                    };
                }

                // Invalidate all previous password reset tokens for this user
                var previousTokens = await _context.PasswordResets
                    .Where(p => p.UserId == user.Id && !p.IsUsed)
                    .ToListAsync();

                foreach (var prev in previousTokens)
                {
                    prev.IsUsed = true;
                }

                // Generate new secure token
                var secureToken = _emailService.GenerateSecureToken();
                var passwordReset = new PasswordReset
                {
                    UserId = user.Id,
                    Token = secureToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15), // Token valid for 15 minutes
                    IsUsed = false
                };
                _context.PasswordResets.Add(passwordReset);
                await _context.SaveChangesAsync();

                // Send password reset link email
                await _emailService.SendPasswordResetLinkAsync(user.Email, user.FirstName, secureToken);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "If your email is associated with an account, you’ll receive a password reset link shortly."
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Forgot password failed due to database error");
                return new AuthResponseDto { Success = false, Message = "Failed to process password reset request. Please try again later." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forgot password failed due to unexpected error");
                return new AuthResponseDto { Success = false, Message = "Failed to process password reset request. Please try again later." };
            }
        }

        public async Task<AuthResponseDto> ValidateResetTokenAsync(ValidateResetTokenDto validateDto)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(validateDto.Token))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Reset token is required."
                    };
                }

                // Find the password reset record by token
                var passwordReset = await _context.PasswordResets
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Token == validateDto.Token && !p.IsUsed);

                if (passwordReset == null)
                {
                    _logger.LogWarning("Invalid or already used password reset token");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired reset link. Please request a new password reset."
                    };
                }

                // Check if token has expired
                if (passwordReset.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Expired password reset token for user: {passwordReset.User?.Email}");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "This reset link has expired. Please request a new password reset."
                    };
                }

                _logger.LogInformation($"Password reset token validated for user: {passwordReset.User?.Email}");

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Reset link is valid. You can proceed to reset your password."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed due to unexpected error");
                return new AuthResponseDto { Success = false, Message = "Token validation failed. Please try again later." };
            }
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(resetPasswordDto.Token))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Reset token is required."
                    };
                }

                // Find the password reset record by token
                var passwordReset = await _context.PasswordResets
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Token == resetPasswordDto.Token && !p.IsUsed);

                if (passwordReset == null)
                {
                    _logger.LogWarning("Invalid or already used password reset token");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired reset link. Please request a new password reset."
                    };
                }

                // Check if token has expired
                if (passwordReset.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Expired password reset token for user: {passwordReset.User?.Email}");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "This reset link has expired. Please request a new password reset."
                    };
                }

                var user = passwordReset.User;
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                // Mark token as used (single-use)
                passwordReset.IsUsed = true;

                // Update user's password
                user.PasswordHash = HashPassword(resetPasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                
                // Clear lockout counters when password is reset
                user.FailedLoginAttempts = 0;
                user.LastFailedLoginAt = null;
                user.LockoutEnd = null;
                user.LockoutReason = null;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Password reset successful for: {user.Email}. Lockout counters cleared.");

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Password reset successfully. You can now login with your new password."
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Password reset failed due to database error");
                return new AuthResponseDto { Success = false, Message = "Password reset failed. Please try again later." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset failed due to unexpected error");
                return new AuthResponseDto { Success = false, Message = "Password reset failed. Please try again later." };
            }
        }
    }
}
