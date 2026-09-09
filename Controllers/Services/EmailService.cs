using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using VotingSystem.Configuration;
using VotingSystem.Models.Domain;

namespace VotingSystem.Controllers.Services
{
    /// <summary>
    /// Delivers the transactional emails the voting flow needs by POSTing them to
    /// a configured webhook (<c>EmailWebhook:Url</c>). Each request body is JSON
    /// with <c>party_email</c>, <c>party_subject</c> and <c>party_html</c> (the
    /// HTML content carrying the link). When the webhook is not configured nothing
    /// is dispatched; the caller still has the link to surface in the admin UI,
    /// and it is written to the log.
    /// </summary>
    public sealed class EmailService
    {
        private readonly EmailWebhookSettings _settings;
        private readonly HttpClient _http;
        private readonly EmailNotifier _notifier;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailWebhookSettings> settings,
            HttpClient http,
            EmailNotifier notifier,
            ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _http = http;
            _notifier = notifier;
            _logger = logger;
        }

        public bool IsConfigured => _settings.IsConfigured;

        public string BaseUrl => _settings.AppBaseUrl.TrimEnd('/');

        public Task<bool> SendLeaderLinkAsync(Partylist partylist, string electionTitle, string link)
        {
            var (subject, body) = EmailTemplates.LeaderLink(partylist, electionTitle, link);
            return SendAsync(partylist.LeaderEmail, subject, body);
        }

        public Task<bool> SendBallotLinkAsync(Voter voter, string electionTitle, string link)
        {
            var (subject, body) = EmailTemplates.BallotLink(voter, electionTitle, link);
            return SendAsync(voter.Email, subject, body);
        }

        public Task<bool> SendInvitationAsync(Voter voter, string electionTitle, string link)
        {
            var (subject, body) = EmailTemplates.Invitation(voter, electionTitle, link);
            return SendAsync(voter.Email, subject, body);
        }

        /// <summary>
        /// POSTs the email to the configured webhook as
        /// <c>{ "party_email", "party_subject", "party_html" }</c>. Returns
        /// <c>true</c> only when the webhook is configured and answers 2xx.
        /// </summary>
        public async Task<bool> SendAsync(string to, string subject, string htmlBody)
        {
            if (!_settings.IsConfigured)
            {
                _logger.LogWarning(
                    "Email not dispatched (webhook not configured). To: {To}; Subject: {Subject}. Body: {Body}",
                    to, subject, htmlBody);
                return false;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, _settings.Url)
                {
                    Content = JsonContent.Create(new
                    {
                        party_email = to,
                        party_subject = subject,
                        party_html = htmlBody
                    })
                };

                if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
                }

                using var response = await _http.SendAsync(request);
                var rawBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _notifier.Failed(to, subject, $"HTTP {(int)response.StatusCode}");
                    return false;
                }

                _notifier.Sent(to, subject, ExtractMessage(rawBody));
                return true;
            }
            catch (Exception ex)
            {
                _notifier.Failed(to, subject, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Pulls the <c>message</c> string from a webhook JSON body such as
        /// <c>{ "message": "Email Sent!" }</c>. Returns <c>null</c> for an empty
        /// or non-JSON body.
        /// </summary>
        private static string? ExtractMessage(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Object
                    && doc.RootElement.TryGetProperty("message", out var message)
                    && message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString();
                }
            }
            catch (JsonException)
            {
                // A non-JSON body is fine — nothing to extract.
            }

            return null;
        }
    }
}
