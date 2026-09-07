using MongoDB.Driver;
using VotingSystem.Models.Domain;

namespace VotingSystem.Controllers.Services
{
    public sealed class PositionService
    {
        public static readonly (string Name, int Seats)[] DefaultPositions =
        {
            ("President", 1),
            ("Vice President", 1),
            ("Senator", 12)
        };

        private readonly IMongoCollection<Position> _positions;

        public PositionService(IMongoDatabase database)
        {
            _positions = database.GetCollection<Position>(CollectionNames.Positions);
        }

        public async Task<List<Position>> GetByElectionAsync(string electionId)
        {
            return await _positions
                .Find(p => p.ElectionId == electionId)
                .SortBy(p => p.Order)
                .ToListAsync();
        }

        public async Task<Position?> GetByIdAsync(string id)
        {
            return await _positions.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task SeedDefaultsAsync(string electionId)
        {
            var existing = await _positions.CountDocumentsAsync(p => p.ElectionId == electionId);
            if (existing > 0)
            {
                return;
            }

            var order = 1;
            var docs = DefaultPositions.Select(d => new Position
            {
                ElectionId = electionId,
                Name = d.Name,
                Seats = d.Seats,
                MinPerPartylist = 1,
                MaxPerPartylist = d.Seats,
                Order = order++
            }).ToList();

            await _positions.InsertManyAsync(docs);
        }

        /// <summary>Replaces the whole position set for an election (Step 2 save).</summary>
        public async Task ReplaceAllAsync(string electionId, IReadOnlyList<Position> incoming)
        {
            await _positions.DeleteManyAsync(p => p.ElectionId == electionId);

            if (incoming.Count == 0)
            {
                return;
            }

            var order = 1;
            foreach (var p in incoming)
            {
                p.Id = string.Empty;
                p.ElectionId = electionId;
                p.Order = order++;
                p.Seats = Math.Max(1, p.Seats);
                p.MinPerPartylist = Math.Max(0, p.MinPerPartylist);
                p.MaxPerPartylist = Math.Max(p.MinPerPartylist, p.MaxPerPartylist);
                p.CreatedAt = DateTime.UtcNow;
            }

            await _positions.InsertManyAsync(incoming);
        }

        public async Task DeleteByElectionAsync(string electionId)
        {
            await _positions.DeleteManyAsync(p => p.ElectionId == electionId);
        }

        /// <summary>Validates that every position is internally consistent.</summary>
        public static bool AreValid(IEnumerable<Position> positions, out string? error)
        {
            error = null;
            var list = positions.ToList();

            if (list.Count == 0)
            {
                error = "At least one position is required.";
                return false;
            }

            foreach (var p in list)
            {
                if (string.IsNullOrWhiteSpace(p.Name))
                {
                    error = "Every position needs a name.";
                    return false;
                }

                if (p.Seats < 1)
                {
                    error = $"\"{p.Name}\" must have at least one seat.";
                    return false;
                }

                if (p.MinPerPartylist < 0 || p.MaxPerPartylist < p.MinPerPartylist)
                {
                    error = $"\"{p.Name}\" has an invalid min/max per party list.";
                    return false;
                }
            }

            return true;
        }
    }
}
