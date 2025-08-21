namespace STC.Logger.Infrastructure.MessageBrokers;

public record MessageBrokerOptions
{
    public string ExchangeName { get; init; } = null!;
    public string QueueName { get; init; } = null!;
    public int BatchSize { get; init; }
}