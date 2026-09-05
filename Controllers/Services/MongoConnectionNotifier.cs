using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;

namespace VotingSystem.Controllers.Services
{
    public sealed class MongoConnectionNotifier
    {
        private readonly ILogger<MongoConnectionNotifier> _logger;
        private volatile bool _isConnected;

        public MongoConnectionNotifier(ILogger<MongoConnectionNotifier> logger)
        {
            _logger = logger;
        }

        public bool IsConnected => _isConnected;

        public event EventHandler<bool>? ConnectionStateChanged;

        public void Configure(ClusterBuilder builder)
        {
            builder.Subscribe<ClusterOpenedEvent>(e =>
            {
                SetState(true, $"Cluster connection opened ({e.ClusterId}).");
            });

            builder.Subscribe<ClusterClosedEvent>(e =>
            {
                SetState(false, $"Cluster connection closed ({e.ClusterId}).");
            });

            builder.Subscribe<ServerHeartbeatSucceededEvent>(e =>
            {
                if (!_isConnected)
                {
                    SetState(true, $"Server heartbeat succeeded ({e.ConnectionId.ServerId}); connection restored.");
                }
            });

            builder.Subscribe<ServerHeartbeatFailedEvent>(e =>
            {
                SetState(false, $"Server heartbeat failed ({e.ConnectionId.ServerId}): {e.Exception.Message}");
            });
        }

        private void SetState(bool connected, string message)
        {
            var changed = _isConnected != connected;
            _isConnected = connected;

            if (connected)
            {
                _logger.LogInformation("MongoDB: {Message}", message);
            }
            else
            {
                _logger.LogWarning("MongoDB: {Message}", message);
            }

            if (changed)
            {
                ConnectionStateChanged?.Invoke(this, connected);
            }
        }
    }
}
