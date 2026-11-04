using MongoDB.Driver;
using VotingSystem.Models.Domain;

namespace VotingSystem.Controllers.Services
{
    public sealed record BallotResult(bool Succeeded, string? Error)
    {
        public static BallotResult Ok() => new(true, null);
        public static BallotResult Fail(string error) => new(false, error);
    }

    public sealed class BallotService
    {
        private readonly IMongoCollection<Ballot> _ballots;
        private readonly IMongoCollection<Voter> _voters;
        private readonly PositionService _positions;
        private readonly CandidateService _candidates;

        public BallotService(
            IMongoDatabase database, PositionService positions, CandidateService candidates)
        {
            _ballots = database.GetCollection<Ballot>(CollectionNames.Ballots);
            _voters = database.GetCollection<Voter>(CollectionNames.Voters);
            _positions = positions;
            _candidates = candidates;
        }

        /// <summary>
        /// Validates and records a ballot: one per voter, each position within its seat
        /// count, only approved candidates for the correct position.
        /// </summary>
        public async Task<BallotResult> CastAsync(
            string electionId, Voter voter, IReadOnlyDictionary<string, List<string>> selectionsByPosition)
        {
            if (voter.HasVoted)
            {
                return BallotResult.Fail("A ballot has already been submitted for this student.");
            }

            var positions = await _positions.GetByElectionAsync(electionId);
            var selections = new List<BallotSelection>();

            foreach (var position in positions)
            {
                selectionsByPosition.TryGetValue(position.Id, out var chosen);
                chosen ??= new List<string>();
                chosen = chosen.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();

                if (chosen.Count > position.Seats)
                {
                    return BallotResult.Fail(
                        $"You selected more than {position.Seats} candidate(s) for {position.Name}.");
                }

                if (chosen.Count > 0)
                {
                    var allowed = (await _candidates.GetApprovedByPositionAsync(position.Id))
                        .Select(c => c.Id)
                        .ToHashSet();

                    if (chosen.Any(c => !allowed.Contains(c)))
                    {
                        return BallotResult.Fail($"An invalid candidate was selected for {position.Name}.");
                    }
                }

                selections.Add(new BallotSelection
                {
                    PositionId = position.Id,
                    CandidateIds = chosen
                });
            }

            // Atomically claim the voter so a double submit cannot slip through.
            var claim = await _voters.UpdateOneAsync(
                v => v.Id == voter.Id && !v.HasVoted,
                Builders<Voter>.Update
                    .Set(v => v.HasVoted, true)
                    .Set(v => v.VotedAt, DateTime.UtcNow)
                    .Set(v => v.AccessToken, null));

            if (claim.ModifiedCount == 0)
            {
                return BallotResult.Fail("A ballot has already been submitted for this student.");
            }

            await _ballots.InsertOneAsync(new Ballot
            {
                ElectionId = electionId,
                VoterId = voter.Id,
                Selections = selections,
                SubmittedAt = DateTime.UtcNow
            });

            return BallotResult.Ok();
        }

        public async Task<long> CountBallotsAsync(string electionId)
        {
            return await _ballots.CountDocumentsAsync(b => b.ElectionId == electionId);
        }

        /// <summary>candidateId -> vote count, across every submitted ballot for the election.</summary>
        public async Task<Dictionary<string, int>> TallyAsync(string electionId)
        {
            var ballots = await _ballots.Find(b => b.ElectionId == electionId).ToListAsync();
            var tally = new Dictionary<string, int>();

            foreach (var candidateId in ballots
                         .SelectMany(b => b.Selections)
                         .SelectMany(s => s.CandidateIds))
            {
                tally.TryGetValue(candidateId, out var current);
                tally[candidateId] = current + 1;
            }

            return tally;
        }

        public async Task DeleteByElectionAsync(string electionId)
        {
            await _ballots.DeleteManyAsync(b => b.ElectionId == electionId);
        }
    }
}
