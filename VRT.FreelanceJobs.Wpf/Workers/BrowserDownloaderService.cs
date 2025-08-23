using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VRT.FreelanceJobs.Wpf.Workers;
internal class BrowserDownloaderService(ILogger<BrowserDownloaderService> logger) : BackgroundService, IBrowserDownloaderService
{
    private SemaphoreSlim _semaphore = new(1, 1);
    private bool _isInitialized;

    public async Task EnsureInitialized(CancellationToken stoppingToken)
    {
        if (_isInitialized)
        {
            return;
        }
        try
        {
            await _semaphore.WaitAsync(TimeSpan.FromSeconds(240), stoppingToken);
            if (_isInitialized is false)
            {
                throw new InvalidOperationException("BrowserDownloaderService is not initialized. Check error log for details");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested is false)
        {

            await _semaphore.WaitAsync(stoppingToken);
            try
            {
                Microsoft.Playwright.Program.Main(["install"]);
                _isInitialized = true;
                break; //nothing to do, exit the loop
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error downloading browser binaries for puppeteer: {Message}", ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); //wait and retry
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
