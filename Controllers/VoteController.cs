using Microsoft.AspNetCore.Mvc;
using VotingSystem.Controllers.Services;
using VotingSystem.Models.Domain;
using VotingSystem.Models.ViewModels;

namespace VotingSystem.Controllers
{
    /// <summary>Public voting flow: browse elections, verify identity, cast a ballot.</summary>
    [Route("vote")]
    public class VoteController : Controller
    {
        private readonly ElectionService _elections;
        private readonly PositionService _positions;
        private readonly CandidateService _candidates;
        private readonly VoterService _voters;
        private readonly BallotService _ballots;
        private readonly EmailService _email;

        public VoteController(
            ElectionService elections,
            PositionService positions,
            CandidateService candidates,
            VoterService voters,
            BallotService ballots,
            EmailService email)
        {
            _elections = elections;
            _positions = positions;
            _candidates = candidates;
            _voters = voters;
            _ballots = ballots;
            _email = email;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var cards = (await _elections.GetAllAsync())
                .Where(e => e.Lifecycle == ElectionLifecycle.Published)
                .Select(e => new PublicElectionCard { Election = e, Status = e.StatusAt(now) })
                .ToList();
            return View(cards);
        }

        [HttpGet("election/{id}")]
        public async Task<IActionResult> Election(string id)
        {
            var election = await _elections.GetByIdAsync(id);
            if (election is null || election.Lifecycle != ElectionLifecycle.Published)
            {
                return View("Unavailable");
            }

            return View(new PublicElectionCard
            {
                Election = election,
                Status = election.StatusAt(DateTime.UtcNow)
            });
        }

        [HttpPost("verify")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(string electionId, string studentNumber, string email)
        {
            var election = await _elections.GetByIdAsync(electionId);
            if (election is null || election.StatusAt(DateTime.UtcNow) != PublicElectionStatus.Ongoing)
            {
                TempData["VerifyError"] = "This election is not currently open for voting.";
                return RedirectToAction(nameof(Election), new { id = electionId });
            }

            if (string.IsNullOrWhiteSpace(studentNumber) || string.IsNullOrWhiteSpace(email))
            {
                TempData["VerifyError"] = "Enter your student number and registered school email.";
                return RedirectToAction(nameof(Election), new { id = electionId });
            }

            var voter = await _voters.VerifyAsync(electionId, studentNumber, email);

            // Deliberately vague on failure — do not reveal which part did not match.
            if (voter is null)
            {
                TempData["VerifyError"] = "We could not verify those details for this election.";
                return RedirectToAction(nameof(Election), new { id = electionId });
            }

            if (voter.HasVoted)
            {
                TempData["VerifyError"] = "Our records show a ballot has already been submitted for you.";
                return RedirectToAction(nameof(Election), new { id = electionId });
            }

            var token = await _voters.IssueAccessTokenAsync(voter.Id);
            var link = $"{BaseUrl()}/vote/ballot/{token}";
            var sent = await _email.SendBallotLinkAsync(voter, election.Title, link);

            TempData["VerifyOk"] = sent
                ? $"A secure ballot link has been sent to {MaskEmail(voter.Email)}. Check your inbox to continue."
                : "The email webhook is not configured. Use the link below to continue to your ballot.";
            TempData["VerifyLink"] = sent ? null : link;
            return RedirectToAction(nameof(Election), new { id = electionId });
        }

        [HttpGet("ballot/{token}")]
        public async Task<IActionResult> Ballot(string token)
        {
            var vm = await BuildBallotAsync(token);
            if (vm is null)
            {
                return View("Unavailable");
            }

            return View(vm);
        }

        [HttpPost("ballot/{token}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitBallot(string token)
        {
            var voter = await _voters.GetByAccessTokenAsync(token);
            if (voter is null)
            {
                return View("Unavailable");
            }

            var election = await _elections.GetByIdAsync(voter.ElectionId);
            if (election is null || election.StatusAt(DateTime.UtcNow) != PublicElectionStatus.Ongoing)
            {
                return View("Unavailable");
            }

            // Selections arrive as form keys "pos_<positionId>" with 0..n candidate id values.
            var selections = new Dictionary<string, List<string>>();
            foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("pos_", StringComparison.Ordinal)))
            {
                var positionId = key[4..];
                selections[positionId] = Request.Form[key]
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => v!)
                    .ToList();
            }

            var result = await _ballots.CastAsync(voter.ElectionId, voter, selections);
            if (!result.Succeeded)
            {
                TempData["BallotError"] = result.Error;
                return RedirectToAction(nameof(Ballot), new { token });
            }

            TempData["ConfirmElection"] = election.Title;
            return RedirectToAction(nameof(Confirmation));
        }

        [HttpGet("confirmation")]
        public IActionResult Confirmation()
        {
            ViewData["ElectionTitle"] = TempData["ConfirmElection"] as string ?? "the election";
            return View();
        }

        private async Task<BallotViewModel?> BuildBallotAsync(string token)
        {
            var voter = await _voters.GetByAccessTokenAsync(token);
            if (voter is null || voter.HasVoted)
            {
                return null;
            }

            var election = await _elections.GetByIdAsync(voter.ElectionId);
            if (election is null || election.StatusAt(DateTime.UtcNow) != PublicElectionStatus.Ongoing)
            {
                return null;
            }

            var positions = await _positions.GetByElectionAsync(election.Id);
            var vm = new BallotViewModel
            {
                Election = election,
                Voter = voter,
                Token = token
            };

            foreach (var position in positions)
            {
                var approved = await _candidates.GetApprovedByPositionAsync(position.Id);
                vm.Positions.Add(new BallotPosition
                {
                    Position = position,
                    Candidates = approved.Select(c => new BallotCandidate
                    {
                        CandidateId = c.Id,
                        FullName = c.FullName,
                        PartylistName = string.Empty
                    }).ToList()
                });
            }

            // Fill partylist names in one pass.
            var partyLookup = await GetPartylistNamesAsync(election.Id);
            var candidateParty = (await _candidates.GetByElectionAsync(election.Id))
                .ToDictionary(c => c.Id, c => c.PartylistId);
            foreach (var bp in vm.Positions)
            {
                foreach (var bc in bp.Candidates)
                {
                    if (candidateParty.TryGetValue(bc.CandidateId, out var partyId))
                    {
                        bc.PartylistName = partyLookup.GetValueOrDefault(partyId, string.Empty);
                    }
                }
            }

            return vm;
        }

        private async Task<Dictionary<string, string>> GetPartylistNamesAsync(string electionId)
        {
            var svc = HttpContext.RequestServices.GetRequiredService<PartylistService>();
            var lists = await svc.GetByElectionAsync(electionId);
            return lists.ToDictionary(p => p.Id, p => p.Name);
        }

        private string BaseUrl()
        {
            var b = _email.BaseUrl;
            return string.IsNullOrWhiteSpace(b) ? $"{Request.Scheme}://{Request.Host}" : b;
        }

        private static string MaskEmail(string email)
        {
            var at = email.IndexOf('@');
            if (at <= 1)
            {
                return email;
            }

            return $"{email[0]}{new string('*', Math.Max(1, at - 2))}{email[at - 1]}{email[at..]}";
        }
    }
}
