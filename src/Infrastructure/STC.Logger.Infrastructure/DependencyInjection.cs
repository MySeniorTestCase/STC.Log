using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STC.Logger.Application;
using STC.Logger.Application.Features.LogStorages.Services;
using STC.Logger.Infrastructure.LogStorages;
using STC.Logger.Infrastructure.MessageBrokers;
using STC.Logger.Infrastructure.MessageBrokers.RabbitMQ;
using STC.Logger.Infrastructure.MessageBrokers.RabbitMQ.Models;

namespace STC.Logger.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MessageBrokerOptions>(config: configuration.GetSection("Source:MessageBroker") ??
                                                         throw new InvalidOperationException(
                                                             "Message Broker configuration is not set."));

        {
            services.Configure<RabbitMqConnectionOptions>(
                config: configuration.GetSection("ConnectionStrings:RabbitMQ") ??
                        throw new InvalidOperationException(
                            "RabbitMQ connection string is not configured."));

            services.AddHostedService<RabbitMqHostedService>();
        }

        services.AddSingleton<ILogStorageService, SeqLogStorageManager>();


        return services;
    }
}