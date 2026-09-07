using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using VotingSystem.Configuration;
using VotingSystem.Models.Domain;

namespace VotingSystem.Controllers.Services
{
    /// <summary>
    /// Sends the transactional emails the voting flow needs. When SMTP is not
    /// configured (blank <c>MailSettings:Host</c>) nothing is sent; the caller
    /// still has the link to surface in the admin UI, and it is written to the log.
    /// </summary>
    public sealed class EmailService
    {
        private readonly MailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public bool IsConfigured => _settings.IsConfigured;

        public string BaseUrl => _settings.AppBaseUrl.TrimEnd('/');

        public Task<bool> SendLeaderLinkAsync(Partylist partylist, string electionTitle, string link)
        {
            var deadline = partylist.SubmissionDeadline is { } d
                ? d.ToLocalTime().ToString("MMMM d, yyyy h:mm tt")
                : "the announced deadline";

            var body = $"""
                <p>Hello {WebUtility.HtmlEncode(partylist.LeaderName)},</p>
                <p>You have been invited to submit the party list
                <strong>{WebUtility.HtmlEncode(partylist.Name)}</strong> for
                <strong>{WebUtility.HtmlEncode(electionTitle)}</strong>.</p>
                <p>Use the secure link below to fill in your party-list details and candidates.
                You can save and return to it any time before {WebUtility.HtmlEncode(deadline)}.</p>
                <p><a href="{link}">{link}</a></p>
                <p>No account is required. Do not share this link.</p>
                """;

            return SendAsync(partylist.LeaderEmail, $"Party-list submission — {electionTitle}", body);
        }

        public Task<bool> SendBallotLinkAsync(Voter voter, string electionTitle, string link)
        {
            var body = $"""
                <p>Hello {WebUtility.HtmlEncode(voter.FullName)},</p>
                <p>Your identity has been verified for <strong>{WebUtility.HtmlEncode(electionTitle)}</strong>.
                Continue to your ballot using the secure link below.</p>
                <p><a href="{link}">{link}</a></p>
                <p>This link is unique to you. Do not share it.</p>
                """;

            return SendAsync(voter.Email, $"Your ballot link — {electionTitle}", body);
        }

        public Task<bool> SendInvitationAsync(Voter voter, string electionTitle, string link)
        {
            var body = $"""
                <p>Hello {WebUtility.HtmlEncode(voter.FullName)},</p>
                <p>You are eligible to vote in <strong>{WebUtility.HtmlEncode(electionTitle)}</strong>.
                Begin here:</p>
                <p><a href="{link}">{link}</a></p>
                """;

            return SendAsync(voter.Email, $"You are invited to vote — {electionTitle}", body);
        }

        public async Task<bool> SendAsync(string to, string subject, string htmlBody)
        {
            if (!_settings.IsConfigured)
            {
                _logger.LogWarning(
                    "Email not sent (SMTP not configured). To: {To}; Subject: {Subject}. Body: {Body}",
                    to, subject, htmlBody);
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
                _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
                return false;
            }
        }
    }
}
