using CSharpFunctionalExtensions;
using Useme.Clients.Wpf.Persistence.Jobs;
using Useme.Clients.Wpf.Services.Upwork;
using VRT.FreelanceJobs.Wpf.Abstractions.Jobs;
using VRT.FreelanceJobs.Wpf.Options;

namespace VRT.FreelanceJobs.Wpf.Services.Upwork;

internal sealed class UpworkJobsServiceAdapter : IJobsService
{
    private readonly IUpworkJobsService _serivce;
    private readonly UpworkOptions _options;

    public UpworkJobsServiceAdapter(AppSettings appSettings, IUpworkJobsService? service = null)
    {
        _options = appSettings?.Upwork ?? throw new NullReferenceException("Upwork options are required");
        _serivce = service ?? Refit.RestService.For<IUpworkJobsService>(_options.BaseUri);
    }

    public string SourceName => UpworkOptions.SourceName;

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
            SourceName = UpworkOptions.SourceName,
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
        var currentPage = 1;
        var resultJobs = new List<Job>();
        while (cancellation.IsCancellationRequested is false)
        {
            var page = currentPage == 0 ? null : currentPage.ToString();

            var doc = new HtmlAgilityPack.HtmlWeb();
            doc.UseCookies = true;
            doc.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";

            var url = $"{_options.BaseUri}/nx/search/jobs?page&per_page=50&q=.net&sort=recency";
            var url2 = $"{_options.BaseUri}/search/jobs/url?page&per_page=50&q=.net&sort=recency";
            var webResponse = doc.Load(url);
            var webResponse2 = doc.Load(url2);


            var result = await _serivce.GetJobEntries(category, page);
            var jobs = result.Content.ToUpworkJobs().ToArray();
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
