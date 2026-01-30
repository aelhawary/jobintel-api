using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.DTOs.Auth;
using RecruitmentPlatformAPI.Services.Auth;

namespace RecruitmentPlatformAPI.Controllers
{
    /// <summary>
    /// Authentication and authorization endpoints for user registration, login, email verification, and password reset
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user (JobSeeker or Recruiter)
        /// </summary>
        /// <param name="registerDto">Registration details</param>
        /// <returns>Auth response with user info (email not verified yet)</returns>
        /// <response code="200">Registration successful - verification email sent</response>
        /// <response code="400">Bad request - validation failed or user already exists</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.RegisterAsync(registerDto);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        /// <summary>
        /// Login user and return JWT token
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>Auth response with JWT token and user info</returns>
        /// <response code="200">Login successful - returns JWT token</response>
        /// <response code="401">Unauthorized - Invalid credentials</response>
        /// <response code="403">Forbidden - Email not verified or account inactive</response>
        /// <response code="423">Locked - Account temporarily locked due to failed attempts</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status423Locked)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);
            
            if (result.Success)
            {
                // Successful login - do not log sensitive email in production
                return Ok(result);
            }

            // Account locked due to failed attempts
            if (result.LockoutEnd.HasValue)
            {
                return StatusCode(StatusCodes.Status423Locked, result);
            }

            // Check for email verification or account status issues
            if (result.Message.Contains("not verified", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("deactivated", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, result);
            }

            // Invalid credentials or other authentication failures
            return Unauthorized(result);
        }

        /// <summary>
        /// Authenticate with Google account
        /// </summary>
        /// <param name="googleAuthDto">Google authentication details</param>
        /// <returns>Auth response with JWT token and user info</returns>
        /// <response code="200">Google authentication successful - returns JWT token</response>
        /// <response code="401">Unauthorized - Invalid Google token or email not verified</response>
        [HttpPost("google")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> GoogleAuth([FromBody] GoogleAuthDto googleAuthDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.GoogleAuthAsync(googleAuthDto);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return Unauthorized(result);
        }

        /// <summary>
        /// Verify email address with verification code
        /// NOTE: This endpoint does NOT authenticate the user or return a JWT token.
        /// After successful verification, users must login with their credentials.
        /// </summary>
        /// <param name="verificationDto">Email and verification code</param>
        /// <returns>Success response without JWT token - user must login separately</returns>
        /// <response code="200">Email verified successfully - user must now login</response>
        /// <response code="400">Bad request - invalid code, expired code, or user not found</response>
        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> VerifyEmail([FromBody] EmailVerificationDto verificationDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.VerifyEmailAsync(verificationDto);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        /// <summary>
        /// Resend email verification code
        /// </summary>
        /// <param name="resendDto">Email address to resend code</param>
        /// <returns>Success message</returns>
        /// <response code="200">Verification code sent successfully</response>
        /// <response code="400">Bad request - user not found or email already verified</response>
        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> ResendVerification([FromBody] ResendVerificationDto resendDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.ResendVerificationCodeAsync(resendDto);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        /// <summary>
        /// Request password reset OTP via email
        /// NOTE: Always returns 200 OK to prevent email enumeration attacks
        /// </summary>
        /// <param name="forgotPasswordDto">Email address to send OTP</param>
        /// <returns>Success message (always 200 OK for security)</returns>
        /// <response code="200">Request processed - OTP sent if email exists</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AuthResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);
            
            // Always return 200 OK to prevent email enumeration
            return Ok(result);
        }

        /// <summary>
        /// Verify password reset OTP and receive temporary reset token
        /// </summary>
        /// <param name="verifyOtpDto">Email and OTP code</param>
        /// <returns>Auth response with reset token</returns>
        /// <response code="200">OTP verified - returns temporary reset token</response>
        /// <response code="400">Bad request - invalid OTP, expired OTP, or user not found</response>
        [HttpPost("verify-reset-otp")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> VerifyResetOtp([FromBody] VerifyResetOtpDto verifyOtpDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.VerifyResetOtpAsync(verifyOtpDto);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        /// <summary>
        /// Reset password using temporary reset token
        /// </summary>
        /// <param name="resetPasswordDto">Email, reset token, and new password</param>
        /// <returns>Success message</returns>
        /// <response code="200">Password reset successful</response>
        /// <response code="400">Bad request - invalid token, expired token, or validation failed</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        /// <summary>
        /// Get current user information from JWT token
        /// </summary>
        /// <returns>User information extracted from JWT claims</returns>
        /// <response code="200">Returns user information successfully</response>
        /// <response code="401">Unauthorized - Invalid or missing JWT token</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<CurrentUserResponse> GetCurrentUser()
        {
            // Extract claims from JWT token
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var firstName = User.FindFirst("FirstName")?.Value;
            var lastName = User.FindFirst("LastName")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Success = false, Message = "Invalid token" });
            }

            var response = new CurrentUserResponse
            {
                Success = true,
                User = new CurrentUserDto
                {
                    Id = userId,
                    Email = email ?? string.Empty,
                    Name = name ?? string.Empty,
                    Role = role ?? string.Empty,
                    FirstName = firstName ?? string.Empty,
                    LastName = lastName ?? string.Empty
                }
            };

            return Ok(response);
        }
    }
}
