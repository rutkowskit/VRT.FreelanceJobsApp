using CSharpFunctionalExtensions;
using Useme.Clients.Wpf.Persistence.Jobs;
using Useme.Clients.Wpf.Services.Useme;
using VRT.FreelanceJobs.Wpf.Abstractions.Jobs;
using VRT.FreelanceJobs.Wpf.Options;

namespace VRT.FreelanceJobs.Wpf.Services.Useme;

internal sealed class UsemeJobsServiceAdapter : IJobsService
{
    private readonly IUsemeJobsService _serivce;
    private readonly UsemeOptions _options;
    public UsemeJobsServiceAdapter(AppSettings appSettings, IUsemeJobsService? service = null)
    {
        _options = appSettings?.Useme ?? throw new ArgumentNullException(nameof(appSettings.Useme));
        _serivce = service ?? Refit.RestService.For<IUsemeJobsService>(_options.BaseUri);
    }

    public string SourceName => UsemeOptions.SourceName;

    public async Task<Result<GetJobsResponse>> GetJobs(GetJobsRequest request,
        CancellationToken cancellationToken = default)
    {

        var newJobs = new List<Job>();

        foreach (var category in _options.Categories)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            var minDueDate = request.DateFrom ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

            var jobs = await GetJobsForCategory(category, minDueDate, cancellationToken);
            newJobs.AddRange(jobs);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<GetJobsResponse>("Operation canceled");
        }
        var result = new GetJobsResponse()
        {
            SourceName = UsemeOptions.SourceName,
            Request = request,
            Data = newJobs.OrderByDescending(j => j.Id).ToArray()
        };
        return result;
    }

    private async Task<IReadOnlyCollection<Job>> GetJobsForCategory(
        string category,
        string minDueDate,
        CancellationToken cancellation)
    {
        var currentPage = 0;
        var resultJobs = new List<Job>();
        while (cancellation.IsCancellationRequested is false)
        {
            var page = currentPage == 0 ? null : currentPage.ToString();
            var result = await _serivce.GetJobEntries(category, page);
            var jobs = result.Content.ToUsemeJobs(_options.BaseUri).ToArray();
            if (jobs.Length == 0)
            {
                break;
            }
            resultJobs.AddRange(jobs);
            if (resultJobs.Any(j => j.OfferDueDate != null && j.OfferDueDate.CompareTo(minDueDate) < 0))
            {
                break;
            }
            currentPage++;
        }
        return cancellation.IsCancellationRequested ? [] : resultJobs;
    }
}
