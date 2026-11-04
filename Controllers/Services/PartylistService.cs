using MongoDB.Driver;
using VotingSystem.Models.Domain;

namespace VotingSystem.Controllers.Services
{
    public sealed class PartylistService
    {
        private readonly IMongoCollection<Partylist> _partylists;
        private readonly IMongoCollection<Candidate> _candidates;

        public PartylistService(IMongoDatabase database)
        {
            _partylists = database.GetCollection<Partylist>(CollectionNames.Partylists);
            _candidates = database.GetCollection<Candidate>(CollectionNames.Candidates);
        }

        public async Task<List<Partylist>> GetByElectionAsync(string electionId)
        {
            return await _partylists
                .Find(p => p.ElectionId == electionId)
                .SortBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Partylist?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !MongoDB.Bson.ObjectId.TryParse(id, out _))
            {
                return null;
            }

            return await _partylists.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Partylist?> GetByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            return await _partylists.Find(p => p.FormToken == token).FirstOrDefaultAsync();
        }

        public async Task<Partylist> CreateAsync(
            string electionId, string name, string leaderName, string leaderEmail, DateTime? deadline)
        {
            var now = DateTime.UtcNow;
            var partylist = new Partylist
            {
                ElectionId = electionId,
                Name = name.Trim(),
                LeaderName = leaderName.Trim(),
                LeaderEmail = leaderEmail.Trim(),
                SubmissionDeadline = deadline,
                Status = PartylistStatus.Draft,
                FormToken = TokenGenerator.Create(),
                CreatedAt = now,
                UpdatedAt = now
            };

            await _partylists.InsertOneAsync(partylist);
            return partylist;
        }

        public async Task UpdateAsync(
            string id, string name, string leaderName, string leaderEmail, DateTime? deadline)
        {
            var update = Builders<Partylist>.Update
                .Set(p => p.Name, name.Trim())
                .Set(p => p.LeaderName, leaderName.Trim())
                .Set(p => p.LeaderEmail, leaderEmail.Trim())
                .Set(p => p.SubmissionDeadline, deadline)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            await _partylists.UpdateOneAsync(p => p.Id == id, update);
        }

        public async Task MarkLinkSentAsync(string id)
        {
            var update = Builders<Partylist>.Update
                .Set(p => p.LinkSentAt, DateTime.UtcNow)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            await _partylists.UpdateOneAsync(p => p.Id == id, update);
        }

        /// <summary>Leader submits the form for admin review.</summary>
        public async Task SubmitAsync(string id)
        {
            var update = Builders<Partylist>.Update
                .Set(p => p.Status, PartylistStatus.Pending)
                .Set(p => p.SubmittedAt, DateTime.UtcNow)
                .Set(p => p.CorrectionNote, null)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            await _partylists.UpdateOneAsync(p => p.Id == id, update);
        }

        public async Task ApproveAsync(string id)
        {
            var update = Builders<Partylist>.Update
                .Set(p => p.Status, PartylistStatus.Approved)
                .Set(p => p.CorrectionNote, null)
                .Set(p => p.ReviewedAt, DateTime.UtcNow)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            await _partylists.UpdateOneAsync(p => p.Id == id, update);
        }

        public async Task ReturnAsync(string id, string note)
        {
            var update = Builders<Partylist>.Update
                .Set(p => p.Status, PartylistStatus.Returned)
                .Set(p => p.CorrectionNote, note.Trim())
                .Set(p => p.ReviewedAt, DateTime.UtcNow)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            await _partylists.UpdateOneAsync(p => p.Id == id, update);
        }

        /// <summary>An edit to an approved submission sends it back to Pending review.</summary>
        public async Task RevertToPendingIfApprovedAsync(string id)
        {
            var update = Builders<Partylist>.Update
                .Set(p => p.Status, PartylistStatus.Pending)
                .Set(p => p.ReviewedAt, (DateTime?)null)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);
            await _partylists.UpdateOneAsync(
                p => p.Id == id && p.Status == PartylistStatus.Approved, update);
        }

        public async Task DeleteAsync(string id)
        {
            await _candidates.DeleteManyAsync(c => c.PartylistId == id);
            await _partylists.DeleteOneAsync(p => p.Id == id);
        }

        public async Task DeleteByElectionAsync(string electionId)
        {
            await _partylists.DeleteManyAsync(p => p.ElectionId == electionId);
        }

        public async Task<bool> AllFinalizedAsync(string electionId)
        {
            var total = await _partylists.CountDocumentsAsync(p => p.ElectionId == electionId);
            if (total == 0)
            {
                return false;
            }

            var approved = await _partylists.CountDocumentsAsync(
                p => p.ElectionId == electionId && p.Status == PartylistStatus.Approved);
            return approved == total;
        }
    }
}
