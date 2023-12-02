using CSharpFunctionalExtensions;

namespace VRT.FreelanceJobs.Wpf.Abstractions.Jobs;

/// <summary>
/// A contract to get jobs list from external service
/// </summary>
public interface IJobsService
{
    string SourceName { get; }
    Task<Result<GetJobsResponse>> GetJobs(GetJobsRequest request, CancellationToken cancellationToken = default);
}
