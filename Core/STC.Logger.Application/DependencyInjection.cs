using Microsoft.Extensions.DependencyInjection;

namespace STC.Logger.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        return services;
    }
}