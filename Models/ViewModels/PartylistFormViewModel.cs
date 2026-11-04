using VotingSystem.Models.Domain;

namespace VotingSystem.Models.ViewModels
{
    public sealed class PartylistFormViewModel
    {
        public Partylist Partylist { get; set; } = null!;
        public Election Election { get; set; } = null!;
        public List<Position> Positions { get; set; } = new();

        /// <summary>positionId -> candidate names already entered for that position.</summary>
        public Dictionary<string, List<string>> CandidatesByPosition { get; set; } = new();

        public bool Locked => Election.Lifecycle == ElectionLifecycle.Published;
        public bool PastDeadline =>
            Partylist.SubmissionDeadline is { } d && DateTime.UtcNow > d;
        public bool ReadOnly => Locked || PastDeadline;
    }
}
