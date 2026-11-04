using VotingSystem.Models.Domain;

namespace VotingSystem.Models.ViewModels
{
    public sealed class ElectionListItem
    {
        public Election Election { get; set; } = null!;
        public PublicElectionStatus Status { get; set; }
        public int PositionCount { get; set; }
        public int CandidateCount { get; set; }
        public int PartylistCount { get; set; }
        public long EligibleVoters { get; set; }
    }

    public sealed class AttentionItem
    {
        public string ElectionId { get; set; } = string.Empty;
        public string ElectionTitle { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public sealed class OngoingSnapshot
    {
        public Election Election { get; set; } = null!;
        public long EligibleVoters { get; set; }
        public long BallotsSubmitted { get; set; }
        public double TurnoutPercent =>
            EligibleVoters == 0 ? 0 : Math.Round(BallotsSubmitted * 100.0 / EligibleVoters, 1);
        public TimeSpan TimeRemaining => Election.EndAt - DateTime.UtcNow;
    }

    public sealed class AdminDashboardViewModel
    {
        public List<ElectionListItem> Draft { get; set; } = new();
        public List<ElectionListItem> Upcoming { get; set; } = new();
        public List<ElectionListItem> Ongoing { get; set; } = new();
        public List<ElectionListItem> Completed { get; set; } = new();
        public List<AttentionItem> Attention { get; set; } = new();
        public List<OngoingSnapshot> OngoingSnapshots { get; set; } = new();

        public int TotalElections =>
            Draft.Count + Upcoming.Count + Ongoing.Count + Completed.Count;
        public bool HasElections => TotalElections > 0;
    }
}
