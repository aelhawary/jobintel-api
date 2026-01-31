using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using RecruitmentPlatformAPI.Configuration;

namespace RecruitmentPlatformAPI.Services.Auth
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> SendVerificationEmailAsync(string email, string firstName, string verificationCode)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(firstName, email));
                message.Subject = "Welcome to JobIntel! Verify Your Email 🎯";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                        <body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 0;'>
                            <div style='max-width: 600px; margin: 40px auto; background-color: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden;'>
                                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center;'>
                                    <h1 style='color: white; margin: 0; font-size: 32px; font-weight: 700;'>JobIntel</h1>
                                    <p style='color: rgba(255,255,255,0.9); margin: 10px 0 0 0; font-size: 16px;'>Your Intelligent Career Partner</p>
                                </div>
                                <div style='padding: 40px 30px;'>
                                    <h2 style='color: #333; margin: 0 0 20px 0; font-size: 24px;'>Welcome Aboard, {firstName}! 👋</h2>
                                    <p style='color: #555; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>
                                        We're thrilled to have you join JobIntel! You're just one step away from unlocking a world of career opportunities powered by AI.
                                    </p>
                                    <p style='color: #555; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>
                                        Please verify your email address using the code below:
                                    </p>
                                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 25px; margin: 30px 0; text-align: center; border-radius: 8px;'>
                                        <p style='color: white; margin: 0 0 10px 0; font-size: 14px; font-weight: 600; text-transform: uppercase; letter-spacing: 1px;'>Your Verification Code</p>
                                        <div style='background-color: white; padding: 15px; border-radius: 6px; display: inline-block;'>
                                            <h1 style='color: #667eea; margin: 0; letter-spacing: 8px; font-size: 36px; font-weight: 700;'>{verificationCode}</h1>
                                        </div>
                                    </div>
                                    <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 30px 0; border-radius: 4px;'>
                                        <p style='color: #856404; margin: 0; font-size: 14px;'>
                                            ⏱️ <strong>Important:</strong> This code expires in 15 minutes for your security.
                                        </p>
                                    </div>
                                    <p style='color: #777; font-size: 14px; line-height: 1.6; margin: 20px 0 0 0;'>
                                        If you didn't create a JobIntel account, you can safely ignore this email.
                                    </p>
                                </div>
                                <div style='background-color: #f8f9fa; padding: 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                                    <p style='color: #6c757d; font-size: 14px; margin: 0 0 10px 0;'>
                                        Need help? Contact us at <a href='mailto:{_emailSettings.SenderEmail}' style='color: #667eea; text-decoration: none;'>{_emailSettings.SenderEmail}</a>
                                    </p>
                                    <p style='color: #6c757d; font-size: 12px; margin: 0;'>
                                        © {DateTime.Now.Year} JobIntel. All rights reserved.
                                    </p>
                                </div>
                            </div>
                        </body>
                        </html>",
                    TextBody = $@"Welcome to JobIntel!

Hi {firstName},

We're thrilled to have you join JobIntel - Your Intelligent Career Partner!

Your verification code is: {verificationCode}

⏱️ This code expires in 15 minutes for your security.

If you didn't create a JobIntel account, you can safely ignore this email.

Need help? Contact us at {_emailSettings.SenderEmail}

Best regards,
The JobIntel Team

