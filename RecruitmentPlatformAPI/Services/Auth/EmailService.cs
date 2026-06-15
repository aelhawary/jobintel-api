using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using RecruitmentPlatformAPI.Configuration;
using System.Text;
using System.Text.Json;

namespace RecruitmentPlatformAPI.Services.Auth
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(
            ILogger<EmailService> logger,
            IOptions<EmailSettings> emailSettings,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value;
            _httpClientFactory = httpClientFactory;
        }

        // ────────────────────────────────────────────────────────────────
        //  Core dispatch
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Unified email sender: uses Brevo HTTP API when UseHttpApi is true, otherwise SMTP.
        /// </summary>
        private async Task<bool> SendEmailAsync(
            string toEmail, string toName,
            string subject, string htmlBody, string textBody,
            string? replyToEmail = null, string? replyToName = null)
        {
            return _emailSettings.UseHttpApi
                ? await SendViaHttpApiAsync(toEmail, toName, subject, htmlBody, textBody, replyToEmail, replyToName)
                : await SendViaSmtpAsync(toEmail, toName, subject, htmlBody, textBody, replyToEmail, replyToName);
        }

        // ────────────────────────────────────────────────────────────────
        //  Transport layer
        // ────────────────────────────────────────────────────────────────

        private async Task<bool> SendViaHttpApiAsync(
            string toEmail, string toName,
            string subject, string htmlBody, string textBody,
            string? replyToEmail = null, string? replyToName = null)
        {
            try
            {
                _logger.LogInformation("Sending email via Brevo HTTP API to: {Email}", toEmail);

                var client = _httpClientFactory.CreateClient();

                string jsonContent;
                if (!string.IsNullOrEmpty(replyToEmail))
                {
                    var requestBodyWithReplyTo = new
                    {
                        sender = new { name = _emailSettings.SenderName, email = _emailSettings.SenderEmail },
                        to = new[] { new { email = toEmail, name = toName } },
                        replyTo = new { email = replyToEmail, name = replyToName ?? replyToEmail },
                        subject,
                        htmlContent = htmlBody,
                        textContent = textBody
                    };
                    jsonContent = JsonSerializer.Serialize(requestBodyWithReplyTo);
                }
                else
                {
                    var requestBody = new
                    {
                        sender = new { name = _emailSettings.SenderName, email = _emailSettings.SenderEmail },
                        to = new[] { new { email = toEmail, name = toName } },
                        subject,
                        htmlContent = htmlBody,
                        textContent = textBody
                    };
                    jsonContent = JsonSerializer.Serialize(requestBody);
                }

                var content = new StringContent(
                    jsonContent,
                    Encoding.UTF8,
                    "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
                request.Headers.Add("api-key", _emailSettings.BrevoApiKey);
                request.Content = content;

                var response     = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully via Brevo HTTP API to: {Email}", toEmail);
                    return true;
                }

                _logger.LogError(
                    "Brevo HTTP API failed. Status: {Status}. Response: {Response}",
                    response.StatusCode, responseBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send email via Brevo HTTP API to {Email}", toEmail);
                return false;
            }
        }

        private async Task<bool> SendViaSmtpAsync(
            string toEmail, string toName,
            string subject, string htmlBody, string textBody,
            string? replyToEmail = null, string? replyToName = null)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                
                if (!string.IsNullOrEmpty(replyToEmail))
                {
                    message.ReplyTo.Add(new MailboxAddress(replyToName ?? replyToEmail, replyToEmail));
                }
                
                message.Subject = subject;
                message.Body   = new BodyBuilder { HtmlBody = htmlBody, TextBody = textBody }.ToMessageBody();

                using var smtp = new SmtpClient();

                _logger.LogInformation(
                    "Connecting to SMTP server: {Server}:{Port}",
                    _emailSettings.SmtpServer, _emailSettings.SmtpPort);

                await smtp.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully via SMTP to: {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send email via SMTP to {Email}. Server: {Server}:{Port}",
                    toEmail, _emailSettings.SmtpServer, _emailSettings.SmtpPort);
                return false;
            }
        }

        // ────────────────────────────────────────────────────────────────
        //  HTML wrapper — email-client-safe, responsive, accessible
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Wraps specific email content inside a standard, modern Job Intel branded HTML shell.
        /// Built to pass Litmus / Email on Acid checks:
        ///   - table-based layout for Outlook compatibility
        ///   - inline styles only (no &lt;style&gt; blocks — Gmail strips them)
        ///   - system font stack fallback (Google Fonts blocked in many clients)
        ///   - role="presentation" on layout tables
        ///   - aria-hidden decorative elements
        ///   - preheader text for inbox preview
        ///   - 600 px max-width with 100 % fluid fallback
        /// </summary>
        private string GetEmailHtmlWrapper(string contentHtml, string preheaderText = "")
        {
            var year          = DateTime.UtcNow.Year;
            var supportEmail  = _emailSettings.SenderEmail;

            // Sanitise: never let untrusted data break the interpolation
            var safePreheader = System.Net.WebUtility.HtmlEncode(preheaderText);

            const string responsiveCss = @"
                .stat-row { display:table; width:100%; margin:0 0 28px 0; }
                .stat-box { display:table-cell; width:48%; background-color:#f8f5f2; border:1px solid #e2ddd6; padding:24px 16px; border-radius:12px; text-align:center; vertical-align:top; }
                .stat-spacer { display:table-cell; width:4%; }
                @media only screen and (max-width:480px) {
                    .stat-row { display:block; width:100%; }
                    .stat-box { display:block; width:100%; margin-bottom:12px; }
                    .stat-spacer { display:none; }
                }";

            return $"""
                <!DOCTYPE html>
                <html lang="en" xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <meta http-equiv="X-UA-Compatible" content="IE=edge">
                    <meta name="x-apple-disable-message-reformatting">
                    <meta name="format-detection" content="telephone=no,address=no,email=no,date=no">
                    <title>Job Intel</title>
                    <!--[if mso]>
                    <noscript>
                        <xml>
                            <o:OfficeDocumentSettings>
                                <o:PixelsPerInch>96</o:PixelsPerInch>
                            </o:OfficeDocumentSettings>
                        </xml>
                    </noscript>
                    <![endif]-->
                    <style type="text/css">
                        {responsiveCss}
                    </style>
                </head>
                <body style="margin:0;padding:0;word-spacing:normal;background-color:#f0ece6;">

                    <!-- Preheader: visible in inbox preview, hidden in email body -->
                    <div aria-hidden="true" style="display:none;font-size:1px;line-height:1px;max-height:0;max-width:0;opacity:0;overflow:hidden;mso-hide:all;">{safePreheader}&zwnj;&nbsp;&zwnj;&nbsp;&zwnj;&nbsp;&zwnj;&nbsp;&zwnj;&nbsp;&zwnj;&nbsp;&zwnj;&nbsp;&zwnj;&nbsp;</div>

                    <!-- Outer wrapper -->
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f0ece6;">
                    <tr>
                        <td align="center" style="padding:24px 16px;">

                            <!-- Email card — 600 px desktop, 100 % on mobile -->
                            <!--[if mso]>
                            <table role="presentation" width="600" cellspacing="0" cellpadding="0" border="0"><tr><td>
                            <![endif]-->
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0"
                                style="max-width:600px;background-color:#ffffff;border-radius:12px;border:1px solid #e2ddd6;overflow:hidden;">

                                <!-- ── Header / Brand ── -->
                                <tr>
                                    <td align="center"
                                        style="background-color:#fa7b05;padding:36px 24px;mso-padding-alt:36px 24px;">
                                        <!--[if mso]><table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%"><tr><td align="center"><![endif]-->
                                        <h1 style="color:#ffffff;margin:0;font-size:30px;font-weight:800;letter-spacing:-0.5px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-line-height-rule:exactly;line-height:1.2;">
                                            Job Intel
                                        </h1>
                                        <p style="color:rgba(255,255,255,0.92);margin:10px 0 0 0;font-size:14px;font-weight:500;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-line-height-rule:exactly;line-height:1.4;">
                                            Empowering Careers through Data Intelligence
                                        </p>
                                        <!--[if mso]></td></tr></table><![endif]-->
                                    </td>
                                </tr>

                                <!-- ── Dynamic Content ── -->
                                <tr>
                                    <td style="padding:36px 32px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-padding-alt:36px 32px;">
                                        {contentHtml}
                                    </td>
                                </tr>

                                <!-- ── Footer ── -->
                                <tr>
                                    <td style="background-color:#f8f5f2;padding:28px 32px;border-top:1px solid #e2ddd6;mso-padding-alt:28px 32px;">
                                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0">
                                            <tr>
                                                <td align="center" style="padding-bottom:12px;">
                                                    <p style="color:#64748b;font-size:13px;font-weight:500;margin:0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-line-height-rule:exactly;line-height:1.5;">
                                                        Need help? <a href="mailto:{supportEmail}" style="color:#fa7b05;text-decoration:none;font-weight:600;">{supportEmail}</a>
                                                    </p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="center" style="padding-bottom:12px;">
                                                    <a href="#" style="color:#94a3b8;text-decoration:none;font-size:12px;font-weight:500;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">Privacy Policy</a>
                                                    <span style="color:#cbd5e1;margin:0 8px;" aria-hidden="true">&bull;</span>
                                                    <a href="#" style="color:#94a3b8;text-decoration:none;font-size:12px;font-weight:500;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">Terms of Service</a>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="center">
                                                    <p style="color:#94a3b8;font-size:12px;margin:0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                                                        &copy; {year} Job Intel. All rights reserved.
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>

                            </table>
                            <!--[if mso]></td></tr></table><![endif]-->

                        </td>
                    </tr>
                    </table>

                </body>
                </html>
                """;
        }



        // ────────────────────────────────────────────────────────────────
        //  Shared HTML primitives (keeps individual templates DRY)
        // ────────────────────────────────────────────────────────────────

        private static string WarningBox(string iconHtml, string text) => $"""
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:28px 0;">
            <tr>
                <td style="background-color:#fffbeb;border-left:4px solid #f59e0b;padding:16px 20px;border-radius:0 6px 6px 0;">
                    <p style="color:#92400e;margin:0;font-size:14px;font-weight:500;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-line-height-rule:exactly;line-height:1.5;">
                        {iconHtml} {text}
                    </p>
                </td>
            </tr>
            </table>
            """;
        private static string PrimaryButton(string href, string label) => $"""
            <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:32px auto;">
            <tr>
                <td align="center" style="border-radius:8px;background-color:#fa7b05;">
                    <!--[if mso]><v:roundrect xmlns:v="urn:schemas-microsoft-com:vml" href="{href}" style="height:48px;v-text-anchor:middle;width:200px;" arcsize="17%" stroke="f" fillcolor="#fa7b05"><w:anchorlock/><center style="color:#ffffff;font-family:Arial,sans-serif;font-size:15px;font-weight:700;">{label}</center></v:roundrect><![endif]-->
                    <!--[if !mso]><!-->
                    <a href="{href}"
                       style="display:inline-block;background-color:#fa7b05;color:#ffffff;text-decoration:none;padding:14px 32px;font-size:15px;font-weight:700;border-radius:8px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-hide:all;">
                        {label}
                    </a>
                    <!--<![endif]-->
                </td>
            </tr>
            </table>
            """;



        // ────────────────────────────────────────────────────────────────
        //  Public email builders
        // ────────────────────────────────────────────────────────────────

        public async Task<bool> SendVerificationEmailAsync(
            string email, string firstName, string verificationCode)
        {
            try
            {
                const string subject = "Verify Your Email – Job Intel";
                var encodedName = System.Net.WebUtility.HtmlEncode(firstName);
                var encodedCode = System.Net.WebUtility.HtmlEncode(verificationCode);

                var innerHtml = $"""
                    <h2 style="color:#1e293b;margin:0 0 20px 0;font-size:22px;font-weight:700;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-line-height-rule:exactly;line-height:1.3;">
                        Welcome aboard, {encodedName}!
                    </h2>
                    <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 16px 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        We're thrilled to have you join Job Intel. You're just one step away from unlocking a world of
                        career opportunities powered by AI.
                    </p>
                    <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 28px 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        Please verify your email address using the code below:
                    </p>

                    <!-- Verification code box -->
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:0 0 28px 0;">
                    <tr>
                        <td align="center" style="background-color:#f8f5f2;border:1px solid #e2ddd6;padding:28px 20px;border-radius:12px;">
                            <p style="color:#64748b;margin:0 0 12px 0;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:1.5px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                                Your Verification Code
                            </p>
                            <!-- Inner white card -->
                            <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="background-color:#ffffff;border:1px solid #e2ddd6;border-radius:8px;">
                            <tr>
                                <td style="padding:16px 28px;">
                                    <span style="color:#fa7b05;font-size:34px;font-weight:800;letter-spacing:10px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-line-height-rule:exactly;line-height:1;">
                                        {encodedCode}
                                    </span>
                                </td>
                            </tr>
                            </table>
                        </td>
                    </tr>
                    </table>

                    {WarningBox("<strong>Important:</strong>", "This code expires in <strong>15 minutes</strong> for your security.")}

                    <p style="color:#94a3b8;font-size:13px;line-height:1.6;margin:20px 0 0 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        If you didn't create a Job Intel account, you can safely ignore this email.
                    </p>
                    """;

                var textBody = $"""
                    Welcome to Job Intel!

                    Hi {firstName},

                    We're thrilled to have you join Job Intel — Your Intelligent Career Partner!

                    Your verification code is: {verificationCode}

                    This code expires in 15 minutes for your security.

                    If you didn't create a Job Intel account, you can safely ignore this email.

                    Need help? Contact us at {_emailSettings.SenderEmail}

                    Best regards,
                    The Job Intel Team
                    """;

                return await SendEmailAsync(
                    email, firstName, subject,
                    GetEmailHtmlWrapper(innerHtml, $"Your verification code is {verificationCode} — expires in 15 minutes."),
                    textBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
        {
            try
            {
                const string subject = "Your Job Intel Account is Active!";
                var encodedName    = System.Net.WebUtility.HtmlEncode(firstName);
                var dashboardUrl   = _emailSettings.ApplicationUrl;

                var innerHtml = $"""
                    <p style="text-align:center;font-size:44px;margin:0 0 20px 0;" aria-hidden="true">&#10003;</p>
                    <h2 style="color:#10b981;margin:0 0 16px 0;font-size:22px;text-align:center;font-weight:800;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-line-height-rule:exactly;line-height:1.3;">
                        Account Verified Successfully!
                    </h2>
                    <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 28px 0;text-align:center;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        Congratulations, <strong style="color:#1e293b;">{encodedName}</strong>! Your email has been verified
                        and your account is now active.
                    </p>

                    <!-- Next steps -->
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:0 0 28px 0;">
                    <tr>
                        <td style="background-color:#f8f5f2;border:1px solid #e2ddd6;padding:24px 28px;border-radius:12px;">
                            <h3 style="color:#1e293b;margin:0 0 14px 0;font-size:16px;font-weight:700;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                                Your Next Steps
                            </h3>
                            <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%">
                                <tr>
                                    <td width="24" valign="top" style="color:#fa7b05;font-size:18px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">1.</td>
                                    <td style="color:#475569;font-size:14px;padding-bottom:10px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;line-height:1.5;">
                                        <strong>Complete your profile</strong> to highlight your unique skills.
                                    </td>
                                </tr>
                                <tr>
                                    <td width="24" valign="top" style="color:#fa7b05;font-size:18px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">2.</td>
                                    <td style="color:#475569;font-size:14px;padding-bottom:10px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;line-height:1.5;">
                                        <strong>Take an assessment</strong> to verify your expertise.
                                    </td>
                                </tr>
                                <tr>
                                    <td width="24" valign="top" style="color:#fa7b05;font-size:18px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">3.</td>
                                    <td style="color:#475569;font-size:14px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;line-height:1.5;">
                                        <strong>Sit back</strong> while our AI matches you with top recruiters.
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    </table>

                    {PrimaryButton(dashboardUrl, "Go to Dashboard &rarr;")}
                    """;

                var textBody = $"""
                    Welcome to Job Intel!

                    Hi {firstName},

                    Congratulations! Your email has been verified and your account is now active.

                    Your Next Steps:
                    1. Complete your profile to highlight your unique skills.
                    2. Take an assessment to verify your expertise.
                    3. Sit back while our AI matches you with top recruiters.

                    Get started now: {dashboardUrl}

                    Need help? Contact us at {_emailSettings.SenderEmail}

                    Best regards,
                    The Job Intel Team
                    """;

                return await SendEmailAsync(
                    email, firstName, subject,
                    GetEmailHtmlWrapper(innerHtml, "Your account is now active — get started on Job Intel."),
                    textBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetLinkAsync(
            string email, string firstName, string resetToken)
        {
            try
            {
                const string subject = "Reset Your Job Intel Password";
                var encodedName = System.Net.WebUtility.HtmlEncode(firstName);

                // Build the reset link URL
                var resetLink        = $"{_emailSettings.FrontendUrl}/reset-password?token={resetToken}";
                var encodedResetLink = System.Net.WebUtility.HtmlEncode(resetLink);

                var innerHtml = $"""
                    <p style="text-align:center;font-size:44px;margin:0 0 20px 0;" aria-hidden="true">&#128274;</p>
                    <h2 style="color:#1e293b;margin:0 0 20px 0;font-size:22px;text-align:center;font-weight:800;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-line-height-rule:exactly;line-height:1.3;">
                        Password Reset Request
                    </h2>
                    <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 12px 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        Hi <strong style="color:#1e293b;">{encodedName}</strong>,
                    </p>
                    <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 28px 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        We received a request to reset the password for your Job Intel account.
                        Click the button below to securely set a new password:
                    </p>

                    {PrimaryButton(resetLink, "Reset My Password")}

                    <p style="color:#64748b;font-size:13px;text-align:center;margin:4px 0 12px 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        Or copy and paste this link into your browser:
                    </p>
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:0 0 28px 0;">
                    <tr>
                        <td style="background-color:#f1f5f9;padding:14px 16px;border-radius:8px;border:1px solid #e2e8f0;word-break:break-all;">
                            <a href="{encodedResetLink}" style="color:#0ea5e9;font-size:12px;text-decoration:none;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">{encodedResetLink}</a>
                        </td>
                    </tr>
                    </table>

                    {WarningBox("<strong>Time-Sensitive:</strong>", "This link expires in <strong>15 minutes</strong>.")}

                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:28px 0 0 0;">
                    <tr>
                        <td style="background-color:#f8fafc;border:1px solid #e2e8f0;padding:20px 24px;border-radius:8px;">
                            <p style="color:#1e293b;margin:0 0 8px 0;font-size:14px;font-weight:700;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                                Didn't request this?
                            </p>
                            <p style="color:#475569;font-size:13px;margin:0;line-height:1.5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                                You can safely ignore this email. Your account remains secure and your password will not change.
                            </p>
                        </td>
                    </tr>
                    </table>
                    """;

                var textBody = $"""
                    Job Intel — Password Reset Request

                    Hi {firstName},

                    We received a request to reset the password for your Job Intel account.

                    Reset your password here:
                    {resetLink}

                    This link expires in 15 minutes for your security.

                    Didn't request this? You can safely ignore this email. Your account is secure.

                    Need help? Contact us at {_emailSettings.SenderEmail}

                    Best regards,
                    The Job Intel Team
                    """;

                return await SendEmailAsync(
                    email, firstName, subject,
                    GetEmailHtmlWrapper(innerHtml, "Reset your Job Intel password — link expires in 15 minutes."),
                    textBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset link to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendAccountLockedEmailAsync(
            string email, string firstName, DateTime lockoutEnd, string resetToken)
        {
            try
            {
                const string subject = "Security Alert: Your Account Has Been Temporarily Locked";

                var remainingTime    = lockoutEnd - DateTime.UtcNow;
                int remainingMinutes = Math.Max(1, (int)Math.Ceiling(remainingTime.TotalMinutes));
                // Display in UTC to avoid ambiguity; clients can convert
                var unlockTimeUtc    = lockoutEnd.ToString("HH:mm 'UTC'");

                var encodedName      = System.Net.WebUtility.HtmlEncode(firstName);
                var resetLink        = $"{_emailSettings.FrontendUrl}/reset-password?token={resetToken}";

                var innerHtml = $"""
                    <p style="text-align:center;font-size:44px;margin:0 0 20px 0;" aria-hidden="true">&#128274;</p>
                    <h2 style="color:#ef4444;margin:0 0 20px 0;font-size:22px;text-align:center;font-weight:800;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-line-height-rule:exactly;line-height:1.3;">
                        Account Temporarily Locked
                    </h2>
                    <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 12px 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        Hi <strong style="color:#1e293b;">{encodedName}</strong>,
                    </p>
                    <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 28px 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        Your Job Intel account has been temporarily locked after <strong>5 consecutive failed login attempts</strong>.
                        This is an automated security measure to protect your account.
                    </p>

                    <!-- Lockout info panel -->
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:0 0 28px 0;">
                    <tr>
                        <td style="background-color:#fef2f2;border-left:4px solid #ef4444;padding:16px 20px;border-radius:0 6px 6px 0;">
                            <p style="color:#b91c1c;margin:0 0 6px 0;font-size:14px;font-weight:700;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                                Lockout Details
                            </p>
                            <p style="color:#b91c1c;margin:0;font-size:13px;line-height:1.5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                                Your account will unlock automatically in <strong>{remainingMinutes} minute{(remainingMinutes == 1 ? "" : "s")}</strong>
                                (at {unlockTimeUtc}).
                            </p>
                        </td>
                    </tr>
                    </table>

                    <!-- Options table -->
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:0 0 28px 0;">
                    <tr>
                        <td style="background-color:#f8f5f2;border:1px solid #e2ddd6;padding:22px 28px;border-radius:12px;">
                            <h3 style="color:#1e293b;margin:0 0 14px 0;font-size:15px;font-weight:700;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                                What You Can Do
                            </h3>
                            <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%">
                                <tr>
                                    <td width="24" valign="top" style="color:#fa7b05;font-size:18px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">&#8226;</td>
                                    <td style="color:#475569;font-size:14px;padding-bottom:10px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;line-height:1.5;">
                                        <strong>Wait {remainingMinutes} minute{(remainingMinutes == 1 ? "" : "s")}:</strong> Your account will unlock automatically.
                                    </td>
                                </tr>
                                <tr>
                                    <td width="24" valign="top" style="color:#fa7b05;font-size:18px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">&#8226;</td>
                                    <td style="color:#475569;font-size:14px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;line-height:1.5;">
                                        <strong>Reset your password</strong> to regain access immediately.
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    </table>

                    {PrimaryButton(resetLink, "Reset Password Now &rarr;")}

                    <p style="color:#64748b;font-size:13px;line-height:1.6;margin:20px 0 0 0;text-align:center;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        If you didn't attempt to log in, someone may be trying to access your account.
                        We strongly recommend resetting your password immediately.
                    </p>
                    """;

                var textBody = $"""
                    Account Temporarily Locked — Job Intel

                    Hi {firstName},

                    Your Job Intel account has been temporarily locked after 5 consecutive failed login attempts.

                    Lockout Details:
                    Your account will unlock automatically in {remainingMinutes} minute{(remainingMinutes == 1 ? "" : "s")} (at {unlockTimeUtc}).

                    What You Can Do:
                    - Wait {remainingMinutes} minute{(remainingMinutes == 1 ? "" : "s")} — account unlocks automatically.
                    - Reset your password to unlock immediately: {resetLink}

                    Didn't try to log in?
                    Someone may be attempting to access your account. Reset your password immediately to stay secure.

                    Need help? Contact us at {_emailSettings.SenderEmail}

                    Best regards,
                    The Job Intel Security Team
                    """;

                return await SendEmailAsync(
                    email, firstName, subject,
                    GetEmailHtmlWrapper(innerHtml, "Security alert: your Job Intel account has been temporarily locked."),
                    textBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send account locked notification to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendWeeklyDigestAsync(string email, string firstName, int searchAppearances, int profileViews, int recommendations)
        {
            const string subject = "Your Weekly Job Intel Profile Stats";
            var encodedName = System.Net.WebUtility.HtmlEncode(firstName);
            var dashboardUrl = _emailSettings.ApplicationUrl;

            var innerHtml = $"""
                <h2 style="color:#1e293b;margin:0 0 20px 0;font-size:22px;text-align:center;font-weight:800;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-line-height-rule:exactly;line-height:1.3;">
                    Your Weekly Profile Stats
                </h2>
                <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 12px 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                    Hi <strong style="color:#1e293b;">{encodedName}</strong>,
                </p>
                <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 28px 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                    Here's how your profile performed on Job Intel this week. Keep your profile updated to increase your visibility to top recruiters.
                </p>

                <!-- Stats grid -->
                <div class="stat-row">
                    <div class="stat-box">
                        <span style="display:block;font-size:32px;font-weight:800;color:#fa7b05;margin-bottom:8px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                            {searchAppearances}
                        </span>
                        <span style="display:block;color:#64748b;font-size:13px;font-weight:600;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                            Search Appearances
                        </span>
                    </div>
                    <div class="stat-spacer"></div>
                    <div class="stat-box">
                        <span style="display:block;font-size:32px;font-weight:800;color:#10b981;margin-bottom:8px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                            {profileViews}
                        </span>
                        <span style="display:block;color:#64748b;font-size:13px;font-weight:600;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                            Profile Views
                        </span>
                    </div>
                </div>

                {PrimaryButton(dashboardUrl, "View Full Dashboard &rarr;")}

                <p style="color:#64748b;font-size:13px;line-height:1.6;margin:20px 0 0 0;text-align:center;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                    Did you know? Completing your profile and adding new skills can increase your profile views by up to 300%.
                </p>
                """;

            var textBody = $"""
                Your Weekly Job Intel Profile Stats

                Hi {firstName},

                Here's how your profile performed on Job Intel this week:
                - Search Appearances: {searchAppearances}
                - Profile Views: {profileViews}

                View your full dashboard: {dashboardUrl}

                Keep your profile updated to increase your visibility to top recruiters.

                Best regards,
                The Job Intel Team
                """;

            // Retry up to 2 additional times for transient failures (3 total attempts)
            for (int attempt = 0; attempt <= 2; attempt++)
            {
                try
                {
                    var sent = await SendEmailAsync(
                        email, firstName, subject,
                        GetEmailHtmlWrapper(innerHtml, $"Your profile had {searchAppearances} search appearances and {profileViews} profile views this week."),
                        textBody);

                    if (sent) return true;

                    if (attempt < 2)
                    {
                        _logger.LogWarning(
                            "Weekly digest email to {Email} failed (attempt {Attempt}/3). Retrying in {Delay}s...",
                            email, attempt + 1, (attempt + 1) * 2);
                        await Task.Delay(TimeSpan.FromSeconds((attempt + 1) * 2));
                    }
                }
                catch (Exception ex) when (attempt < 2)
                {
                    _logger.LogWarning(ex,
                        "Weekly digest email to {Email} errored (attempt {Attempt}/3). Retrying...",
                        email, attempt + 1);
                    await Task.Delay(TimeSpan.FromSeconds((attempt + 1) * 2));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send weekly digest email to {Email} after 3 attempts", email);
                    return false;
                }
            }

            _logger.LogError("Failed to send weekly digest email to {Email} after 3 attempts", email);
            return false;
        }

        // ────────────────────────────────────────────────────────────────
        //  Token / code generators
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates a cryptographically secure 6-digit verification code.
        /// Uses rejection sampling to avoid modulo bias.
        /// </summary>
        public string GenerateVerificationCode()
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            Span<byte> buffer = stackalloc byte[4];
            uint value;

            // Rejection sampling: discard values in the biased tail of uint range
            const uint maxUnbiased = uint.MaxValue - (uint.MaxValue % 900_000) - 1;
            do
            {
                rng.GetBytes(buffer);
                value = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(buffer);
            }
            while (value > maxUnbiased);

            return (100_000 + value % 900_000).ToString();
        }

        /// <summary>
        /// Generates a cryptographically secure URL-safe Base64 token (64 characters).
        /// </summary>
        public string GenerateSecureToken()
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[48]; // 48 bytes → 64 Base64 chars
            rng.GetBytes(bytes);

            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        // ────────────────────────────────────────────────────────────────
        //  Contact Candidate
        // ────────────────────────────────────────────────────────────────

        public async Task<bool> SendContactEmailAsync(
            string candidateEmail, string candidateFirstName,
            string recruiterEmail, string recruiterFirstName, string recruiterLastName,
            string recruiterCompany, string jobTitle, string message)
        {
            try
            {
                const string subject = "A Recruiter Wants to Connect with You on Job Intel";
                var encodedCandidateName = System.Net.WebUtility.HtmlEncode(candidateFirstName);
                var encodedRecruiterName = System.Net.WebUtility.HtmlEncode($"{recruiterFirstName} {recruiterLastName}");
                var encodedCompany = System.Net.WebUtility.HtmlEncode(recruiterCompany);
                var encodedJobTitle = System.Net.WebUtility.HtmlEncode(jobTitle);
                var encodedMessage = System.Net.WebUtility.HtmlEncode(message);
                
                var mailtoLink = $"mailto:{recruiterEmail}?subject={System.Uri.EscapeDataString($"Re: {jobTitle} at {recruiterCompany}")}";

                var innerHtml = $"""
                    <p style="text-align:center;font-size:44px;margin:0 0 20px 0;" aria-hidden="true">&#9993;</p>
                    <h2 style="color:#1e293b;margin:0 0 16px 0;font-size:22px;text-align:center;font-weight:800;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;mso-line-height-rule:exactly;line-height:1.3;">
                        New Message from a Recruiter
                    </h2>
                    <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 28px 0;text-align:center;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        Hi <strong style="color:#1e293b;">{encodedCandidateName}</strong>,
                    </p>
                    <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 28px 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        <strong style="color:#1e293b;">{encodedRecruiterName}</strong> from
                        <strong style="color:#1e293b;">{encodedCompany}</strong> is interested in connecting
                        with you regarding the <strong style="color:#fa7b05;">{encodedJobTitle}</strong> position.
                    </p>

                    <!-- Recruiter Message -->
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:0 0 28px 0;">
                    <tr>
                        <td style="background-color:#f8f5f2;border:1px solid #e2ddd6;border-left:4px solid #fa7b05;padding:24px 28px;border-radius:0 12px 12px 0;">
                            <p style="color:#64748b;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:0.5px;margin:0 0 10px 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                                Message from {encodedRecruiterName}
                            </p>
                            <p style="color:#1e293b;font-size:15px;line-height:1.65;margin:0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;white-space:pre-wrap;">{encodedMessage}</p>
                        </td>
                    </tr>
                    </table>

                    <p style="color:#475569;font-size:15px;line-height:1.6;margin:0 0 28px 0;text-align:center;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                        You can reply directly to this email to get in touch with the recruiter, or click the button below.
                    </p>

                    {PrimaryButton(mailtoLink, "Reply to Recruiter")}

                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:8px 0 0 0;">
                    <tr>
                        <td align="center">
                            <a href="{_emailSettings.FrontendUrl}/employee/notifications"
                               style="color:#64748b;font-size:13px;font-weight:500;text-decoration:underline;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                                View all notifications
                            </a>
                        </td>
                    </tr>
                    </table>
                    """;

                var textBody = $"""
                    Hi {candidateFirstName},

                    {recruiterFirstName} {recruiterLastName} from {recruiterCompany} is interested in connecting with you regarding the {jobTitle} position.

                    Message from {recruiterFirstName}:
                    {message}

                    ---
                    You can reply directly to this email to get in touch with the recruiter, or use the following email address:
                    {recruiterEmail}

                    Or view your dashboard: {_emailSettings.FrontendUrl}/employee

                    Need help? Contact us at {_emailSettings.SenderEmail}

                    Best regards,
                    The Job Intel Team
                    """;

                return await SendEmailAsync(
                    candidateEmail, candidateFirstName, subject,
                    GetEmailHtmlWrapper(innerHtml, $"{recruiterFirstName} {recruiterLastName} from {recruiterCompany} wants to connect with you about the {jobTitle} role."),
                    textBody,
                    recruiterEmail, $"{recruiterFirstName} {recruiterLastName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact email to {Email} from recruiter", candidateEmail);
                return false;
            }
        }
    }
}