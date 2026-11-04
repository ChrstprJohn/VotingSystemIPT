using VotingSystem.Models.Domain;

namespace VotingSystem.Models.ViewModels
{
    public sealed class BallotPosition
    {
        public Position Position { get; set; } = null!;
        public List<BallotCandidate> Candidates { get; set; } = new();
    }

    public sealed class BallotCandidate
    {
        public string CandidateId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PartylistName { get; set; } = string.Empty;
    }

    public sealed class BallotViewModel
    {
        public Election Election { get; set; } = null!;
        public Voter Voter { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public List<BallotPosition> Positions { get; set; } = new();
    }

    public sealed class PublicElectionCard
    {
        public Election Election { get; set; } = null!;
        public PublicElectionStatus Status { get; set; }
    }
}
