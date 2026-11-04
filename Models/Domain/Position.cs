using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VotingSystem.Models.Domain
{
    public class Position
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("election_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ElectionId { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("order")]
        public int Order { get; set; }

        /// <summary>
        /// How many candidates win, and the maximum a single voter may select.
        /// </summary>
        [BsonElement("seats")]
        public int Seats { get; set; } = 1;

        [BsonElement("min_per_partylist")]
        public int MinPerPartylist { get; set; } = 1;

        [BsonElement("max_per_partylist")]
        public int MaxPerPartylist { get; set; } = 1;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
