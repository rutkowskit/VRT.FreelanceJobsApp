using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Refit;
using System.Net.Http;
using VRT.FreelanceJobs.Wpf.Options;

namespace VRT.FreelanceJobs.Wpf.Services.Useme;

internal sealed class UsemeWebJobService : IUsemeJobsService
{
    private static readonly LaunchOptions LaunchOptions = new()
    {
        Headless = true,
        Browser = SupportedBrowser.Chrome,
        Args = new[]
            {
                "--no-sandbox", // Often needed for headless stability (use cautiously in production)
                "--disable-setuid-sandbox",
                "--disable-blink-features=AutomationControlled" // Helps evade detection
            }
    };

    private readonly UsemeOptions _options;
    private readonly ILogger<UsemeWebJobService> _logger;

    public UsemeWebJobService(
        AppSettings appSettings,
        ILogger<UsemeWebJobService> logger)
    {
        ArgumentNullException.ThrowIfNull(appSettings?.Useme);
        _options = appSettings.Useme;

        _logger = logger;
    }

    public string SourceName => UsemeOptions.SourceName;

    public async Task<ApiResponse<string>> GetJobEntries(string category, string? page = null)
    {
        // Ensure PuppeteerSharp has the browser binaries
        BrowserFetcher browserFetcher = new();
        await browserFetcher.DownloadAsync();

        await using var browser = await Puppeteer.LaunchAsync(LaunchOptions);
        await using var usemePage = await browser.NewPageAsync();
        // Set a realistic user agent to mimic a real browser
        await usemePage.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");

        // Spoof navigator.webdriver to undefined (evades common bot detection)
        await usemePage.EvaluateExpressionAsync("Object.defineProperty(navigator, 'webdriver', { get: () => undefined });");

        // Set a viewport to simulate a desktop window (optional but helps with responsive sites)
        await usemePage.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });


        var url = $"{_options.BaseUri}/pl/jobs/category/{category}/?page={page}";
        var navigationOptions = new NavigationOptions
        {
            WaitUntil = [WaitUntilNavigation.Networkidle2] // Or use Networkidle2 for stricter idle check
        };


        await usemePage.GoToAsync(url, navigationOptions);
        await usemePage.WaitForSelectorAsync("div.jobs", new WaitForSelectorOptions { Timeout = 5000 });



        var html = await usemePage.GetContentAsync();
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(html)
        };

        return new ApiResponse<string>(response, html, new());
    }
}