© {DateTime.Now.Year} JobIntel. All rights reserved."
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Connect to SMTP server
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                // Authenticate
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                
                // Send email
                await client.SendAsync(message);
                
                // Disconnect
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Verification email sent successfully to: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send verification email to {email}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(firstName, email));
                message.Subject = "🎉 Welcome to JobIntel - Your Account is Active!";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                        <body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 0;'>
                            <div style='max-width: 600px; margin: 40px auto; background-color: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden;'>
                                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center;'>
                                    <h1 style='color: white; margin: 0; font-size: 32px; font-weight: 700;'>JobIntel</h1>
                                    <p style='color: rgba(255,255,255,0.9); margin: 10px 0 0 0; font-size: 16px;'>Your Intelligent Career Partner</p>
                                </div>
                                <div style='padding: 40px 30px;'>
                                    <div style='text-align: center; margin-bottom: 30px;'>
                                        <span style='font-size: 60px;'>✨</span>
                                    </div>
                                    <h2 style='color: #28a745; margin: 0 0 20px 0; font-size: 28px; text-align: center;'>Account Verified Successfully!</h2>
                                    <p style='color: #555; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0; text-align: center;'>
                                        Congratulations, <strong>{firstName}</strong>! Your email has been verified and your account is now active.
                                    </p>
                                    <div style='background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); padding: 30px; margin: 30px 0; text-align: center; border-radius: 8px;'>
                                        <h3 style='color: white; margin: 0 0 20px 0; font-size: 20px;'>What's Next?</h3>
                                        <div style='background-color: rgba(255,255,255,0.15); padding: 20px; border-radius: 6px; backdrop-filter: blur(10px);'>
                                            <div style='text-align: left; color: white;'>
                                                <p style='margin: 10px 0; font-size: 15px;'>✅ Complete your profile to stand out</p>
                                                <p style='margin: 10px 0; font-size: 15px;'>🔍 Explore AI-powered job recommendations</p>
                                                <p style='margin: 10px 0; font-size: 15px;'>🤝 Connect with top recruiters</p>
                                                <p style='margin: 10px 0; font-size: 15px;'>📊 Track your application progress</p>
                                            </div>
                                        </div>
                                    </div>
                                    <div style='text-align: center; margin: 40px 0 30px 0;'>
                                        <a href='{_emailSettings.ApplicationUrl}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 40px; text-decoration: none; border-radius: 25px; display: inline-block; font-weight: 600; font-size: 16px; box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);'>Get Started Now →</a>
                                    </div>
                                    <div style='background-color: #e7f3ff; border-left: 4px solid #2196f3; padding: 15px; margin: 30px 0; border-radius: 4px;'>
                                        <p style='color: #1565c0; margin: 0; font-size: 14px;'>
                                            💡 <strong>Pro Tip:</strong> Complete your profile soon to get featured to recruiters!
                                        </p>
                                    </div>
                                </div>
                                <div style='background-color: #f8f9fa; padding: 30px; border-top: 1px solid #e9ecef;'>
                                    <div style='text-align: center; margin-bottom: 20px;'>
                                        <p style='color: #6c757d; font-size: 14px; margin: 0 0 15px 0;'>Follow us for career tips and updates:</p>
                                        <div style='margin: 15px 0;'>
                                            <a href='#' style='color: #667eea; text-decoration: none; margin: 0 10px; font-size: 14px;'>LinkedIn</a>
                                            <span style='color: #dee2e6;'>|</span>
                                            <a href='#' style='color: #667eea; text-decoration: none; margin: 0 10px; font-size: 14px;'>Twitter</a>
                                            <span style='color: #dee2e6;'>|</span>
                                            <a href='#' style='color: #667eea; text-decoration: none; margin: 0 10px; font-size: 14px;'>Facebook</a>
                                        </div>
                                    </div>
                                    <div style='text-align: center; border-top: 1px solid #e9ecef; padding-top: 20px; margin-top: 20px;'>
                                        <p style='color: #6c757d; font-size: 14px; margin: 0 0 10px 0;'>
                                            Need help? Contact us at <a href='mailto:{_emailSettings.SenderEmail}' style='color: #667eea; text-decoration: none;'>{_emailSettings.SenderEmail}</a>
                                        </p>
                                        <p style='color: #6c757d; font-size: 12px; margin: 0;'>
                                            © {DateTime.Now.Year} JobIntel. All rights reserved.
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </body>
                        </html>",
                    TextBody = $@"Welcome to JobIntel!

Hi {firstName},

Congratulations! Your email has been verified and your account is now active.

What's Next?
✅ Complete your profile to stand out
🔍 Explore AI-powered job recommendations
🤝 Connect with top recruiters
📊 Track your application progress

Get started now: {_emailSettings.ApplicationUrl}

💡 Pro Tip: Complete your profile soon to get featured to recruiters!

Need help? Contact us at {_emailSettings.SenderEmail}

Best regards,
The JobIntel Team

