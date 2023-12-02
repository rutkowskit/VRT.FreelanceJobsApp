namespace VRT.FreelanceJobs.Wpf.Abstractions.Jobs
{

    /// <summary>
    /// Request to get Jobs list from external service
    /// </summary>
    /// <param name="DateFrom">Starting date of </param>
    /// <param name="PageNumber">Page number of the list of jobs</param>
    public sealed record GetJobsRequest(string? DateFrom);
}
