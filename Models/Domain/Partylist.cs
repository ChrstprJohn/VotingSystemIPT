using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VotingSystem.Models.Domain
{
    public enum PartylistStatus
    {
        /// <summary>Created by the admin; the leader has not submitted it yet.</summary>
        Draft = 0,

        /// <summary>Submitted by the leader and awaiting admin review.</summary>
        Pending = 1,

        /// <summary>Approved by the admin. Only approved party lists appear on the ballot.</summary>
        Approved = 2,

        /// <summary>Sent back to the leader with a correction note.</summary>
        Returned = 3
    }

    public class Partylist
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("election_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ElectionId { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("leader_name")]
        public string LeaderName { get; set; } = string.Empty;

        [BsonElement("leader_email")]
        public string LeaderEmail { get; set; } = string.Empty;

        [BsonElement("submission_deadline")]
        public DateTime? SubmissionDeadline { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public PartylistStatus Status { get; set; } = PartylistStatus.Draft;

        [BsonElement("correction_note")]
        public string? CorrectionNote { get; set; }

        /// <summary>Unguessable token used for the leader's secure form link (no account needed).</summary>
        [BsonElement("form_token")]
        public string FormToken { get; set; } = string.Empty;

        [BsonElement("link_sent_at")]
        public DateTime? LinkSentAt { get; set; }

        [BsonElement("submitted_at")]
        public DateTime? SubmittedAt { get; set; }

        [BsonElement("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
