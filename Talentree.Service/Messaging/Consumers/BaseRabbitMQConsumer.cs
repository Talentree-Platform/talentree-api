using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Talentree.Core.Entities;
using Talentree.Repository.Data;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public abstract class BaseRabbitMQConsumer<TMessage> : BackgroundService where TMessage : BaseMessage
    {
        private readonly RabbitMQConnectionManager _connectionManager;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger _logger;
        private readonly AsyncRetryPolicy _consumerRetryPolicy;

        private const string RetryExchangeName = "talentree.retry.exchange";
        private const string DlxName = "talentree.dlx";

        protected abstract string QueueName { get; }
        protected abstract string RoutingKey { get; }

        private IModel? _channel;

        protected BaseRabbitMQConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Polly retry policy for transient database/service failures
            _consumerRetryPolicy = Policy
                .Handle<DbUpdateConcurrencyException>()
                .Or<DbUpdateException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Consumer transient retry {RetryCount} for queue {QueueName} after {Delay}ms.", retryCount, QueueName, timeSpan.TotalMilliseconds);
                    });
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background consumer for queue {QueueName} is starting.", QueueName);

            _channel = _connectionManager.CreateModel();
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var correlationId = ea.BasicProperties.CorrelationId ?? Guid.NewGuid().ToString();

                TMessage? message = null;
                try
                {
                    message = JsonSerializer.Deserialize<TMessage>(messageJson);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize message for queue {QueueName}. Body: {MessageJson}. Routing to DLQ.", QueueName, messageJson);
                    // Invalid format: Reject and move to DLQ
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                if (message == null)
                {
                    _logger.LogWarning("Deserialized message is null for queue {QueueName}. Routing to DLQ.", QueueName);
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                // Attach properties if missing
                if (string.IsNullOrEmpty(message.CorrelationId)) message.CorrelationId = correlationId;

                _logger.LogInformation("Processing message {MessageId} in queue {QueueName} (Correlation: {CorrelationId})", message.MessageId, QueueName, message.CorrelationId);

                try
                {
                    await ProcessWithIdempotencyAndRetriesAsync(message, ea);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatal error executing message {MessageId} in queue {QueueName}. Initiating retry/DLQ fallback.", message.MessageId, QueueName);
                    HandleFailureFallback(ea, message);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private async Task ProcessWithIdempotencyAndRetriesAsync(TMessage message, BasicDeliverEventArgs ea)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TalentreeDbContext>();

            // 1. Idempotency Check (Check if message is already processed)
            var alreadyProcessed = await dbContext.ProcessedMessages
                .AnyAsync(pm => pm.MessageId == message.MessageId);

            if (alreadyProcessed)
            {
                _logger.LogWarning("Message {MessageId} was already processed. Skipping duplicate execution.", message.MessageId);
                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                return;
            }

            // 2. Local execution with Polly Retries
            await _consumerRetryPolicy.ExecuteAsync(async () =>
            {
                // Execute implementation-specific processing
                await ProcessMessageAsync(message, scope.ServiceProvider);
            });

            // 3. Save to Idempotency Table and commit transaction
            dbContext.ProcessedMessages.Add(new ProcessedMessage
            {
                MessageId = message.MessageId,
                ProcessedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            // 4. Acknowledge message success
            _channel!.BasicAck(ea.DeliveryTag, multiple: false);
            _logger.LogInformation("Successfully processed and acknowledged message {MessageId} in queue {QueueName}.", message.MessageId, QueueName);
        }

        protected abstract Task ProcessMessageAsync(TMessage message, IServiceProvider serviceProvider);

        private void HandleFailureFallback(BasicDeliverEventArgs ea, TMessage message)
        {
            try
            {
                var headers = ea.BasicProperties.Headers ?? new Dictionary<string, object>();
                int retryCount = 0;

                if (headers.TryGetValue("x-retry-count", out var value))
                {
                    if (value is byte[] bytes)
                    {
                        int.TryParse(Encoding.UTF8.GetString(bytes), out retryCount);
                    }
                    else
                    {
                        retryCount = Convert.ToInt32(value);
                    }
                }

                if (retryCount < 3)
                {
                    int nextRetry = retryCount + 1;
                    int delayMs = (int)Math.Pow(2, nextRetry) * 1000;

                    _logger.LogWarning("Message {MessageId} failed processing. Republishing to retry exchange. Attempt: {RetryAttempt}/3, Delay: {DelayMs}ms", message.MessageId, nextRetry, delayMs);

                    var retryProperties = _channel!.CreateBasicProperties();
                    retryProperties.Persistent = true;
                    retryProperties.CorrelationId = message.CorrelationId;

                    var newHeaders = new Dictionary<string, object>();
                    foreach (var kvp in headers)
                    {
                        newHeaders[kvp.Key] = kvp.Value;
                    }
                    newHeaders["x-retry-count"] = nextRetry;
                    retryProperties.Headers = newHeaders;

                    // Set message expiration for delay queue
                    retryProperties.Expiration = delayMs.ToString();

                    // Publish to retry exchange with the same routing key
                    _channel.BasicPublish(
                        exchange: RetryExchangeName,
                        routingKey: ea.RoutingKey,
                        basicProperties: retryProperties,
                        body: ea.Body);

                    // Acknowledge the old message so it doesn't block
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                else
                {
                    _logger.LogError("Message {MessageId} has exceeded max retries in queue {QueueName}. Rejecting to DLQ.", message.MessageId, QueueName);
                    // nack with requeue: false to route to DLX/DLQ
                    _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
            }
            catch (Exception fallbackEx)
            {
                _logger.LogCritical(fallbackEx, "Failed to execute retry/DLQ fallback for message {MessageId}.", message.MessageId);
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            base.Dispose();
        }
    }
}
