namespace VRT.FreelanceJobs.Wpf.Options;

public sealed class UsemeOptions
{
    public const string SourceName = "Useme";
    required public string BaseUri { get; init; }
    public string[] Categories { get; init; } = [];
}
