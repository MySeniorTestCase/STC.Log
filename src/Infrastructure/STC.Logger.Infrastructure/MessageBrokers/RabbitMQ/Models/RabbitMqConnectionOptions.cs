namespace STC.Logger.Infrastructure.MessageBrokers.RabbitMQ.Models;

public record RabbitMqConnectionOptions
{
    public string HostName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int Port { get; set; }
}