using RabbitMQ.Client;
using Microsoft.Extensions.Logging;

namespace TimecardService.Messaging.RabbitMQ;

public class RabbitMqConnection : IDisposable
{
    private readonly IConnection _connection;
    private readonly RabbitMqOptions _options;

    public RabbitMqConnection(RabbitMqOptions options, ILogger<RabbitMqConnection> logger)
    {
        _options = options;

        logger.LogWarning("🔥 RABBITMQ CONNECTION CONSTRUCTOR HIT");

        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();

        using var channel = _connection.CreateModel();

        logger.LogWarning("🔥 DECLARING EXCHANGE: {Exchange}", options.ExchangeName);

        channel.ExchangeDeclare(
            exchange: options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );
    }

    public IModel CreateChannel() => _connection.CreateModel();

    public void Dispose()
    {
        if (_connection.IsOpen)
            _connection.Close();
    }
}
