using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimecardService.Messaging.Abstractions;
using TimecardService.Messaging.RabbitMQ;

namespace TimecardService.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection("RabbitMq")
            .Get<RabbitMqOptions>()!;

        services.AddSingleton(options);
        services.AddSingleton<RabbitMqConnection>();
        services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }
}