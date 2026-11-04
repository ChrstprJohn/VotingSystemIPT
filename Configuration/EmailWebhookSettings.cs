namespace VotingSystem.Configuration
{
    /// <summary>
    /// Outbound webhook used to deliver the transactional emails the voting flow
    /// needs (party-list leader links, voter ballot links, election invitations).
    /// Bound from the "EmailWebhook" section. Leave <see cref="Url"/> blank to run
    /// without dispatching — links are still generated, logged, and shown in the
    /// admin UI.
    /// </summary>
    public sealed class EmailWebhookSettings
    {
        /// <summary>Absolute URL that receives the POSTed email payload.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Optional token sent as "Authorization: Bearer &lt;token&gt;" on every
        /// webhook request. Leave blank to send no auth header.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Absolute base URL used to build links in emails, e.g. "https://localhost:7027".
        /// </summary>
        public string AppBaseUrl { get; set; } = string.Empty;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Url);
    }
}
