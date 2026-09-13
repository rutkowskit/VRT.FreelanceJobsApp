namespace VRT.FreelanceJobs.Wpf.Workers;

internal interface IBrowserDownloaderService
{
    /// <summary>
    /// Ensures that the browser binaries are downloaded and ready to use.
    /// When a CDP endpoint is configured this is a no-op.
    /// It will throw the exception if the initialization failed.
    /// </summary>
    /// <param name="stoppingToken">Operation cancellation token</param>
    /// <returns>Completed task</returns>
    Task EnsureInitialized(CancellationToken stoppingToken);
}
