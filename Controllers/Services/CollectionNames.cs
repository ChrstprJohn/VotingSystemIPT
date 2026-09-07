namespace VotingSystem.Controllers.Services
{
    /// <summary>Central list of MongoDB collection names used by the voting system.</summary>
    public static class CollectionNames
    {
        public const string Users = "users";
        public const string Elections = "elections";
        public const string Positions = "positions";
        public const string Partylists = "partylists";
        public const string Candidates = "candidates";
        public const string VoterFiles = "voter_files";
        public const string Voters = "voters";
        public const string Ballots = "ballots";
    }
}
