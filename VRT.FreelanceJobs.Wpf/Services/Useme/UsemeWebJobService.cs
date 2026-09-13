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
    private readonly PlaywrightOptions? _playwrightOptions;
    private readonly IBrowserDownloaderService _browserDownloaderService;
    private readonly ILogger<UsemeWebJobService> _logger;
    private IPlaywright? _playwright;
    private IBrowser? _cdpBrowser;

    public UsemeWebJobService(
        AppSettings appSettings,
        IBrowserDownloaderService browserDownloaderService,
        ILogger<UsemeWebJobService> logger)
    {
        ArgumentNullException.ThrowIfNull(appSettings?.Useme);
        _options = appSettings.Useme;
        _playwrightOptions = appSettings.Playwright;
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
            .MapTry(OpenBrowser)
            .Tap(browser => RegisterBrowserCleanup(browser, cleanup))
            .MapTry(browser => OpenPage(browser, url, cleanup))
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

    private async Task<IBrowser> OpenBrowser()
    {
        try
        {
            await _browserSemaphore.WaitAsync();
            if (_playwrightOptions?.HasCdpEndpoint is true)
            {
                return await GetOrConnectCdpBrowser();
            }

            await _browserDownloaderService.EnsureInitialized(CancellationToken.None);
            var playwright = _playwright ??= await Playwright.CreateAsync();
            return await playwright.Chromium.LaunchAsync(LaunchOptions);
        }
        finally
        {
            _browserSemaphore.Release();
        }
    }

    private async Task<IBrowser> GetOrConnectCdpBrowser()
    {
        if (_cdpBrowser is { IsConnected: true })
        {
            return _cdpBrowser;
        }

        var endpoint = _playwrightOptions!.CdpEndpoint!.Trim();
        var playwright = _playwright ??= await Playwright.CreateAsync();
        _logger.LogInformation("Connecting Playwright to CDP endpoint {CdpEndpoint}", endpoint);
        _cdpBrowser = await playwright.Chromium.ConnectOverCDPAsync(endpoint);
        return _cdpBrowser;
    }

    private IBrowser RegisterBrowserCleanup(IBrowser browser, DisposablesSet cleanup)
    {
        // Closing a CDP-attached browser would shut down the user's Chrome/Edge.
        if (_playwrightOptions?.HasCdpEndpoint is not true)
        {
            browser.AsyncDisposeWith(cleanup);
        }
        return browser;
    }

    private async Task<IPage> OpenPage(IBrowser browser, string url, DisposablesSet cleanup)
    {
        var usemePage = _playwrightOptions?.HasCdpEndpoint is true
            ? await OpenCdpPage(browser)
            : await browser.NewPageAsync(new BrowserNewPageOptions()
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36",
                ViewportSize = new ViewportSize() { Width = 800, Height = 600 }
            });
        if (_playwrightOptions?.HasCdpEndpoint is true)
        {
            cleanup.Add(new PageCleanup(usemePage));
        }

        await usemePage.GotoAsync(url, new PageGotoOptions()
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        return usemePage;
    }

    private static async Task<IPage> OpenCdpPage(IBrowser browser)
    {
        var context = browser.Contexts.Count > 0
            ? browser.Contexts[0]
            : await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.SetViewportSizeAsync(800, 600);
        return page;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        // Do not dispose the CDP browser: IBrowser.DisposeAsync closes the remote Chrome/Edge.
        _cdpBrowser = null;
        _playwright?.Dispose();
        GC.SuppressFinalize(this);
    }
}
file sealed class PageCleanup(IPage page) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        if (page.IsClosed)
        {
            return;
        }
        await page.CloseAsync();
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