© {DateTime.Now.Year} JobIntel. All rights reserved."
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Welcome email sent successfully to: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send welcome email to {email}: {ex.Message}");
                return false;
            }
        }

        public string GenerateVerificationCode()
        {
            // Use cryptographically secure random number generator
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = Math.Abs(BitConverter.ToInt32(bytes, 0));
            return (randomNumber % 900000 + 100000).ToString();
        }

        /// <summary>
        /// Generate a cryptographically secure token for password reset links
        /// </summary>
        public string GenerateSecureToken()
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[48]; // 48 bytes = 64 base64 characters
            rng.GetBytes(bytes);
            // Use URL-safe base64 encoding (replace + with -, / with _, remove =)
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        public async Task<bool> SendPasswordResetLinkAsync(string email, string firstName, string resetToken)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(firstName, email));
                message.Subject = "🔒 Reset Your JobIntel Password";

                // Build the reset link URL
                var resetLink = $"{_emailSettings.FrontendUrl}/reset-password?token={resetToken}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                        <body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 0;'>
                            <div style='max-width: 600px; margin: 40px auto; background-color: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden;'>
                                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center;'>
                                    <h1 style='color: white; margin: 0; font-size: 32px; font-weight: 700;'>JobIntel</h1>
                                    <p style='color: rgba(255,255,255,0.9); margin: 10px 0 0 0; font-size: 16px;'>Your Intelligent Career Partner</p>
                                </div>
                                <div style='padding: 40px 30px;'>
                                    <div style='text-align: center; margin-bottom: 20px;'>
                                        <span style='font-size: 60px;'>🔐</span>
                                    </div>
                                    <h2 style='color: #333; margin: 0 0 20px 0; font-size: 24px; text-align: center;'>Password Reset Request</h2>
                                    <p style='color: #555; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>
                                        Hi {firstName},
                                    </p>
                                    <p style='color: #555; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>
                                        We received a request to reset the password for your JobIntel account. Click the button below to reset your password:
                                    </p>
                                    <div style='text-align: center; margin: 30px 0;'>
                                        <a href='{resetLink}' style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; padding: 16px 40px; font-size: 18px; font-weight: 600; border-radius: 8px; box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);'>
                                            Reset My Password
                                        </a>
                                    </div>
                                    <p style='color: #777; font-size: 13px; text-align: center; margin: 20px 0;'>
                                        Or copy and paste this link into your browser:
                                    </p>
                                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 6px; word-break: break-all; margin: 20px 0;'>
                                        <a href='{resetLink}' style='color: #667eea; font-size: 13px; text-decoration: none;'>{resetLink}</a>
                                    </div>
                                    <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 30px 0; border-radius: 4px;'>
                                        <p style='color: #856404; margin: 0; font-size: 14px;'>
                                            ⏱️ <strong>Time-Sensitive:</strong> This link expires in 15 minutes for your security.
                                        </p>
                                    </div>
                                    <div style='background-color: #f8d7da; border-left: 4px solid #dc3545; padding: 15px; margin: 30px 0; border-radius: 4px;'>
                                        <p style='color: #721c24; margin: 0 0 10px 0; font-size: 14px; font-weight: 600;'>
                                            🛡️ Security Tips:
                                        </p>
                                        <ul style='color: #721c24; margin: 0; padding-left: 20px; font-size: 13px;'>
                                            <li style='margin: 5px 0;'>Never share this link with anyone, including JobIntel staff</li>
                                            <li style='margin: 5px 0;'>If you didn't request this reset, please ignore this email</li>
                                            <li style='margin: 5px 0;'>Your password will remain unchanged if you don't take action</li>
                                        </ul>
                                    </div>
                                    <div style='background-color: #e7f3ff; border-left: 4px solid #2196f3; padding: 15px; margin: 30px 0; border-radius: 4px;'>
                                        <p style='color: #1565c0; margin: 0; font-size: 14px;'>
                                            <strong>Didn't request this?</strong> If you didn't request a password reset, you can safely ignore this email. Your account is secure.
                                        </p>
                                    </div>
                                </div>
                                <div style='background-color: #f8f9fa; padding: 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                                    <p style='color: #6c757d; font-size: 14px; margin: 0 0 10px 0;'>
                                        Need help? Contact us at <a href='mailto:{_emailSettings.SenderEmail}' style='color: #667eea; text-decoration: none;'>{_emailSettings.SenderEmail}</a>
                                    </p>
                                    <p style='color: #6c757d; font-size: 12px; margin: 0;'>
                                        © {DateTime.Now.Year} JobIntel. All rights reserved.
                                    </p>
                                </div>
                            </div>
                        </body>
                        </html>",
                    TextBody = $@"JobIntel - Password Reset Request

Hi {firstName},

We received a request to reset the password for your JobIntel account.

Click the link below to reset your password:
{resetLink}

⏱️ This link expires in 15 minutes for your security.

🛡️ Security Tips:
• Never share this link with anyone, including JobIntel staff
• If you didn't request this reset, please ignore this email
• Your password will remain unchanged if you don't take action

Didn't request this? If you didn't request a password reset, you can safely ignore this email. Your account is secure.

Need help? Contact us at {_emailSettings.SenderEmail}

Best regards,
The JobIntel Team

