using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Refit;
using System.Net.Http;
using VRT.FreelanceJobs.Wpf.Options;
using VRT.FreelanceJobs.Wpf.Workers;

namespace VRT.FreelanceJobs.Wpf.Services.Useme;

internal sealed class UsemeWebJobService : IUsemeJobsService, IAsyncDisposable
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
    private IBrowser? _browser;
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

    public async Task<ApiResponse<string>> GetJobEntries(string category, string? page = null)
    {
        var url = $"{_options.BaseUri}/pl/jobs/category/{category}/?page={page}";

        var result = await Result.Success()
            .TapTry(() => _browserDownloaderService.EnsureInitialized(CancellationToken.None))
            .MapTry(OpenHeadlessBrowser)
            .MapTry(browser => OpenPage(browser, url))
            .TapTry(WaitForJobsDiv)
            .MapTry(GetHtmlAndClose)
            .Map(html => html.ToSuccessApiResponse())
            .TapError(err => _logger.LogError("Error occured when fetching jobs from useme. {Message}", err))
            .Compensate(err => err.ToInternalApiError());
        //await Result.Try(() => browser.CloseAsync());
        return result.Value; //always success here
    }
    private async Task<string> GetHtmlAndClose(IPage page)
    {
        var html = await page.ContentAsync();
        await page.CloseAsync();
        return html;
    }
    private async Task WaitForJobsDiv(IPage page)
    {
        await page.WaitForSelectorAsync("div.jobs");
    }
    private async Task<IBrowser> OpenHeadlessBrowser()
    {
        if (_browser is not null && _browser.IsConnected)
        {
            return _browser;
        }
        try
        {
            await _browserSemaphore.WaitAsync();
            if (_browser is not null)
            {
                await _browser.DisposeAsync();
                _browser = null;
            }
            await _browserDownloaderService.EnsureInitialized(CancellationToken.None);
            var playwright = _playwright ??= await Playwright.CreateAsync();
            _browser = await playwright.Chromium.LaunchAsync(LaunchOptions);
            return _browser;
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
            WaitUntil = WaitUntilState.NetworkIdle
        });
        return usemePage;
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();
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