using VotingSystem.Models.Domain;

namespace VotingSystem.Models.ViewModels
{
    public sealed class CandidateResult
    {
        public string CandidateId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PartylistName { get; set; } = string.Empty;
        public int Votes { get; set; }
        public bool IsWinner { get; set; }
        public int Rank { get; set; }
    }

    public sealed class PositionResult
    {
        public string PositionId { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public int Seats { get; set; }
        public List<CandidateResult> Candidates { get; set; } = new();
        public IEnumerable<CandidateResult> Winners => Candidates.Where(c => c.IsWinner);
    }

    public sealed class NonVoterRow
    {
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
    }

    public sealed class ElectionReport
    {
        public Election Election { get; set; } = null!;
        public bool VotingClosed { get; set; }
        public List<PositionResult> Positions { get; set; } = new();
        public int TotalEligible { get; set; }
        public int TotalVoters { get; set; }
        public double TurnoutPercent =>
            TotalEligible == 0 ? 0 : Math.Round(TotalVoters * 100.0 / TotalEligible, 1);
        public List<NonVoterRow> NonVoters { get; set; } = new();
    }
}
