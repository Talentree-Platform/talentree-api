using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Talentree.Repository.Data;

namespace Talentree.Service.Messaging
{
    public class RabbitMQInitializer : IHostedService
    {
        private readonly RabbitMQConnectionManager _connectionManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMQInitializer> _logger;

        private const string ExchangeName = "talentree.events";
        private const string DlxName = "talentree.dlx";
        private const string RetryExchangeName = "talentree.retry.exchange";

        private const string InteractionQueue = "talentree.interaction.queue";
        private const string SentimentQueue = "talentree.ai.sentiment.queue";
        private const string TriageQueue = "talentree.ai.triage.queue";
        private const string ProductQueue = "talentree.ai.product.queue";
        private const string ProfileQueue = "talentree.ai.profile.queue";
        private const string ChurnQueue = "talentree.ai.churn.queue";
        private const string CustomerRecommendQueue = "talentree.ai.customer.recommend.queue";
        private const string OwnerRecommendQueue = "talentree.ai.owner.recommend.queue";
        private const string RetrainQueue = "talentree.ai.retrain.queue";
        private const string FraudQueue = "talentree.ai.fraud.queue";
        private const string RequestQueue = "talentree.ai.request.queue";
        private const string AnomalyQueue = "talentree.ai.anomaly.queue";
        private const string RetryDelayQueue = "talentree.retry.delay.queue";

        public RabbitMQInitializer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            ILogger<RabbitMQInitializer> logger)
        {
            _connectionManager = connectionManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitMQInitializer is starting topology setup...");

            try
            {
                // 1. Initialize RabbitMQ topology
                SetupRabbitTopology();

                // 2. Setup Database Idempotency Table
                await SetupDatabaseIdempotencyTableAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to initialize RabbitMQ topology or database table.");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private void SetupRabbitTopology()
        {
            using var channel = _connectionManager.CreateModel();

            _logger.LogInformation("Declaring exchanges: {EventsExchange}, {DlxExchange}, {RetryExchange}", ExchangeName, DlxName, RetryExchangeName);
            // Declare exchanges
            channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
            channel.ExchangeDeclare(DlxName, ExchangeType.Direct, durable: true);
            channel.ExchangeDeclare(RetryExchangeName, ExchangeType.Topic, durable: true);

            // Declare DLQs
            _logger.LogInformation("Declaring dead letter queues...");
            channel.QueueDeclare("talentree.interaction.dlq", durable: true, exclusive: false, autoDelete: false);
            channel.QueueDeclare("talentree.ai.dlq", durable: true, exclusive: false, autoDelete: false);

            channel.QueueBind("talentree.interaction.dlq", DlxName, "interaction.log.dead");
            channel.QueueBind("talentree.ai.dlq", DlxName, "ai.task.dead");

            // Declare retry delay queue (dead-letters back to main exchange)
            _logger.LogInformation("Declaring retry delay queue: {RetryDelayQueue}", RetryDelayQueue);
            var retryDelayArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", ExchangeName }
            };
            channel.QueueDeclare(RetryDelayQueue, durable: true, exclusive: false, autoDelete: false, arguments: retryDelayArgs);
            channel.QueueBind(RetryDelayQueue, RetryExchangeName, "#"); // binds all retry routing keys

            // Declare main queues with dead-letter configurations
            DeclareAndBindMainQueue(channel, InteractionQueue, "interaction.log", "interaction.log.dead");
            DeclareAndBindMainQueue(channel, SentimentQueue, "ai.sentiment", "ai.task.dead");
            DeclareAndBindMainQueue(channel, TriageQueue, "ai.triage", "ai.task.dead");
            DeclareAndBindMainQueue(channel, ProductQueue, "ai.product", "ai.task.dead");
            DeclareAndBindMainQueue(channel, ProfileQueue, "ai.profile", "ai.task.dead");
            DeclareAndBindMainQueue(channel, ChurnQueue, "ai.churn", "ai.task.dead");
            DeclareAndBindMainQueue(channel, CustomerRecommendQueue, "ai.customer.recommend", "ai.task.dead");
            DeclareAndBindMainQueue(channel, OwnerRecommendQueue, "ai.owner.recommend", "ai.task.dead");
            DeclareAndBindMainQueue(channel, RetrainQueue, "ai.retrain", "ai.task.dead");
            DeclareAndBindMainQueue(channel, FraudQueue, "ai.fraud", "ai.task.dead");
            DeclareAndBindMainQueue(channel, RequestQueue, "ai.request", "ai.task.dead");
            DeclareAndBindMainQueue(channel, AnomalyQueue, "ai.anomaly", "ai.task.dead");

            _logger.LogInformation("RabbitMQ topology initialized successfully.");
        }

        private void DeclareAndBindMainQueue(IModel channel, string queueName, string routingKey, string deadLetterRoutingKey)
        {
            _logger.LogInformation("Declaring main queue '{QueueName}' bound to routing key '{RoutingKey}' with dead letter '{DeadLetterRoutingKey}'", queueName, routingKey, deadLetterRoutingKey);
            
            var arguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", DlxName },
                { "x-dead-letter-routing-key", deadLetterRoutingKey }
            };

            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
            channel.QueueBind(queueName, ExchangeName, routingKey);
        }

        private async Task SetupDatabaseIdempotencyTableAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Verifying/creating ProcessedMessages database table for idempotency layer...");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TalentreeDbContext>();

            const string createTableSql = @"
                IF OBJECT_ID('ProcessedMessages', 'U') IS NULL
                BEGIN
                    CREATE TABLE ProcessedMessages (
                        MessageId UNIQUEIDENTIFIER PRIMARY KEY,
                        ProcessedAt DATETIME2 NOT NULL
                    );
                END";

            await dbContext.Database.ExecuteSqlRawAsync(createTableSql, cancellationToken);
            _logger.LogInformation("ProcessedMessages table verified successfully.");
        }
    }
}
