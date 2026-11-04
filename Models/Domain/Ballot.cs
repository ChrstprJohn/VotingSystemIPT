using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VotingSystem.Models.Domain
{
    public class BallotSelection
    {
        [BsonElement("position_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PositionId { get; set; } = string.Empty;

        [BsonElement("candidate_ids")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> CandidateIds { get; set; } = new();
    }

    /// <summary>
    /// A submitted ballot. <see cref="VoterId"/> is stored only to enforce one ballot
    /// per student; reports never join selections back to a voter identity.
    /// </summary>
    public class Ballot
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("election_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ElectionId { get; set; } = string.Empty;

        [BsonElement("voter_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string VoterId { get; set; } = string.Empty;

        [BsonElement("selections")]
        public List<BallotSelection> Selections { get; set; } = new();

        [BsonElement("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
