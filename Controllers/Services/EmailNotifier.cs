namespace VotingSystem.Controllers.Services
{
    /// <summary>The outcome of a single webhook email-dispatch attempt.</summary>
    public sealed record EmailNotification(
        bool Ok,
        string Recipient,
        string Subject,
        string? Message,
        DateTime AtUtc);

    /// <summary>
    /// Records the outcome of every email dispatched through <see cref="EmailService"/>.
    /// Logs each attempt, keeps the most recent one on <see cref="Last"/>, and raises
    /// <see cref="Notified"/> so other components can react. The webhook is expected to
    /// answer 2xx with a JSON body such as <c>{ "message": "Email Sent!" }</c>; that
    /// message is captured here on success.
    /// </summary>
    public sealed class EmailNotifier
    {
        private readonly ILogger<EmailNotifier> _logger;

        public EmailNotifier(ILogger<EmailNotifier> logger)
        {
            _logger = logger;
        }

        /// <summary>The most recent dispatch outcome, or <c>null</c> before the first send.</summary>
        public EmailNotification? Last { get; private set; }

        public event EventHandler<EmailNotification>? Notified;

        /// <summary>Call when the webhook confirmed the send (2xx response).</summary>
        public void Sent(string recipient, string subject, string? message)
        {
            var note = new EmailNotification(true, recipient, subject, message, DateTime.UtcNow);
            Last = note;
            _logger.LogInformation(
                "Email webhook confirmed for {Recipient} ({Subject}): {Message}",
                recipient, subject, string.IsNullOrWhiteSpace(message) ? "(no message returned)" : message);
            Raise(note);
        }

        /// <summary>Call when the webhook was not reached or did not answer 2xx.</summary>
        public void Failed(string recipient, string subject, string reason)
        {
            var note = new EmailNotification(false, recipient, subject, reason, DateTime.UtcNow);
            Last = note;
            _logger.LogWarning(
                "Email webhook did not confirm for {Recipient} ({Subject}): {Reason}",
                recipient, subject, reason);
            Raise(note);
        }

        private void Raise(EmailNotification note)
        {
            try
            {
                Notified?.Invoke(this, note);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An EmailNotifier subscriber threw.");
            }
        }
    }
}
