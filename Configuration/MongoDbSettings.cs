namespace VotingSystem.Configuration
{
    public sealed class MongoDbSettings
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ClusterUrl { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;

        public string ConnectionString =>
            $"mongodb+srv://{Username}:{Password}@{ClusterUrl}/?retryWrites=true&w=majority";
    }
}
