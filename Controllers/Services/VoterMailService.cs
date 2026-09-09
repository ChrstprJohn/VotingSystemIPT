using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using VotingSystem.Configuration;
using VotingSystem.Models.Domain;

namespace VotingSystem.Controllers.Services
{
    /// <summary>
    /// Sends <em>voter</em>-facing email over SMTP (configured from the "Smtp"
    /// section). This is what delivers each voter their form when an election is
    /// published, and backs the admin "Email invitations" button.
    ///
    /// Party-list leader mail deliberately does NOT go through here — that keeps
    /// using <see cref="EmailService"/>'s webhook so the leader flow can stay on
    /// the webhook for testing. When SMTP is not configured nothing is sent; the
    /// attempt is logged and the caller is told.
    /// </summary>
    public sealed class VoterMailService
    {
        private readonly MailSettings _settings;
        private readonly EmailNotifier _notifier;
        private readonly ILogger<VoterMailService> _logger;

        public VoterMailService(
            IOptions<MailSettings> settings,
            EmailNotifier notifier,
            ILogger<VoterMailService> logger)
        {
            _settings = settings.Value;
            _notifier = notifier;
            _logger = logger;
        }

        public bool IsConfigured => _settings.IsConfigured;

        public string BaseUrl => _settings.AppBaseUrl.TrimEnd('/');

        /// <summary>Sends a voter the "you are invited to vote — here is your form" email.</summary>
        public Task<bool> SendInvitationAsync(Voter voter, string electionTitle, string link)
        {
            var (subject, body) = EmailTemplates.Invitation(voter, electionTitle, link);
            return SendAsync(voter.Email, subject, body);
        }

        /// <summary>
        /// Sends one HTML email over SMTP. Returns <c>true</c> only when SMTP is
        /// configured and the server accepted the message.
        /// </summary>
        public async Task<bool> SendAsync(string to, string subject, string htmlBody)
        {
            if (!_settings.IsConfigured)
            {
                _logger.LogWarning(
                    "Voter email not sent (SMTP not configured). To: {To}; Subject: {Subject}.",
                    to, subject);
                return false;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(
                        string.IsNullOrWhiteSpace(_settings.FromEmail) ? _settings.User : _settings.FromEmail,
                        _settings.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_settings.User, _settings.Password),
                    Timeout = 20000
                };

                await client.SendMailAsync(message);
                _notifier.Sent(to, subject, "SMTP accepted the message");
                return true;
            }
            catch (Exception ex)
            {
                _notifier.Failed(to, subject, ex.Message);
                return false;
            }
        }
    }
}
