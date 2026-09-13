namespace VRT.FreelanceJobs.Wpf.Options;

public sealed class PlaywrightOptions
{
    /// <summary>
    /// HTTP or WebSocket CDP endpoint of a running Chromium-based browser
    /// (for example http://127.0.0.1:9222). When set, Playwright attaches to that
    /// browser and does not install or launch a new instance.
    /// </summary>
    public string? CdpEndpoint { get; init; }

    public bool HasCdpEndpoint => string.IsNullOrWhiteSpace(CdpEndpoint) is false;
}
