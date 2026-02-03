namespace TimecardService.Messaging.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<T>(string routingKey, T message);
}