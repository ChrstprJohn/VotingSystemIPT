namespace VotingSystem.Configuration
{
    /// <summary>
    /// SMTP configuration for outbound email (party-list leader links, voter ballot
    /// links, election invitations). Bound from the "MailSettings" section.
    /// Leave <see cref="Host"/> blank to run without sending mail — links are still
    /// generated, logged, and shown in the admin UI.
    /// </summary>
    public sealed class MailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "QCU Voting System";
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// Absolute base URL used to build links in emails, e.g. "https://localhost:7233".
        /// </summary>
        public string AppBaseUrl { get; set; } = string.Empty;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Host)
            && !string.IsNullOrWhiteSpace(User)
            && !string.IsNullOrWhiteSpace(Password);
    }
}
