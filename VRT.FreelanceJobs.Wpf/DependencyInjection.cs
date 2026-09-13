using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRT.FreelanceJobs.Wpf.Workers;

namespace VRT.FreelanceJobs.Wpf;

internal static partial class DependencyInjection
{
    private static IConfiguration? AppConfig;
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddAppConfiguration()
            .AddSerilogLogging()
            .AddSingleton<BrowserDownloaderService>()
            .AddSingleton<IBrowserDownloaderService>(p => p.GetRequiredService<BrowserDownloaderService>())
            .AddHostedService(p => p.GetRequiredService<BrowserDownloaderService>());
        return services;
    }

    internal static IServiceCollection AddAppConfiguration(this IServiceCollection services)
    {
        services.AddSingleton(services => GetConfiguration());
        return services;
    }
    internal static IConfiguration GetConfiguration()
    {
        if (AppConfig is not null)
        {
            return AppConfig;
        }
        var builder = new ConfigurationBuilder()
               .SetBasePath(AppContext.BaseDirectory)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        AppConfig = builder.Build();
        return AppConfig;
    }
}
