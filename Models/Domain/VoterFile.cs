using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VotingSystem.Models.Domain
{
    /// <summary>
    /// One department/course CSV registered for an election. Each department has
    /// exactly one current file; replacing it fully overrides that department's voters.
    /// </summary>
    public class VoterFile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("election_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ElectionId { get; set; } = string.Empty;

        [BsonElement("department_name")]
        public string DepartmentName { get; set; } = string.Empty;

        [BsonElement("file_name")]
        public string FileName { get; set; } = string.Empty;

        [BsonElement("row_count")]
        public int RowCount { get; set; }

        [BsonElement("uploaded_at")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
