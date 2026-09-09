namespace VotingSystem.Models.ViewModels
{
    public sealed class VoterImportResult
    {
        public bool Succeeded { get; set; }
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; } = new();
        public string DepartmentName { get; set; } = string.Empty;
    }
}
