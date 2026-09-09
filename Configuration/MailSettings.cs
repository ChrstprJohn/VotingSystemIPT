namespace VotingSystem.Configuration
{
    /// <summary>
    /// SMTP configuration for outbound <em>voter</em> email — the "here is your
    /// form" message sent to every voter when an election is published, plus
    /// admin-triggered invitation resends. Bound from the "Smtp" section.
    /// Party-list leader mail does NOT use this; it stays on the webhook in
    /// <see cref="VotingSystem.Controllers.Services.EmailService"/>.
    /// Leave <see cref="Host"/> blank to run without sending — publishing still
    /// works, and the send is skipped and logged.
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
        /// Absolute base URL used to build links in emails, e.g. "https://localhost:7027".
        /// </summary>
        public string AppBaseUrl { get; set; } = string.Empty;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Host)
            && !string.IsNullOrWhiteSpace(User)
            && !string.IsNullOrWhiteSpace(Password);
    }
}
