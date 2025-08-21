using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using STC.Logger.Application;
using STC.Logger.Application.Features.LogStorages.Services;
using STC.Logger.Infrastructure.MessageBrokers.RabbitMQ.Models;

namespace STC.Logger.Infrastructure.MessageBrokers.RabbitMQ;

public class RabbitMqHostedService(
    ILogStorageService logStorageService,
    IOptions<RabbitMqConnectionOptions> rabbitMqConnectionOptions,
    IOptions<MessageBrokerOptions> messageBrokerOptions) : IHostedService
{
    private IChannel _channel = null!;
    private readonly ConcurrentDictionary<ulong, string> _deliveryTagsAndPayloads = [];
    private readonly StringBuilder _sb = new();

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        ConnectionFactory factory = new ConnectionFactory()
        {
            HostName = rabbitMqConnectionOptions.Value.HostName,
            Port = rabbitMqConnectionOptions.Value.Port,
            UserName = rabbitMqConnectionOptions.Value.UserName,
            Password = rabbitMqConnectionOptions.Value.Password
        };

        IConnection connection = await factory.CreateConnectionAsync(cancellationToken: cancellationToken);

        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(exchange: messageBrokerOptions.Value.ExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            cancellationToken: cancellationToken);

        QueueDeclareOk queueDeclareResult = await _channel.QueueDeclareAsync(
            queue: messageBrokerOptions.Value.QueueName, durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        string queueName = queueDeclareResult.QueueName;

        await _channel.QueueBindAsync(queue: queueName,
            exchange: messageBrokerOptions.Value.ExchangeName,
            routingKey: string.Empty,
            cancellationToken: cancellationToken);
    }

    private async Task RejectAllMessagesFromMemoryAsync(CancellationToken cancellationToken)
    {
        foreach (var message in _deliveryTagsAndPayloads)
            await _channel.BasicRejectAsync(deliveryTag: message.Key,
                requeue: true,
                cancellationToken: cancellationToken);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken: cancellationToken);

        AsyncEventingBasicConsumer consumer = new(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            string message = Encoding.UTF8.GetString(bytes: ea.Body.ToArray());

            _deliveryTagsAndPayloads.TryAdd(key: ea.DeliveryTag, value: message);

            if (_deliveryTagsAndPayloads.Count >= messageBrokerOptions.Value.BatchSize)
            {
                foreach (var currentMsg in _deliveryTagsAndPayloads)
                    _sb.AppendLine(value: currentMsg.Value);

                bool isSavedToLogStorage = await logStorageService.SaveAsync(logMessage: _sb.ToString(),
                    cancellationToken: cancellationToken);
                if (isSavedToLogStorage)
                    await _channel.BasicAckAsync(deliveryTag: _deliveryTagsAndPayloads.Keys.Max(),
                        multiple: true,
                        cancellationToken: cancellationToken);
                else
                    await RejectAllMessagesFromMemoryAsync(cancellationToken: cancellationToken);

                _sb.Clear();
                _deliveryTagsAndPayloads.Clear();
            }
        };

        await _channel.BasicConsumeAsync(queue: messageBrokerOptions.Value.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await RejectAllMessagesFromMemoryAsync(cancellationToken: cancellationToken);

        _deliveryTagsAndPayloads.Clear();
        _sb.Clear();

        await _channel.CloseAsync(cancellationToken: cancellationToken);
        await _channel.DisposeAsync();
    }
}