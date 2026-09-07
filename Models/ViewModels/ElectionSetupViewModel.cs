using VotingSystem.Models.Domain;

namespace VotingSystem.Models.ViewModels
{
    public sealed class PartylistCard
    {
        public Partylist Partylist { get; set; } = null!;
        public int CandidateCount { get; set; }
        public string LeaderLink { get; set; } = string.Empty;
    }

    public sealed class ElectionSetupViewModel
    {
        public Election Election { get; set; } = null!;
        public List<Position> Positions { get; set; } = new();
        public List<PartylistCard> Partylists { get; set; } = new();
        public Dictionary<string, int> CandidateCountByPosition { get; set; } = new();
        public List<VoterFile> VoterFiles { get; set; } = new();
        public long EligibleVoterCount { get; set; }
        public PublishChecklist Checklist { get; set; } = new();

        public PublicElectionStatus Status => Election.StatusAt(DateTime.UtcNow);
        public bool IsPublished => Election.Lifecycle == ElectionLifecycle.Published;

        /// <summary>Voting has begun (or passed): everything is locked.</summary>
        public bool IsFullyLocked => IsPublished && DateTime.UtcNow >= Election.StartAt;

        /// <summary>Published but not started: only the schedule may still be changed.</summary>
        public bool IsScheduleOnly => IsPublished && DateTime.UtcNow < Election.StartAt;

        public bool CanEditContent => !IsPublished;
    }
}
