using VotingSystem.Models.Domain;
using VotingSystem.Models.ViewModels;

namespace VotingSystem.Controllers.Services
{
    /// <summary>
    /// Builds the Step 5 review checklist. The admin cannot publish until every item passes.
    /// </summary>
    public static class PublishValidator
    {
        public static PublishChecklist Build(
            Election election,
            IReadOnlyList<Position> positions,
            IReadOnlyList<Partylist> partylists,
            int voterFileCount,
            long eligibleVoterCount)
        {
            var checklist = new PublishChecklist();

            var scheduleOk = election.EndAt > election.StartAt
                             && !string.IsNullOrWhiteSpace(election.Title);
            checklist.Items.Add(new ChecklistItem(
                "Election details and schedule are complete",
                scheduleOk,
                scheduleOk
                    ? $"{election.Title} · {election.StartAt.ToLocalTime():MMM d, yyyy h:mm tt} – {election.EndAt.ToLocalTime():MMM d, yyyy h:mm tt}"
                    : "Add a title and make sure the end time is after the start time."));

            var positionsOk = PositionService.AreValid(positions, out var positionError);
            checklist.Items.Add(new ChecklistItem(
                "Positions, seats, and party-list limits are valid",
                positionsOk,
                positionsOk ? $"{positions.Count} position(s) configured" : positionError!));

            var hasParty = partylists.Count > 0;
            var allApproved = hasParty && partylists.All(p => p.Status == PartylistStatus.Approved);
            var pending = partylists.Count(p => p.Status != PartylistStatus.Approved);
            checklist.Items.Add(new ChecklistItem(
                "All party-list submissions and candidates are approved",
                allApproved,
                !hasParty
                    ? "No party lists have been added yet."
                    : allApproved
                        ? $"{partylists.Count} party list(s) approved"
                        : $"{pending} party list(s) still awaiting approval."));

            var votersOk = voterFileCount > 0 && eligibleVoterCount > 0;
            checklist.Items.Add(new ChecklistItem(
                "At least one department/course voter CSV is uploaded",
                votersOk,
                votersOk
                    ? $"{voterFileCount} department file(s) · {eligibleVoterCount} eligible voter(s)"
                    : "Upload at least one department CSV."));

            return checklist;
        }
    }
}
