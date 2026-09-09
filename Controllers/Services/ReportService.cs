using VotingSystem.Models.Domain;
using VotingSystem.Models.ViewModels;

namespace VotingSystem.Controllers.Services
{
    public sealed class ReportService
    {
        private readonly ElectionService _elections;
        private readonly PositionService _positions;
        private readonly PartylistService _partylists;
        private readonly CandidateService _candidates;
        private readonly VoterService _voters;
        private readonly BallotService _ballots;

        public ReportService(
            ElectionService elections,
            PositionService positions,
            PartylistService partylists,
            CandidateService candidates,
            VoterService voters,
            BallotService ballots)
        {
            _elections = elections;
            _positions = positions;
            _partylists = partylists;
            _candidates = candidates;
            _voters = voters;
            _ballots = ballots;
        }

        public async Task<ElectionReport?> BuildAsync(string electionId)
        {
            var election = await _elections.GetByIdAsync(electionId);
            if (election is null)
            {
                return null;
            }

            var positions = await _positions.GetByElectionAsync(electionId);
            var candidates = await _candidates.GetByElectionAsync(electionId);
            var partylists = await _partylists.GetByElectionAsync(electionId);
            var tally = await _ballots.TallyAsync(electionId);
            var allVoters = await _voters.GetAllVotersAsync(electionId);

            var partyNames = partylists.ToDictionary(p => p.Id, p => p.Name);
            var approvedParty = partylists
                .Where(p => p.Status == PartylistStatus.Approved)
                .Select(p => p.Id)
                .ToHashSet();

            var report = new ElectionReport
            {
                Election = election,
                VotingClosed = election.StatusAt(DateTime.UtcNow) == PublicElectionStatus.Closed,
                TotalEligible = allVoters.Count,
                TotalVoters = allVoters.Count(v => v.HasVoted)
            };

            foreach (var position in positions)
            {
                var rows = candidates
                    .Where(c => c.PositionId == position.Id && approvedParty.Contains(c.PartylistId))
                    .Select(c => new CandidateResult
                    {
                        CandidateId = c.Id,
                        FullName = c.FullName,
                        PartylistName = partyNames.GetValueOrDefault(c.PartylistId, "—"),
                        Votes = tally.GetValueOrDefault(c.Id, 0)
                    })
                    .OrderByDescending(c => c.Votes)
                    .ThenBy(c => c.FullName)
                    .ToList();

                for (var i = 0; i < rows.Count; i++)
                {
                    rows[i].Rank = i + 1;
                    rows[i].IsWinner = i < position.Seats && rows[i].Votes > 0;
                }

                report.Positions.Add(new PositionResult
                {
                    PositionId = position.Id,
                    PositionName = position.Name,
                    Seats = position.Seats,
                    Candidates = rows
                });
            }

            report.NonVoters = allVoters
                .Where(v => !v.HasVoted)
                .OrderBy(v => v.Course).ThenBy(v => v.FullName)
                .Select(v => new NonVoterRow
                {
                    StudentNumber = v.StudentNumber,
                    FullName = v.FullName,
                    Course = v.Course,
                    Section = v.Section
                })
                .ToList();

            return report;
        }
    }
}
