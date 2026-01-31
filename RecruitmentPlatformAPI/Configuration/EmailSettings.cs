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
    }
}
