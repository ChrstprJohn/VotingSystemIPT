using MongoDB.Driver;
using VotingSystem.Models.Domain;

namespace VotingSystem.Controllers.Services
{
    /// <summary>
    /// Ensures the indexes the voting flow relies on. Runs once at startup and is
    /// idempotent — CreateOne is a no-op when the index already exists.
    /// </summary>
    public sealed class DbInitializer : IHostedService
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(IMongoDatabase database, ILogger<DbInitializer> logger)
        {
            _database = database;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Voter uniqueness (student number / email within an election) is enforced
                // in VoterService during import, not by a DB constraint — a unique index
                // would make a legitimate cross-department re-import throw at insert time,
                // and cannot be built at all if existing data already has duplicates.
                var voters = _database.GetCollection<Voter>(CollectionNames.Voters);

                // Remove earlier unique variants of these indexes if a previous build created them.
                foreach (var stale in new[] { "uq_election_student", "uq_election_email" })
                {
                    try { await voters.Indexes.DropOneAsync(stale, cancellationToken); }
                    catch { /* not present — fine */ }
                }

                await voters.Indexes.CreateManyAsync(new[]
                {
                    new CreateIndexModel<Voter>(
                        Builders<Voter>.IndexKeys.Ascending(v => v.ElectionId).Ascending(v => v.StudentNumber),
                        new CreateIndexOptions { Name = "ix_election_student" }),
                    new CreateIndexModel<Voter>(
                        Builders<Voter>.IndexKeys.Ascending(v => v.ElectionId).Ascending(v => v.Email),
                        new CreateIndexOptions { Name = "ix_election_email" }),
                    new CreateIndexModel<Voter>(
                        Builders<Voter>.IndexKeys.Ascending(v => v.AccessToken),
                        new CreateIndexOptions { Sparse = true, Name = "ix_access_token" })
                }, cancellationToken);

                var ballots = _database.GetCollection<Ballot>(CollectionNames.Ballots);
                await ballots.Indexes.CreateOneAsync(
                    new CreateIndexModel<Ballot>(
                        Builders<Ballot>.IndexKeys.Ascending(b => b.VoterId),
                        new CreateIndexOptions { Unique = true, Name = "uq_ballot_voter" }),
                    cancellationToken: cancellationToken);

                var partylists = _database.GetCollection<Partylist>(CollectionNames.Partylists);
                await partylists.Indexes.CreateOneAsync(
                    new CreateIndexModel<Partylist>(
                        Builders<Partylist>.IndexKeys.Ascending(p => p.FormToken),
                        new CreateIndexOptions { Unique = true, Name = "uq_partylist_token" }),
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Voting system indexes ensured.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ensure indexes (continuing anyway).");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
