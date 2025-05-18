using CartService.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CartService.Infrastructure.Messaging
{
    public class RabbitMqItemUpdateListener : BackgroundService
    {
        private readonly ICartService _cartService;
        private readonly ILogger<RabbitMqItemUpdateListener> _logger;
        private IConnection? _connection;
        private IChannel? _channel;
        private const string ExchangeName = "catalog.item.updated";
        private const string QueueName = "cart.update.queue";
        private const string RetryQueueName = "cart.retry.queue";

        public RabbitMqItemUpdateListener(ICartService cartService, ILogger<RabbitMqItemUpdateListener> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false);
            await _channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", string.Empty },
                { "x-dead-letter-routing-key", RetryQueueName }
            });

            await _channel.QueueDeclareAsync(queue: RetryQueueName, durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object?>
            {
                { "x-message-ttl", 10000 },
                { "x-dead-letter-exchange", string.Empty },
                { "x-dead-letter-routing-key", QueueName }
            });

            await _channel.QueueBindAsync(queue: QueueName, exchange: ExchangeName, routingKey: string.Empty);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel is null)
                throw new InvalidOperationException("Listener not initialized.");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    //throw new Exception("Simulated failure.");
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var productUpdate = JsonSerializer.Deserialize<ProductUpdatedMessage>(message);

                    if (productUpdate != null)
                    {
                        _logger.LogInformation($"Received product update: {productUpdate.Id} - {productUpdate.Name}");
                        await _cartService.ApplyProductUpdate(productUpdate);
                    }

                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process product update message");
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}
