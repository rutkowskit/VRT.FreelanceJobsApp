using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpFunctionalExtensions;
using VRT.FreelanceJobs.Wpf.Abstractions.Jobs;
using VRT.FreelanceJobs.Wpf.Helpers;
using VRT.FreelanceJobs.Wpf.Mvvm;
using VRT.FreelanceJobs.Wpf.Persistence.Jobs;

namespace VRT.FreelanceJobs.Wpf;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly IJobsService[] _jobServices;
    private readonly IJobsRepository _jobsRepository;

    public MainWindowViewModel(
        IEnumerable<IJobsService> jobServices,
        IJobsRepository jobsRepository)
    {
        _jobServices = jobServices?.ToArray() ?? [];
        _jobsRepository = jobsRepository;
    }
    private IReadOnlyCollection<Job> _allJobs = [];

    [ObservableProperty]
    private bool _showHidden;

    [ObservableProperty]
    private string? _filterText;

    [ObservableProperty]
    private bool _jobsUpdating;

    [ObservableProperty]
    private bool _jobsDownloading;

    [ObservableProperty]
    private IReadOnlyCollection<Job> _jobs = [];

    [ObservableProperty]
    private int? _newJobsCount;

    [ObservableProperty]
    private int? _updatedJobsCount;

    [RelayCommand]
    private Task LoadCachedJobs()
    {
        _jobsRepository.MaintainJobs();
        _allJobs = _jobsRepository.Jobs;
        ApplyFilters();
        return Task.CompletedTask;
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task GetJobsFromServices(CancellationToken cancellationToken)
    {
        var totalJobsAdded = 0;
        JobsDownloading = true;
        NewJobsCount = null;
        foreach (var service in _jobServices)
        {
            var newestJob = _jobsRepository.GetNewestJob(service.SourceName);
            var fromDate = newestJob?.OfferDueDate ?? DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            var request = new GetJobsRequest(fromDate);
            await service
                .GetJobs(request, cancellationToken)
                .Map(jobs => _jobsRepository.AddMissingJobs(jobs.Data, service.SourceName))
                .Tap(addedJobsCnt => totalJobsAdded += addedJobsCnt)
                .ConfigureAwait(true);
        }
        var hasChanges = totalJobsAdded > 0 || Jobs.Any(j => j.IsDirty);
        NewJobsCount = totalJobsAdded > 0 ? totalJobsAdded : null;
        if (hasChanges && cancellationToken.IsCancellationRequested is false)
        {
            _jobsRepository.Save();
            LoadCachedJobsCommand.Execute(cancellationToken);
        }
        JobsDownloading = false;
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task UpdateAllJobs(CancellationToken cancellationToken)
    {
        var totalJobsUpdated = 0;
        JobsUpdating = true;
        UpdatedJobsCount = null;
        var fromDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
        foreach (var service in _jobServices)
        {
            var request = new GetJobsRequest(fromDate);
            await service
                .GetJobs(request, cancellationToken)
                .Map(jobs => _jobsRepository.UpdateExistingJobs(jobs.Data))
                .Tap(changesCount => totalJobsUpdated += changesCount)
                .ConfigureAwait(true);
        }
        JobsUpdating = false;
        var hasChanges = totalJobsUpdated > 0;
        UpdatedJobsCount = totalJobsUpdated == 0 ? null : totalJobsUpdated;
        if (hasChanges && cancellationToken.IsCancellationRequested is false)
        {
            _jobsRepository.Save();
            LoadCachedJobsCommand.Execute(cancellationToken);
        }
    }
    [RelayCommand]
    private void ApplyFilters()
    {
        var jobsQuery = ApplyFilters(_allJobs)
            .OrderByDescending(s => s.AddedTimestampMs)
            .ThenByDescending(s => s.Id);
        Jobs = [.. jobsQuery];
    }
    [RelayCommand]
    private void SaveAndApplyFilters()
    {
        _jobsRepository.Save();
        ApplyFilters();
    }
    private Job[] ApplyFilters(IEnumerable<Job> jobs)
    {
        var showHidden = ShowHidden;
        var jobsQuery = jobs
            .Where(j => j.Hidden == showHidden);
        return FilterJobs(jobsQuery);

    }
    private Job[] FilterJobs(IEnumerable<Job> jobs)
    {
        var filterParts = FilterText?.Split() ?? [];
        var filtered = string.IsNullOrWhiteSpace(FilterText)
            ? jobs?.ToArray()
            : jobs?.Where(p => GetFilterValues(p).MatchesAll(filterParts)).ToArray();

        return filtered ?? [];
    }

    partial void OnShowHiddenChanged(bool oldValue, bool newValue) => ApplyFilters();
    partial void OnFilterTextChanged(string? value) => ApplyFilters();

    private static string?[] GetFilterValues(Job job)
        => [job.Category, job.JobTitle, job.JobTitle, job.ContentShort, job.SourceName, .. job.Skills];
}
