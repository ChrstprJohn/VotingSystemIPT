using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VotingSystem.Controllers.Services;
using VotingSystem.Models.Domain;
using VotingSystem.Models.ViewModels;

namespace VotingSystem.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ElectionService _elections;
        private readonly PositionService _positions;
        private readonly PartylistService _partylists;
        private readonly CandidateService _candidates;
        private readonly VoterService _voters;
        private readonly BallotService _ballots;
        private readonly ReportService _reports;
        private readonly EmailService _email;

        public AdminController(
            ElectionService elections,
            PositionService positions,
            PartylistService partylists,
            CandidateService candidates,
            VoterService voters,
            BallotService ballots,
            ReportService reports,
            EmailService email)
        {
            _elections = elections;
            _positions = positions;
            _partylists = partylists;
            _candidates = candidates;
            _voters = voters;
            _ballots = ballots;
            _reports = reports;
            _email = email;
        }

        // ---- Dashboard --------------------------------------------------------

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Dashboard";
            var now = DateTime.UtcNow;
            var elections = await _elections.GetAllAsync();
            var vm = new AdminDashboardViewModel();

            foreach (var election in elections)
            {
                var positions = await _positions.GetByElectionAsync(election.Id);
                var partylists = await _partylists.GetByElectionAsync(election.Id);
                var candidates = await _candidates.GetByElectionAsync(election.Id);
                var eligible = await _voters.CountEligibleAsync(election.Id);
                var status = election.StatusAt(now);

                var item = new ElectionListItem
                {
                    Election = election,
                    Status = status,
                    PositionCount = positions.Count,
                    CandidateCount = candidates.Count,
                    PartylistCount = partylists.Count,
                    EligibleVoters = eligible
                };

                switch (status)
                {
                    case PublicElectionStatus.Draft: vm.Draft.Add(item); break;
                    case PublicElectionStatus.Upcoming: vm.Upcoming.Add(item); break;
                    case PublicElectionStatus.Ongoing: vm.Ongoing.Add(item); break;
                    default: vm.Completed.Add(item); break;
                }

                // Items needing attention.
                if (status == PublicElectionStatus.Draft)
                {
                    var checklist = PublishValidator.Build(election, positions, partylists,
                        (await _voters.GetFilesAsync(election.Id)).Count, eligible);
                    foreach (var failing in checklist.Items.Where(i => !i.Ok))
                    {
                        vm.Attention.Add(new AttentionItem
                        {
                            ElectionId = election.Id,
                            ElectionTitle = election.Title,
                            Message = failing.Detail
                        });
                    }
                }

                foreach (var pending in partylists.Where(p =>
                             p.Status is PartylistStatus.Pending))
                {
                    vm.Attention.Add(new AttentionItem
                    {
                        ElectionId = election.Id,
                        ElectionTitle = election.Title,
                        Message = $"Party list \"{pending.Name}\" is awaiting review."
                    });
                }

                if (status == PublicElectionStatus.Ongoing)
                {
                    vm.OngoingSnapshots.Add(new OngoingSnapshot
                    {
                        Election = election,
                        EligibleVoters = eligible,
                        BallotsSubmitted = await _ballots.CountBallotsAsync(election.Id)
                    });
                }
            }

            return View(vm);
        }

        // ---- Elections list -------------------------------------------------

        public async Task<IActionResult> Elections()
        {
            ViewData["ActivePage"] = "Elections";
            var now = DateTime.UtcNow;
            var elections = await _elections.GetAllAsync();
            var list = new List<ElectionListItem>();

            foreach (var election in elections)
            {
                var positions = await _positions.GetByElectionAsync(election.Id);
                var partylists = await _partylists.GetByElectionAsync(election.Id);
                var candidates = await _candidates.GetByElectionAsync(election.Id);
                list.Add(new ElectionListItem
                {
                    Election = election,
                    Status = election.StatusAt(now),
                    PositionCount = positions.Count,
                    CandidateCount = candidates.Count,
                    PartylistCount = partylists.Count,
                    EligibleVoters = await _voters.CountEligibleAsync(election.Id)
                });
            }

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateElection(string title)
        {
            var election = await _elections.CreateDraftAsync(title ?? string.Empty);
            await _positions.SeedDefaultsAsync(election.Id);
            TempData["Flash"] = "Draft election created. Complete the setup below.";
            return RedirectToAction(nameof(ElectionSetup), new { id = election.Id });
        }

        // ---- Election setup (5 steps) --------------------------------------

        public async Task<IActionResult> ElectionSetup(string id)
        {
            ViewData["ActivePage"] = "Elections";
            var vm = await BuildSetupViewModelAsync(id);
            if (vm is null)
            {
                return NotFound();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDetails(
            string id, string title, string? description, string? instructions,
            string startDate, string startTime, string endDate, string endTime)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null)
            {
                return NotFound();
            }

            if (election.Lifecycle == ElectionLifecycle.Published)
            {
                return await RedirectSetup(id, "step-1", "Details are locked once the election is published.");
            }

            var start = CombineDateTime(startDate, startTime);
            var end = CombineDateTime(endDate, endTime);
            if (start is null || end is null || end <= start)
            {
                return await RedirectSetup(id, "step-1", "Enter a valid start and end (end must be after start).");
            }

            await _elections.UpdateDetailsAsync(
                id, title ?? string.Empty, description ?? string.Empty,
                instructions ?? string.Empty, start.Value, end.Value);
            return await RedirectSetup(id, "step-1", "Election details saved.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePositions(
            string id, string[] posName, int[] posSeats, int[] posMin, int[] posMax)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null)
            {
                return NotFound();
            }

            if (election.Lifecycle == ElectionLifecycle.Published)
            {
                return await RedirectSetup(id, "step-2", "Positions are locked once the election is published.");
            }

            var incoming = new List<Position>();
            for (var i = 0; i < (posName?.Length ?? 0); i++)
            {
                if (string.IsNullOrWhiteSpace(posName![i]))
                {
                    continue;
                }

                incoming.Add(new Position
                {
                    Name = posName[i].Trim(),
                    Seats = Get(posSeats, i, 1),
                    MinPerPartylist = Get(posMin, i, 1),
                    MaxPerPartylist = Get(posMax, i, 1)
                });
            }

            if (!PositionService.AreValid(incoming, out var error))
            {
                return await RedirectSetup(id, "step-2", error!);
            }

            await _positions.ReplaceAllAsync(id, incoming);
            return await RedirectSetup(id, "step-2", "Positions saved.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPartylist(
            string id, string name, string leaderName, string leaderEmail, string? deadline)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null)
            {
                return NotFound();
            }

            if (election.Lifecycle == ElectionLifecycle.Published)
            {
                return await RedirectSetup(id, "step-3", "Party lists are locked once the election is published.");
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(leaderName)
                || string.IsNullOrWhiteSpace(leaderEmail))
            {
                return await RedirectSetup(id, "step-3", "Party-list name, leader name and leader email are required.");
            }

            var pl = await _partylists.CreateAsync(id, name, leaderName, leaderEmail, ParseDate(deadline));
            var link = LeaderLink(pl.FormToken);
            var sent = await _email.SendLeaderLinkAsync(pl, election.Title, link);
            if (sent)
            {
                await _partylists.MarkLinkSentAsync(pl.Id);
            }

            return await RedirectSetup(id, "step-3",
                sent
                    ? $"Party list added and the form link was emailed to {pl.LeaderEmail}."
                    : "Party list added. SMTP is not configured — copy the leader link from the card below.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePartylist(
            string id, string partylistId, string name, string leaderName, string leaderEmail, string? deadline)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null || election.Lifecycle == ElectionLifecycle.Published)
            {
                return await RedirectSetup(id, "step-3", "Party lists are locked once the election is published.");
            }

            await _partylists.UpdateAsync(partylistId, name, leaderName, leaderEmail, ParseDate(deadline));
            return await RedirectSetup(id, "step-3", "Party list updated.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePartylist(string id, string partylistId)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null || election.Lifecycle == ElectionLifecycle.Published)
            {
                return await RedirectSetup(id, "step-3", "Party lists are locked once the election is published.");
            }

            await _partylists.DeleteAsync(partylistId);
            return await RedirectSetup(id, "step-3", "Party list removed.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendLeaderLink(string id, string partylistId)
        {
            var election = await _elections.GetByIdAsync(id);
            var pl = await _partylists.GetByIdAsync(partylistId);
            if (election is null || pl is null)
            {
                return NotFound();
            }

            var sent = await _email.SendLeaderLinkAsync(pl, election.Title, LeaderLink(pl.FormToken));
            if (sent)
            {
                await _partylists.MarkLinkSentAsync(pl.Id);
            }

            return await RedirectSetup(id, "step-3",
                sent ? $"Form link re-sent to {pl.LeaderEmail}." : "SMTP is not configured — use the copyable link on the card.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewPartylist(string id, string partylistId, string action, string? note)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null)
            {
                return NotFound();
            }

            if (action == "approve")
            {
                await _partylists.ApproveAsync(partylistId);
                return await RedirectSetup(id, "step-3", "Party list approved.");
            }

            if (string.IsNullOrWhiteSpace(note))
            {
                return await RedirectSetup(id, "step-3", "Add a correction note before sending a submission back.");
            }

            await _partylists.ReturnAsync(partylistId, note);
            var pl = await _partylists.GetByIdAsync(partylistId);
            if (pl is not null)
            {
                await _email.SendAsync(pl.LeaderEmail,
                    $"Party-list correction needed — {election.Title}",
                    $"<p>Your submission for <strong>{pl.Name}</strong> needs changes:</p><p>{System.Net.WebUtility.HtmlEncode(note)}</p><p><a href=\"{LeaderLink(pl.FormToken)}\">Open the form</a></p>");
            }

            return await RedirectSetup(id, "step-3", "Sent back to the leader with your note.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadVoters(string id, string departmentName, IFormFile? file)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null)
            {
                return NotFound();
            }

            if (election.Lifecycle == ElectionLifecycle.Published)
            {
                return await RedirectSetup(id, "step-4", "Voter data is locked once the election is published.");
            }

            if (string.IsNullOrWhiteSpace(departmentName) || file is null || file.Length == 0)
            {
                return await RedirectSetup(id, "step-4", "Choose a department name and a CSV file.");
            }

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();
            var result = await _voters.ImportDepartmentAsync(id, departmentName, file.FileName, content);

            TempData["ImportErrors"] = result.Errors.Count > 0
                ? string.Join("\n", result.Errors.Take(15))
                : null;

            return await RedirectSetup(id, "step-4",
                result.Succeeded
                    ? $"Imported {result.ImportedCount} voter(s) for {result.DepartmentName}" +
                      (result.SkippedCount > 0 ? $" ({result.SkippedCount} skipped)." : ".")
                    : "Import failed — see the details above.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveVoterFile(string id, string voterFileId)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null || election.Lifecycle == ElectionLifecycle.Published)
            {
                return await RedirectSetup(id, "step-4", "Voter data is locked once the election is published.");
            }

            await _voters.RemoveDepartmentAsync(id, voterFileId);
            return await RedirectSetup(id, "step-4", "Department file removed.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSchedule(
            string id, string startDate, string startTime, string endDate, string endTime)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null)
            {
                return NotFound();
            }

            if (ElectionService.VotingHasBegun(election, DateTime.UtcNow))
            {
                return await RedirectSetup(id, "step-5", "Voting has begun — the schedule is locked.");
            }

            var start = CombineDateTime(startDate, startTime);
            var end = CombineDateTime(endDate, endTime);
            if (start is null || end is null || end <= start)
            {
                return await RedirectSetup(id, "step-5", "Enter a valid start and end.");
            }

            await _elections.UpdateScheduleAsync(id, start.Value, end.Value);
            return await RedirectSetup(id, "step-5", "Schedule updated.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(string id)
        {
            var vm = await BuildSetupViewModelAsync(id);
            if (vm is null)
            {
                return NotFound();
            }

            if (vm.IsPublished)
            {
                return await RedirectSetup(id, "step-5", "This election is already published.");
            }

            if (!vm.Checklist.CanPublish)
            {
                return await RedirectSetup(id, "step-5", "Every checklist item must pass before publishing.");
            }

            await _elections.PublishAsync(id);
            return await RedirectSetup(id, "step-5", "Election published. It is now live on the public site per its schedule.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseElection(string id)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null)
            {
                return NotFound();
            }

            var status = election.StatusAt(DateTime.UtcNow);
            if (election.Lifecycle != ElectionLifecycle.Published || status == PublicElectionStatus.Closed)
            {
                return await RedirectSetup(id, "step-5", "Only a published election that is still open can be ended.");
            }

            await _elections.CloseNowAsync(id);
            return await RedirectSetup(id, "step-5",
                "Election ended. Voting is closed, no further ballots are accepted, and the reports are final.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendInvitations(string id)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null || election.Lifecycle != ElectionLifecycle.Published)
            {
                return await RedirectSetup(id, "step-5", "Publish the election before sending invitations.");
            }

            var voters = await _voters.GetAllVotersAsync(id);
            var sent = 0;
            foreach (var voter in voters)
            {
                if (await _email.SendInvitationAsync(voter, election.Title,
                        $"{_email.BaseUrl}/vote/election/{election.Id}"))
                {
                    sent++;
                }
            }

            return await RedirectSetup(id, "step-5",
                sent > 0 ? $"Invitations sent to {sent} of {voters.Count} voters."
                    : "No invitations sent — SMTP is not configured.");
        }

        // ---- Reports -------------------------------------------------------

        public async Task<IActionResult> Reports(string id)
        {
            ViewData["ActivePage"] = "Elections";
            var report = await _reports.BuildAsync(id);
            if (report is null)
            {
                return NotFound();
            }

            return View(report);
        }

        // ---- Placeholder pages kept from the original scaffold ------------

        public async Task<IActionResult> Voters()
        {
            ViewData["ActivePage"] = "Voters";
            return View(await _elections.GetAllAsync());
        }

        public IActionResult Settings()
        {
            ViewData["ActivePage"] = "Settings";
            return View();
        }

        // ---- Helpers -----------------------------------------------------

        private async Task<ElectionSetupViewModel?> BuildSetupViewModelAsync(string id)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null)
            {
                return null;
            }

            var positions = await _positions.GetByElectionAsync(id);
            var partylists = await _partylists.GetByElectionAsync(id);
            var candidateCounts = (await _candidates.GetByElectionAsync(id))
                .GroupBy(c => c.PartylistId)
                .ToDictionary(g => g.Key, g => g.Count());
            var files = await _voters.GetFilesAsync(id);
            var eligible = await _voters.CountEligibleAsync(id);

            return new ElectionSetupViewModel
            {
                Election = election,
                Positions = positions,
                Partylists = partylists.Select(p => new PartylistCard
                {
                    Partylist = p,
                    CandidateCount = candidateCounts.GetValueOrDefault(p.Id, 0),
                    LeaderLink = LeaderLink(p.FormToken)
                }).ToList(),
                CandidateCountByPosition = await _candidates.CountByPositionAsync(id),
                VoterFiles = files,
                EligibleVoterCount = eligible,
                Checklist = PublishValidator.Build(election, positions, partylists, files.Count, eligible)
            };
        }

        private async Task<IActionResult> RedirectSetup(string id, string anchor, string flash)
        {
            TempData["Flash"] = flash;
            TempData["FlashAnchor"] = anchor;
            await Task.CompletedTask;
            return Redirect(Url.Action(nameof(ElectionSetup), new { id })! + "#" + anchor);
        }

        private string LeaderLink(string token)
        {
            var baseUrl = _email.BaseUrl;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = $"{Request.Scheme}://{Request.Host}";
            }

            return $"{baseUrl}/party-form/{token}";
        }

        private static int Get(int[]? arr, int i, int fallback) =>
            arr is not null && i < arr.Length ? arr[i] : fallback;

        private static DateTime? CombineDateTime(string? date, string? time)
        {
            if (string.IsNullOrWhiteSpace(date))
            {
                return null;
            }

            var t = string.IsNullOrWhiteSpace(time) ? "00:00" : time;
            if (!DateTime.TryParse($"{date}T{t}", out var parsed))
            {
                return null;
            }

            return DateTime.SpecifyKind(parsed, DateTimeKind.Local).ToUniversalTime();
        }

        private static DateTime? ParseDate(string? date)
        {
            if (string.IsNullOrWhiteSpace(date))
            {
                return null;
            }

            return DateTime.TryParse(date, out var parsed)
                ? DateTime.SpecifyKind(parsed.Date.AddHours(23).AddMinutes(59), DateTimeKind.Local).ToUniversalTime()
                : null;
        }
    }
}
