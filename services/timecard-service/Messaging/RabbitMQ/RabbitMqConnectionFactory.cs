using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using TimecardService.Messaging.RabbitMQ;

namespace TimecardService.Messaging.RabbitMQ;

public class RabbitMqConnectionFactory
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConnectionFactory> _logger;

    public RabbitMqConnectionFactory(
        RabbitMqOptions options,
        ILogger<RabbitMqConnectionFactory> logger)
    {
        _options = options;
        _logger = logger;
    }

    public IConnection CreateConnectionWithRetry()
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryForever(
                retryAttempt => TimeSpan.FromSeconds(Math.Min(30, retryAttempt * 2)),
                (ex, delay) =>
                {
                    _logger.LogWarning(
                        ex,
                        "RabbitMQ not reachable at {Host}:{Port}. Retrying in {Delay}s",
                        _options.HostName,
                        _options.Port,
                        delay.TotalSeconds);
                });

        return retryPolicy.Execute(() =>
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                DispatchConsumersAsync = true
            };

            return factory.CreateConnection();
        });
    }
}
