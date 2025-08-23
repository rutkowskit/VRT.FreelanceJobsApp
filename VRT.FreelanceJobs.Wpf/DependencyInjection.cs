using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VRT.FreelanceJobs.Wpf;
internal static partial class DependencyInjection
{
    private static IConfiguration? AppConfig;
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddAppConfiguration()
            .AddSerilogLogging();
        return services;
    }

    internal static IServiceCollection AddAppConfiguration(this IServiceCollection services)
    {
        services.AddSingleton(services => GetConfiguration());
        return services;
    }
    private static IConfiguration GetConfiguration()
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
