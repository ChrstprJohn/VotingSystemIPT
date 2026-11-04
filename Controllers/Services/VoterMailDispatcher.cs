using System.Threading.Channels;
using VotingSystem.Models.Domain;

namespace VotingSystem.Controllers.Services
{
    /// <summary>One queued batch of voter invitation emails to send in the background.</summary>
    public sealed record VoterMailJob(
        IReadOnlyList<Voter> Recipients,
        string ElectionTitle,
        string Link);

    /// <summary>
    /// Sends voter invitation email off the request thread. Publishing an election
    /// (or pressing "Email invitations") enqueues a <see cref="VoterMailJob"/> and
    /// returns immediately; this hosted service drains the queue and sends each
    /// message through <see cref="VoterMailService"/> one at a time. Without this,
    /// a class-sized voter list makes the Publish request hang past the browser's
    /// fetch timeout ("failed to fetch").
    /// </summary>
    public sealed class VoterMailDispatcher : BackgroundService
    {
        private readonly Channel<VoterMailJob> _queue =
            Channel.CreateUnbounded<VoterMailJob>(new UnboundedChannelOptions { SingleReader = true });

        private readonly VoterMailService _mail;
        private readonly ILogger<VoterMailDispatcher> _logger;

        public VoterMailDispatcher(VoterMailService mail, ILogger<VoterMailDispatcher> logger)
        {
            _mail = mail;
            _logger = logger;
        }

        /// <summary>Queues a batch. Returns at once; delivery happens in the background.</summary>
        public void Enqueue(VoterMailJob job) => _queue.Writer.TryWrite(job);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                var sent = 0;
                foreach (var voter in job.Recipients)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        if (await _mail.SendInvitationAsync(voter, job.ElectionTitle, job.Link))
                        {
                            sent++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Voter invitation email threw for {Email}", voter.Email);
                    }
                }

                _logger.LogInformation(
                    "Voter invitation batch for \"{Title}\": {Sent}/{Total} delivered.",
                    job.ElectionTitle, sent, job.Recipients.Count);
            }
        }
    }
}
