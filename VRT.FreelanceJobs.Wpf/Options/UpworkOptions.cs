namespace VRT.FreelanceJobs.Wpf.Options;

public sealed class UpworkOptions
{
    public const string SourceName = "Upwork";
    required public string BaseUri { get; init; }
    public string[] Categories { get; init; } = [];
}
