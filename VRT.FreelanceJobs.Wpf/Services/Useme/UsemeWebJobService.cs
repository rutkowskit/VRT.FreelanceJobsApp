using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Refit;
using System.Net.Http;
using VRT.FreelanceJobs.Wpf.Helpers;
using VRT.FreelanceJobs.Wpf.Options;
using VRT.FreelanceJobs.Wpf.Workers;

namespace VRT.FreelanceJobs.Wpf.Services.Useme;

internal sealed class UsemeWebJobService : IUsemeJobsService, IAsyncDisposable, IDisposable
{
    private static readonly BrowserTypeLaunchOptions LaunchOptions = new()
    {
        Headless = true,
        Args = new[]
            {
                "--no-sandbox", // Often needed for headless stability (use cautiously in production)
                "--disable-setuid-sandbox",
                "--disable-blink-features=AutomationControlled" // Helps evade detection
            }
    };
    private SemaphoreSlim _browserSemaphore = new SemaphoreSlim(1, 1);

    private readonly UsemeOptions _options;
    private readonly IBrowserDownloaderService _browserDownloaderService;
    private readonly ILogger<UsemeWebJobService> _logger;
    private IPlaywright? _playwright;

    public UsemeWebJobService(
        AppSettings appSettings,
        IBrowserDownloaderService browserDownloaderService,
        ILogger<UsemeWebJobService> logger)
    {
        ArgumentNullException.ThrowIfNull(appSettings?.Useme);
        _options = appSettings.Useme;
        LaunchOptions.Headless = _options.ShowBrowserWindow is false;
        _browserDownloaderService = browserDownloaderService;
        _logger = logger;
    }

    public string SourceName => UsemeOptions.SourceName;

    public async Task<ApiResponse<string>> GetJobEntries(string category, string? page = null, CancellationToken cancellationToken = default)
    {
        var url = $"{_options.BaseUri}/pl/jobs/category/{category}/?page={page}";
        await using var cleanup = new DisposablesSet();
        var result = await Result.Success()
            .TapTry(() => _browserDownloaderService.EnsureInitialized(cancellationToken))
            .MapTry(OpenHeadlessBrowser)
            .Tap(browser => browser.AsyncDisposeWith(cleanup))
            .MapTry(browser => OpenPage(browser, url))
            .TapTry(WaitForJobsDiv)
            .MapTry(GetHtml)
            .Map(html => html.ToSuccessApiResponse())
            .TapError(err => _logger.LogError("Error occured when fetching jobs from useme. {Message}", err))
            .Compensate(err => err.ToInternalApiError());
        return result.Value; //always success here
    }
    private Task<string> GetHtml(IPage page)
        => page.ContentAsync();

    private Task WaitForJobsDiv(IPage page)
        => page.WaitForSelectorAsync("div.jobs");

    private async Task<IBrowser> OpenHeadlessBrowser()
    {
        try
        {
            await _browserSemaphore.WaitAsync();
            await _browserDownloaderService.EnsureInitialized(CancellationToken.None);
            var playwright = _playwright ??= await Playwright.CreateAsync();
            return await playwright.Chromium.LaunchAsync(LaunchOptions);
        }
        finally
        {
            _browserSemaphore.Release();
        }
    }

    private static async Task<IPage> OpenPage(IBrowser browser, string url)
    {
        var usemePage = await browser.NewPageAsync(new BrowserNewPageOptions()
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36",
            ViewportSize = new ViewportSize() { Width = 800, Height = 600 }
        });

        await usemePage.GotoAsync(url, new PageGotoOptions()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        return usemePage;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        _playwright?.Dispose();
        GC.SuppressFinalize(this);
    }
}
file static class ObjectExtensions
{
    public static async Task<ApiResponse<string>> ToSuccessApiResponse(this Task<string> objTask)
    {
        var obj = await objTask;
        return obj.ToSuccessApiResponse();
    }

    public static ApiResponse<string> ToInternalApiError(this string error)
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(error)
        };

        return new ApiResponse<string>(response, error, new());
    }

    public static ApiResponse<string> ToSuccessApiResponse(this string obj)
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(obj)
        };

        return new ApiResponse<string>(response, obj, new());
    }
}