using VRT.FreelanceJobs.Wpf.Persistence.Jobs;

namespace VRT.FreelanceJobs.Wpf.Abstractions.Jobs
{
    /// <summary>
    /// Response data contract from Jobs external service
    /// </summary>
    public sealed class GetJobsResponse
    {
        /// <summary>
        /// Name of the external service
        /// </summary>
        required public string SourceName { get; init; }
        /// <summary>
        /// Original request
        /// </summary>
        required public GetJobsRequest Request { get; init; }

        /// <summary>
        /// Jobs list
        /// </summary>
        public IReadOnlyCollection<Job> Data { get; init; } = [];
    }
}