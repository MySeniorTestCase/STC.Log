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
        services.Configure<MessageBrokerOptions>(config: configuration.GetRequiredSection("Source:MessageBroker"));

        {
            services.Configure<RabbitMqConnectionOptions>(config: configuration.GetRequiredSection("ConnectionStrings:RabbitMQ"));

            services.AddHostedService<RabbitMqHostedService>();
        }

        services.AddSingleton<ILogStorageService, SeqLogStorageManager>();


        return services;
    }
}