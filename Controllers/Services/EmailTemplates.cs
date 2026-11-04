using System.Net;
using VotingSystem.Models.Domain;

namespace VotingSystem.Controllers.Services
{
    /// <summary>
    /// Single source of truth for the transactional email copy the voting flow
    /// sends. Both <see cref="EmailService"/> (webhook — used for party-list
    /// leaders) and <see cref="VoterMailService"/> (SMTP — used for voters) render
    /// their messages from here so the wording can never drift apart.
    /// </summary>
    public static class EmailTemplates
    {
        public static (string Subject, string Html) LeaderLink(
            Partylist partylist, string electionTitle, string link)
        {
            var deadline = partylist.SubmissionDeadline is { } d
                ? d.ToLocalTime().ToString("MMMM d, yyyy h:mm tt")
                : "the announced deadline";

            var html = $"""
                <p>Hello {WebUtility.HtmlEncode(partylist.LeaderName)},</p>
                <p>You have been invited to submit the party list
                <strong>{WebUtility.HtmlEncode(partylist.Name)}</strong> for
                <strong>{WebUtility.HtmlEncode(electionTitle)}</strong>.</p>
                <p>Use the secure link below to fill in your party-list details and candidates.
                You can save and return to it any time before {WebUtility.HtmlEncode(deadline)}.</p>
                <p><a href="{link}">{link}</a></p>
                <p>No account is required. Do not share this link.</p>
                """;

            return ($"Party-list submission — {electionTitle}", html);
        }

        public static (string Subject, string Html) BallotLink(
            Voter voter, string electionTitle, string link)
        {
            var html = $"""
                <p>Hello {WebUtility.HtmlEncode(voter.FullName)},</p>
                <p>Your identity has been verified for <strong>{WebUtility.HtmlEncode(electionTitle)}</strong>.
                Continue to your ballot using the secure link below.</p>
                <p><a href="{link}">{link}</a></p>
                <p>This link is unique to you. Do not share it.</p>
                """;

            return ($"Your ballot link — {electionTitle}", html);
        }

        public static (string Subject, string Html) Invitation(
            Voter voter, string electionTitle, string link)
        {
            var html = $"""
                <p>Hello {WebUtility.HtmlEncode(voter.FullName)},</p>
                <p>You are eligible to vote in <strong>{WebUtility.HtmlEncode(electionTitle)}</strong>.
                Begin here:</p>
                <p><a href="{link}">{link}</a></p>
                """;

            return ($"You are invited to vote — {electionTitle}", html);
        }
    }
}
