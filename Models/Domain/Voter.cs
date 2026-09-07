using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VotingSystem.Models.Domain
{
    /// <summary>
    /// One eligible student for an election, imported from a department CSV.
    /// The participation record exposes only <see cref="HasVoted"/> — never ballot choices.
    /// </summary>
    public class Voter
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("election_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ElectionId { get; set; } = string.Empty;

        [BsonElement("voter_file_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string VoterFileId { get; set; } = string.Empty;

        [BsonElement("student_number")]
        public string StudentNumber { get; set; } = string.Empty;

        [BsonElement("full_name")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("course")]
        public string Course { get; set; } = string.Empty;

        [BsonElement("section")]
        public string Section { get; set; } = string.Empty;

        [BsonElement("has_voted")]
        public bool HasVoted { get; set; }

        [BsonElement("voted_at")]
        public DateTime? VotedAt { get; set; }

        /// <summary>One-time token issued for the ballot link after successful verification.</summary>
        [BsonElement("access_token")]
        public string? AccessToken { get; set; }

        [BsonElement("token_issued_at")]
        public DateTime? TokenIssuedAt { get; set; }
    }
}
