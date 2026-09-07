namespace VotingSystem.Models.ViewModels
{
    public sealed record ChecklistItem(string Label, bool Ok, string Detail);

    public sealed class PublishChecklist
    {
        public List<ChecklistItem> Items { get; } = new();
        public bool CanPublish => Items.Count > 0 && Items.TrueForAll(i => i.Ok);
    }
}
