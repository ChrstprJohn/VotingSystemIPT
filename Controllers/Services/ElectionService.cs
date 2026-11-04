using MongoDB.Driver;
using VotingSystem.Models.Domain;

namespace VotingSystem.Controllers.Services
{
    public sealed class ElectionService
    {
        private readonly IMongoCollection<Election> _elections;

        public ElectionService(IMongoDatabase database)
        {
            _elections = database.GetCollection<Election>(CollectionNames.Elections);
        }

        public async Task<List<Election>> GetAllAsync()
        {
            return await _elections
                .Find(FilterDefinition<Election>.Empty)
                .SortByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<Election?> GetByIdAsync(string id)
        {
            if (!IdIsValid(id))
            {
                return null;
            }

            return await _elections.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Election> CreateDraftAsync(string title)
        {
            var now = DateTime.UtcNow;
            var election = new Election
            {
                Title = string.IsNullOrWhiteSpace(title) ? "Untitled election" : title.Trim(),
                StartAt = now.Date.AddDays(7).AddHours(8),
                EndAt = now.Date.AddDays(8).AddHours(17),
                Lifecycle = ElectionLifecycle.Draft,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _elections.InsertOneAsync(election);
            return election;
        }

        public async Task UpdateDetailsAsync(
            string id, string title, string description, string instructions,
            DateTime startAt, DateTime endAt)
        {
            var update = Builders<Election>.Update
                .Set(e => e.Title, title.Trim())
                .Set(e => e.Description, description?.Trim() ?? string.Empty)
                .Set(e => e.Instructions, instructions?.Trim() ?? string.Empty)
                .Set(e => e.StartAt, startAt)
                .Set(e => e.EndAt, endAt)
                .Set(e => e.UpdatedAt, DateTime.UtcNow);

            await _elections.UpdateOneAsync(e => e.Id == id, update);
        }

        /// <summary>Only the schedule may change once published and before voting begins.</summary>
        public async Task UpdateScheduleAsync(string id, DateTime startAt, DateTime endAt)
        {
            var update = Builders<Election>.Update
                .Set(e => e.StartAt, startAt)
                .Set(e => e.EndAt, endAt)
                .Set(e => e.UpdatedAt, DateTime.UtcNow);

            await _elections.UpdateOneAsync(e => e.Id == id, update);
        }

        public async Task PublishAsync(string id)
        {
            var update = Builders<Election>.Update
                .Set(e => e.Lifecycle, ElectionLifecycle.Published)
                .Set(e => e.PublishedAt, DateTime.UtcNow)
                .Set(e => e.UpdatedAt, DateTime.UtcNow);

            await _elections.UpdateOneAsync(e => e.Id == id, update);
        }

        /// <summary>Ends an ongoing election immediately. No further ballots are accepted.</summary>
        public async Task CloseNowAsync(string id)
        {
            var update = Builders<Election>.Update
                .Set(e => e.ClosedAt, DateTime.UtcNow)
                .Set(e => e.VotingLocked, true)
                .Set(e => e.UpdatedAt, DateTime.UtcNow);

            await _elections.UpdateOneAsync(e => e.Id == id && e.ClosedAt == null, update);
        }

        public async Task LockVotingAsync(string id)
        {
            var update = Builders<Election>.Update
                .Set(e => e.VotingLocked, true)
                .Set(e => e.UpdatedAt, DateTime.UtcNow);

            await _elections.UpdateOneAsync(e => e.Id == id && !e.VotingLocked, update);
        }

        public async Task DeleteAsync(string id)
        {
            await _elections.DeleteOneAsync(e => e.Id == id);
        }

        /// <summary>True once the current time has reached the start time of a published election.</summary>
        public static bool VotingHasBegun(Election election, DateTime nowUtc)
        {
            return election.Lifecycle == ElectionLifecycle.Published && nowUtc >= election.StartAt;
        }

        private static bool IdIsValid(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && MongoDB.Bson.ObjectId.TryParse(id, out _);
        }
    }
}
