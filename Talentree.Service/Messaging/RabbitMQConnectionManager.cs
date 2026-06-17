using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Talentree.Service.Messaging
{
    public class RabbitMQConnectionManager : IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMQConnectionManager> _logger;
        private IConnection? _connection;
        private readonly object _lock = new object();
        private bool _disposed;

        public RabbitMQConnectionManager(IConfiguration configuration, ILogger<RabbitMQConnectionManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _connectionFactory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            var uriString = configuration["RabbitMQ:Uri"];
            if (!string.IsNullOrEmpty(uriString))
            {
                _logger.LogInformation("Configuring RabbitMQ using Connection Uri.");
                _connectionFactory.Uri = new Uri(uriString);
            }
            else
            {
                _logger.LogInformation("Configuring RabbitMQ using individual settings.");
                _connectionFactory.HostName = configuration["RabbitMQ:Host"] ?? "localhost";
                _connectionFactory.UserName = configuration["RabbitMQ:Username"] ?? "guest";
                _connectionFactory.Password = configuration["RabbitMQ:Password"] ?? "guest";
                
                var vhost = configuration["RabbitMQ:VirtualHost"];
                _connectionFactory.VirtualHost = string.IsNullOrEmpty(vhost) ? "/" : vhost;

                if (int.TryParse(configuration["RabbitMQ:Port"], out var port))
                {
                    _connectionFactory.Port = port;
                }

                if (bool.TryParse(configuration["RabbitMQ:UseSsl"], out var useSsl) && useSsl)
                {
                    _connectionFactory.Ssl.Enabled = true;
                    _connectionFactory.Ssl.ServerName = _connectionFactory.HostName;
                }
            }
        }

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        public IConnection GetConnection()
        {
            if (IsConnected) return _connection!;

            lock (_lock)
            {
                if (IsConnected) return _connection!;

                _logger.LogInformation("Creating a new RabbitMQ connection...");
                _connection = _connectionFactory.CreateConnection();

                _connection.ConnectionShutdown += (sender, e) => _logger.LogWarning("RabbitMQ connection shutdown. ReplyCode: {ReplyCode}, ReplyText: {ReplyText}", e.ReplyCode, e.ReplyText);
                _connection.CallbackException += (sender, e) => _logger.LogError(e.Exception, "RabbitMQ callback exception occurred.");
                _connection.ConnectionBlocked += (sender, e) => _logger.LogWarning("RabbitMQ connection blocked. Reason: {Reason}", e.Reason);

                return _connection;
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                GetConnection();
            }

            return GetConnection().CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _connection?.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex, "IOException occurred during RabbitMQ connection disposal.");
            }
        }
    }
}
