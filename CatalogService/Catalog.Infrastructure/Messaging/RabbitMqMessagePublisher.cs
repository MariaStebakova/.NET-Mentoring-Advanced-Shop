using System.Text;
using System.Text.Json;

using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;

using RabbitMQ.Client;

namespace Catalog.Infrastructure.Messaging
{
    public class RabbitMqMessagePublisher : IMessagePublisher, IAsyncDisposable
    {
        private IChannel? _channel;
        private IConnection? _connection;
        private const string ExchangeName = "catalog.item.updated";

        public async Task InitializeAsync()
        {
            var hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            if (string.IsNullOrEmpty(hostName))
            {
                return;
            }
            var factory = new ConnectionFactory { HostName = hostName };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false);
        }

        public async Task PublishProductUpdatedAsync(ProductUpdatedMessage item)
        {
            if (_channel is null)
                throw new InvalidOperationException("Publisher not initialized.");

            var message = JsonSerializer.Serialize(item);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                Persistent = true
            };

            await _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: string.Empty,
                mandatory: true,
                basicProperties: properties,
                body: body);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
        }
    }
}