using MongoDB.Driver;
using VotingSystem.Models.Domain;

namespace VotingSystem.Controllers.Services
{
    public sealed class CandidateService
    {
        private readonly IMongoCollection<Candidate> _candidates;
        private readonly IMongoCollection<Partylist> _partylists;

        public CandidateService(IMongoDatabase database)
        {
            _candidates = database.GetCollection<Candidate>(CollectionNames.Candidates);
            _partylists = database.GetCollection<Partylist>(CollectionNames.Partylists);
        }

        public async Task<List<Candidate>> GetByElectionAsync(string electionId)
        {
            return await _candidates.Find(c => c.ElectionId == electionId).ToListAsync();
        }

        public async Task<List<Candidate>> GetByPartylistAsync(string partylistId)
        {
            return await _candidates
                .Find(c => c.PartylistId == partylistId)
                .SortBy(c => c.Order)
                .ToListAsync();
        }

        /// <summary>
        /// Candidates that appear on the ballot for a position: those whose party list is Approved.
        /// </summary>
        public async Task<List<Candidate>> GetApprovedByPositionAsync(string positionId)
        {
            var candidates = await _candidates
                .Find(c => c.PositionId == positionId)
                .SortBy(c => c.Order)
                .ToListAsync();

            if (candidates.Count == 0)
            {
                return candidates;
            }

            var partyIds = candidates.Select(c => c.PartylistId).Distinct().ToList();
            var approved = await _partylists
                .Find(p => partyIds.Contains(p.Id) && p.Status == PartylistStatus.Approved)
                .Project(p => p.Id)
                .ToListAsync();
            var approvedSet = approved.ToHashSet();

            return candidates.Where(c => approvedSet.Contains(c.PartylistId)).ToList();
        }

        /// <summary>
        /// Replaces every candidate for one party list with the supplied set
        /// (used when a leader saves their form).
        /// </summary>
        public async Task ReplaceForPartylistAsync(
            string electionId, string partylistId, IEnumerable<(string PositionId, string FullName)> entries)
        {
            await _candidates.DeleteManyAsync(c => c.PartylistId == partylistId);

            var docs = new List<Candidate>();
            var perPosition = new Dictionary<string, int>();

            foreach (var (positionId, fullName) in entries)
            {
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    continue;
                }

                perPosition.TryGetValue(positionId, out var order);
                perPosition[positionId] = order + 1;

                docs.Add(new Candidate
                {
                    ElectionId = electionId,
                    PartylistId = partylistId,
                    PositionId = positionId,
                    FullName = fullName.Trim(),
                    Order = order
                });
            }

            if (docs.Count > 0)
            {
                await _candidates.InsertManyAsync(docs);
            }
        }

        public async Task DeleteByElectionAsync(string electionId)
        {
            await _candidates.DeleteManyAsync(c => c.ElectionId == electionId);
        }

        public async Task<Dictionary<string, int>> CountByPositionAsync(string electionId)
        {
            var all = await _candidates.Find(c => c.ElectionId == electionId).ToListAsync();
            return all
                .GroupBy(c => c.PositionId)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
