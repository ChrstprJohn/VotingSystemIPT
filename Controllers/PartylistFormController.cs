using Microsoft.AspNetCore.Mvc;
using VotingSystem.Controllers.Services;
using VotingSystem.Models.Domain;
using VotingSystem.Models.ViewModels;

namespace VotingSystem.Controllers
{
    /// <summary>
    /// Public, no-login form a party-list leader opens from their emailed secure link.
    /// </summary>
    [Route("party-form")]
    public class PartylistFormController : Controller
    {
        private readonly PartylistService _partylists;
        private readonly ElectionService _elections;
        private readonly PositionService _positions;
        private readonly CandidateService _candidates;

        public PartylistFormController(
            PartylistService partylists,
            ElectionService elections,
            PositionService positions,
            CandidateService candidates)
        {
            _partylists = partylists;
            _elections = elections;
            _positions = positions;
            _candidates = candidates;
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> Index(string token)
        {
            var vm = await BuildAsync(token);
            return vm is null ? View("NotFound") : View(vm);
        }

        [HttpPost("{token}/save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string token, string partyName, string leaderName,
            string[] candPosition, string[] candName)
        {
            var partylist = await _partylists.GetByTokenAsync(token);
            if (partylist is null)
            {
                return View("NotFound");
            }

            var election = await _elections.GetByIdAsync(partylist.ElectionId);
            if (election is null || election.Lifecycle == ElectionLifecycle.Published)
            {
                TempData["Flash"] = "This election is published — the form can no longer be edited.";
                return RedirectToAction(nameof(Index), new { token });
            }

            await PersistAsync(partylist, partyName, leaderName, candPosition, candName);
            await _partylists.RevertToPendingIfApprovedAsync(partylist.Id);
            TempData["Flash"] = "Progress saved. You can come back to this link any time before the deadline.";
            return RedirectToAction(nameof(Index), new { token });
        }

        [HttpPost("{token}/submit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string token, string partyName, string leaderName,
            string[] candPosition, string[] candName)
        {
            var partylist = await _partylists.GetByTokenAsync(token);
            if (partylist is null)
            {
                return View("NotFound");
            }

            var election = await _elections.GetByIdAsync(partylist.ElectionId);
            if (election is null || election.Lifecycle == ElectionLifecycle.Published)
            {
                TempData["Flash"] = "This election is published — the form can no longer be edited.";
                return RedirectToAction(nameof(Index), new { token });
            }

            var positions = await _positions.GetByElectionAsync(partylist.ElectionId);
            await PersistAsync(partylist, partyName, leaderName, candPosition, candName);

            // Validate min/max per position against saved candidates.
            var saved = await _candidates.GetByPartylistAsync(partylist.Id);
            var byPosition = saved.GroupBy(c => c.PositionId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var position in positions)
            {
                var count = byPosition.GetValueOrDefault(position.Id, 0);
                if (count < position.MinPerPartylist || count > position.MaxPerPartylist)
                {
                    TempData["Flash"] =
                        $"{position.Name}: enter between {position.MinPerPartylist} and {position.MaxPerPartylist} candidate(s). You have {count}.";
                    return RedirectToAction(nameof(Index), new { token });
                }
            }

            await _partylists.SubmitAsync(partylist.Id);
            TempData["Flash"] = "Submitted for admin review. You will be emailed if changes are needed.";
            return RedirectToAction(nameof(Index), new { token });
        }

        private async Task PersistAsync(Partylist partylist, string partyName, string leaderName,
            string[] candPosition, string[] candName)
        {
            if (!string.IsNullOrWhiteSpace(partyName) || !string.IsNullOrWhiteSpace(leaderName))
            {
                await _partylists.UpdateAsync(
                    partylist.Id,
                    string.IsNullOrWhiteSpace(partyName) ? partylist.Name : partyName,
                    string.IsNullOrWhiteSpace(leaderName) ? partylist.LeaderName : leaderName,
                    partylist.LeaderEmail,
                    partylist.SubmissionDeadline);
            }

            var entries = new List<(string, string)>();
            for (var i = 0; i < Math.Min(candPosition?.Length ?? 0, candName?.Length ?? 0); i++)
            {
                if (!string.IsNullOrWhiteSpace(candName![i]))
                {
                    entries.Add((candPosition![i], candName[i]));
                }
            }

            await _candidates.ReplaceForPartylistAsync(partylist.ElectionId, partylist.Id, entries);
        }

        private async Task<PartylistFormViewModel?> BuildAsync(string token)
        {
            var partylist = await _partylists.GetByTokenAsync(token);
            if (partylist is null)
            {
                return null;
            }

            var election = await _elections.GetByIdAsync(partylist.ElectionId);
            if (election is null)
            {
                return null;
            }

            var positions = await _positions.GetByElectionAsync(partylist.ElectionId);
            var candidates = await _candidates.GetByPartylistAsync(partylist.Id);

            return new PartylistFormViewModel
            {
                Partylist = partylist,
                Election = election,
                Positions = positions,
                CandidatesByPosition = positions.ToDictionary(
                    p => p.Id,
                    p => candidates.Where(c => c.PositionId == p.Id).Select(c => c.FullName).ToList())
            };
        }
    }
}
