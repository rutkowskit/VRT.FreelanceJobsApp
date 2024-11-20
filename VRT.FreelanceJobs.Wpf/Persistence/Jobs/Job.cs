using System.Text.Json.Serialization;

namespace VRT.FreelanceJobs.Wpf.Persistence.Jobs;

public sealed class Job : IEquatable<Job>
{
    public static Job AsExpired(Job job)
    {
        return new Job()
        {
            Id = job.Id,
            JobTitle = job.JobTitle,
            SourceName = job.SourceName,
            AddedTimestampMs = job.AddedTimestampMs,
            OfferDueDate = null
        };
    }

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

    public bool Equals(Job? other)
    {
        return other switch
        {
            null => false,
            _ => Id == other.Id && SourceName == other.SourceName
        };
    }
    public override bool Equals(object? obj) => Equals(obj as Job);
    public override int GetHashCode() => $"{Id}_{SourceName}".GetHashCode();
}
