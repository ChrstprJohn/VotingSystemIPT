using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VotingSystem.Models.Domain
{
    /// <summary>
    /// A candidate entered by a party-list leader under one position.
    /// A candidate is "on the ballot" only when its party list is Approved.
    /// </summary>
    public class Candidate
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("election_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ElectionId { get; set; } = string.Empty;

        [BsonElement("partylist_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PartylistId { get; set; } = string.Empty;

        [BsonElement("position_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PositionId { get; set; } = string.Empty;

        [BsonElement("full_name")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("order")]
        public int Order { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
