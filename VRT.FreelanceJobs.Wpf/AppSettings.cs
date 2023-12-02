using System.Text.Json.Serialization;
using VRT.FreelanceJobs.Wpf.Options;

namespace VRT.FreelanceJobs.Wpf;

public sealed class AppSettings
{
    [JsonIgnore]
    public readonly static AppSettings Empty = new();
    public UsemeOptions? Useme { get; init; }
    public UpworkOptions? Upwork { get; init; }
}
