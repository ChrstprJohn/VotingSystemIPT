using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VotingSystem.Models.Domain
{
    /// <summary>
    /// Lifecycle flag the admin controls directly. The public-facing status
    /// (Upcoming / Ongoing / Closed) is derived from the schedule once Published.
    /// </summary>
    public enum ElectionLifecycle
    {
        Draft = 0,
        Published = 1
    }

    /// <summary>
    /// Status shown on the public website, derived from the schedule.
    /// </summary>
    public enum PublicElectionStatus
    {
        Draft = 0,
        Upcoming = 1,
        Ongoing = 2,
        Closed = 3
    }

    public class Election
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("instructions")]
        public string Instructions { get; set; } = string.Empty;

        [BsonElement("start_at")]
        public DateTime StartAt { get; set; }

        [BsonElement("end_at")]
        public DateTime EndAt { get; set; }

        [BsonElement("image_path")]
        public string ImagePath { get; set; } = "/image-logo/qcu-image-sample.jpg";

        [BsonElement("lifecycle")]
        [BsonRepresentation(BsonType.String)]
        public ElectionLifecycle Lifecycle { get; set; } = ElectionLifecycle.Draft;

        [BsonElement("published_at")]
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// Set when the admin ends the election early. Once set, the election counts as
        /// Closed regardless of <see cref="EndAt"/> and no further ballots are accepted.
        /// </summary>
        [BsonElement("closed_at")]
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// Set to true once voting has begun. After this everything is locked;
        /// before it only the voting start/end time may be adjusted.
        /// </summary>
        [BsonElement("voting_locked")]
        public bool VotingLocked { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public PublicElectionStatus StatusAt(DateTime nowUtc)
        {
            if (Lifecycle != ElectionLifecycle.Published)
            {
                return PublicElectionStatus.Draft;
            }

            if (ClosedAt.HasValue)
            {
                return PublicElectionStatus.Closed;
            }

            if (nowUtc < StartAt)
            {
                return PublicElectionStatus.Upcoming;
            }

            return nowUtc <= EndAt
                ? PublicElectionStatus.Ongoing
                : PublicElectionStatus.Closed;
        }
    }
}
