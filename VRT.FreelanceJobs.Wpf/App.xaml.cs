using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;
using VRT.FreelanceJobs.Wpf.Abstractions.Jobs;
using VRT.FreelanceJobs.Wpf.Persistence.Jobs;
using VRT.FreelanceJobs.Wpf.Services.Useme;

namespace VRT.FreelanceJobs.Wpf;

public sealed partial class App : Application, IDisposable
{
    private IHost? _host;
    private IHost CurrentHost => _host ??= InitHost();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        CurrentHost.Start();
        MainWindow = CurrentHost.Services.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    private static IHost InitHost()
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        var builder = Host.CreateApplicationBuilder();
        var services = builder.Services;
        var settings = LoadAppSettings(DependencyInjection.GetConfiguration());

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
        }
        services.AddSingleton<MainWindow>();
        services.AddTransient<MainWindowViewModel>();


        return builder.Build();
    }
    private static AppSettings LoadAppSettings(IConfiguration configuration)
    {
        var settings = new AppSettings();
        configuration.Bind(settings);
        return settings;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel(); //this should stop all background services

        _host?.Dispose();
        //(Services.GetService<IEnumerable<IHostedService>>() ?? [])
        //    .OfType<IDisposable>()
        //    .ToList()
        //    .ForEach(d => d.Dispose());
    }
}
