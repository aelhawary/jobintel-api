using RecruitmentPlatformAPI.DTOs;

namespace RecruitmentPlatformAPI.Configuration {
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string ApplicationUrl { get; set; } = "http://localhost:5217";
        /// <summary>
        /// Frontend URL for password reset links (e.g., http://localhost:3000)
        /// </summary>
        public string FrontendUrl { get; set; } = "http://localhost:3000";
        /// <summary>
        /// When true, use Brevo HTTP API (port 443) instead of SMTP (port 587).
        /// Required for hosts like MonsterASP that block outbound SMTP.
        /// </summary>
        public bool UseHttpApi { get; set; } = false;
        /// <summary>
        /// Brevo API key for HTTP API email sending.
        /// Generate from: Brevo Dashboard → Settings → SMTP &amp; API → API Keys tab
        /// </summary>
        public string BrevoApiKey { get; set; } = string.Empty;
    }
}
