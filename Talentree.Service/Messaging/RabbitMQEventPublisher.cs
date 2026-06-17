using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging
{
    public class RabbitMQEventPublisher : IEventPublisher
    {
        private readonly RabbitMQConnectionManager _connectionManager;
        private readonly ILogger<RabbitMQEventPublisher> _logger;
        private readonly RetryPolicy _publishRetryPolicy;
        private const string ExchangeName = "talentree.events";

        public RabbitMQEventPublisher(RabbitMQConnectionManager connectionManager, ILogger<RabbitMQEventPublisher> logger)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Setup Polly retry policy for transient publishing/connection errors
            _publishRetryPolicy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<TimeoutException>()
                .Or<IOException>()
                .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "RabbitMQ publisher retry {RetryCount} after {Delay}ms due to exception.", retryCount, timeSpan.TotalMilliseconds);
                    });
        }

        public Task PublishAsync<T>(string routingKey, T message) where T : class
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            _publishRetryPolicy.Execute(() =>
            {
                using var channel = _connectionManager.CreateModel();
                
                // Enable Publisher Confirms
                channel.ConfirmSelect();

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                // Extract or generate Correlation ID
                string correlationId = (message is BaseMessage baseMsg) 
                    ? baseMsg.CorrelationId 
                    : Guid.NewGuid().ToString();

                properties.CorrelationId = correlationId;

                _logger.LogDebug("Publishing message to exchange '{Exchange}' with routing key '{RoutingKey}' and correlation ID '{CorrelationId}'", ExchangeName, routingKey, correlationId);

                channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);

                // Block and wait for confirm (timeout: 5 seconds)
                channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
                
                _logger.LogDebug("Message confirmed by RabbitMQ broker. Correlation ID: {CorrelationId}", correlationId);
            });

            return Task.CompletedTask;
        }
    }
}
