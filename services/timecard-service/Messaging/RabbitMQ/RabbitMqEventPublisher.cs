using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TimecardService.Messaging.Abstractions;

namespace TimecardService.Messaging.RabbitMQ;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMqConnection _connection;
    private readonly RabbitMqOptions _options;

    public RabbitMqEventPublisher(
        RabbitMqConnection connection,
        RabbitMqOptions options)
    {
        _connection = connection;
        _options = options;
    }

    public Task PublishAsync<T>(string routingKey, T message)
    {
        using var channel = _connection.CreateChannel();

        channel.ExchangeDeclare(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message));

        channel.BasicPublish(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }
}