© {DateTime.Now.Year} JobIntel. All rights reserved."
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Password reset link sent successfully to: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send password reset link to {email}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendAccountLockedEmailAsync(string email, string firstName, DateTime lockoutEnd)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(firstName, email));
                message.Subject = "🔒 Security Alert: Account Temporarily Locked";

                // Calculate remaining time
                var remainingTime = lockoutEnd - DateTime.UtcNow;
                int remainingMinutes = (int)Math.Ceiling(remainingTime.TotalMinutes);
                string unlockTimeLocal = lockoutEnd.ToLocalTime().ToString("h:mm tt");

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                        <body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 0;'>
                            <div style='max-width: 600px; margin: 40px auto; background-color: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden;'>
                                <div style='background: linear-gradient(135deg, #dc3545 0%, #c82333 100%); padding: 40px 20px; text-align: center;'>
                                    <div style='font-size: 60px; margin-bottom: 10px;'>🔒</div>
                                    <h1 style='color: white; margin: 0; font-size: 28px; font-weight: 700;'>Account Temporarily Locked</h1>
                                </div>
                                <div style='padding: 40px 30px;'>
                                    <h2 style='color: #333; margin: 0 0 20px 0; font-size: 20px;'>Hi {firstName},</h2>
                                    <p style='color: #555; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>
                                        Your JobIntel account has been temporarily locked due to <strong>5 consecutive failed login attempts</strong>.
                                    </p>
                                    <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 20px; margin: 30px 0; border-radius: 4px;'>
                                        <p style='color: #856404; margin: 0 0 10px 0; font-size: 16px; font-weight: 600;'>
                                            ⏱️ Lockout Details
                                        </p>
                                        <p style='color: #856404; margin: 0; font-size: 14px;'>
                                            Your account will automatically unlock in <strong>{remainingMinutes} minutes</strong> (at {unlockTimeLocal}).
                                        </p>
                                    </div>
                                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 25px; margin: 30px 0; text-align: center; border-radius: 8px;'>
                                        <h3 style='color: white; margin: 0 0 15px 0; font-size: 18px;'>What You Can Do:</h3>
                                        <div style='background-color: rgba(255,255,255,0.15); padding: 20px; border-radius: 6px;'>
                                            <div style='text-align: left; color: white;'>
                                                <p style='margin: 10px 0; font-size: 15px;'>⏳ <strong>Wait {remainingMinutes} minutes</strong> - Account unlocks automatically</p>
                                                <p style='margin: 10px 0; font-size: 15px;'>🔑 <strong>Reset your password</strong> - Unlock immediately</p>
                                                <p style='margin: 10px 0; font-size: 15px;'>📧 <strong>Contact support</strong> - If you need assistance</p>
                                            </div>
                                        </div>
                                    </div>
                                    <div style='text-align: center; margin: 30px 0;'>
                                        <a href='{_emailSettings.ApplicationUrl}/forgot-password' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 40px; text-decoration: none; border-radius: 25px; display: inline-block; font-weight: 600; font-size: 16px; box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);'>Reset Password Now →</a>
                                    </div>
                                    <div style='background-color: #f8d7da; border-left: 4px solid #dc3545; padding: 15px; margin: 30px 0; border-radius: 4px;'>
                                        <p style='color: #721c24; margin: 0 0 10px 0; font-size: 14px; font-weight: 600;'>
                                            ⚠️ Didn't Try to Login?
                                        </p>
                                        <p style='color: #721c24; margin: 0; font-size: 13px;'>
                                            If you didn't attempt to log in, someone may be trying to access your account. We recommend resetting your password immediately to secure your account.
                                        </p>
                                    </div>
                                    <p style='color: #777; font-size: 13px; line-height: 1.6; margin: 20px 0 0 0; text-align: center;'>
                                        This is an automated security measure to protect your account from unauthorized access.
                                    </p>
                                </div>
                                <div style='background-color: #f8f9fa; padding: 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                                    <p style='color: #6c757d; font-size: 14px; margin: 0 0 10px 0;'>
                                        Need help? Contact us at <a href='mailto:{_emailSettings.SenderEmail}' style='color: #667eea; text-decoration: none;'>{_emailSettings.SenderEmail}</a>
                                    </p>
                                    <p style='color: #6c757d; font-size: 12px; margin: 0;'>
                                        © {DateTime.Now.Year} JobIntel. All rights reserved.
                                    </p>
                                </div>
                            </div>
                        </body>
                        </html>",
                    TextBody = $@"Account Temporarily Locked - JobIntel

Hi {firstName},

Your JobIntel account has been temporarily locked due to 5 consecutive failed login attempts.

⏱️ Lockout Details:
Your account will automatically unlock in {remainingMinutes} minutes (at {unlockTimeLocal}).

What You Can Do:
⏳ Wait {remainingMinutes} minutes - Account unlocks automatically
🔑 Reset your password - Unlock immediately: {_emailSettings.ApplicationUrl}/forgot-password
📧 Contact support - If you need assistance

⚠️ Didn't Try to Login?
If you didn't attempt to log in, someone may be trying to access your account. We recommend resetting your password immediately to secure your account.

This is an automated security measure to protect your account from unauthorized access.

Need help? Contact us at {_emailSettings.SenderEmail}

Best regards,
The JobIntel Security Team

© {DateTime.Now.Year} JobIntel. All rights reserved."
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Account locked notification sent successfully to: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send account locked notification to {email}: {ex.Message}");
                return false;
            }
        }
    }
}
