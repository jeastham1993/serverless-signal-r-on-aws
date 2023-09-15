namespace SignalR.Shared;

using Microsoft.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    public static IServiceCollection SetupSignalR(this IServiceCollection services)
    {
        // Setup SignalR & Redis Backplane
        var hostName = Environment.GetEnvironmentVariable("HOST_NAME") ?? "localhost";
        var portNumber = 6379;
        var password = Environment.GetEnvironmentVariable("CACHE_PASSWORD") ?? "";

        var connectionString = $"{hostName}:{portNumber}";

        if (!string.IsNullOrEmpty(password))
        {
            connectionString = $"{connectionString},password={password}";
        }

        if (string.IsNullOrEmpty(hostName))
        {
            services.AddSignalR();
        }
        else
        {
            services.AddSignalR()
                .AddStackExchangeRedis(connectionString, options =>
                {
                    options.Configuration.ChannelPrefix = "translation";
                });   
        }

        return services;
    }
}