using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;
using System.Windows;
using VRT.FreelanceJobs.Wpf.Abstractions.Jobs;
using VRT.FreelanceJobs.Wpf.Persistence.Jobs;
using VRT.FreelanceJobs.Wpf.Services.Useme;

namespace VRT.FreelanceJobs.Wpf;

public partial class App : Application
{
    private const string AppSettingsFileName = "appsettings.json";
    private static IServiceProvider? _services;
    public static IServiceProvider Services => _services ??= InitServices();

    protected override void OnStartup(StartupEventArgs e)
    {

        MainWindow = Services.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }
    private static ServiceProvider InitServices()
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        var services = new ServiceCollection();
        var settings = LoadAppSettings();
        services
            .AddInfrastructure()
            .AddSingleton<IJobsRepository, JsonFileRepository>()
            .AddSingleton(p => settings);
        if (settings.Useme is not null)
        {
            services
                .AddTransient<IJobsService, UsemeJobsServiceAdapter>()
                .AddSingleton<IUsemeJobsService, UsemeWebJobService>()
                ;
            //.AddRefitClient<IUsemeJobsService>()
            //.ConfigureHttpClient(client =>
            //{
            //    client.BaseAddress = new Uri(settings.Useme.BaseUri);
            //    client.Timeout = TimeSpan.FromSeconds(15);
            //});
        }
        //Not yet implemented !
        //if (settings.Upwork is not null)
        //{
        //    services
        //        .AddTransient<IJobsService, UpworkJobsServiceAdapter>()
        //        .AddRefitClient<IUpworkJobsService>()
        //        .ConfigureHttpClient(client =>
        //        {
        //            client.BaseAddress = new Uri(settings.Upwork.BaseUri);
        //            client.Timeout = TimeSpan.FromSeconds(15);
        //        });
        //}
        services.AddSingleton<MainWindow>();
        services.AddTransient<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
    private static AppSettings LoadAppSettings()
    {
        if (File.Exists(AppSettingsFileName) == false)
        {
            return AppSettings.Empty;
        }
        try
        {
            var json = File.ReadAllText(AppSettingsFileName, Encoding.UTF8);
            var result = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
            return result ?? AppSettings.Empty;
        }
        catch
        {
            return AppSettings.Empty;
        }
    }
}
