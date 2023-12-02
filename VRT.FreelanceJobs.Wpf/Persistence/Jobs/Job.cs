using System.Text.Json.Serialization;

namespace Useme.Clients.Wpf.Persistence.Jobs;

public sealed class Job
{
    required public string Id { get; init; }
    required public string SourceName { get; init; }
    required public string JobTitle { get; set; }
    public string? OffersCount { get; set; }
    public string? ContentShort { get; set; }
    public string? ContentFull { get; set; }
    public string? OfferDueDate { get; set; }
    public string? FullOfferDetailsUrl { get; set; }
    public string? Category { get; set; }
    public string? Budget { get; set; }
    public string[] Skills { get; set; } = [];
    public bool Hidden { get; set; }
    public bool IsNew { get; set; }
    public long AddedTimestampMs { get; set; }
    [JsonIgnore]
    public bool IsDirty { get; set; }

}
