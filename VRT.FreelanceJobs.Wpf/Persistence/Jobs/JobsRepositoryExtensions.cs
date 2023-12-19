using System.Text.RegularExpressions;
using Useme.Clients.Wpf.Persistence.Jobs;

namespace VRT.FreelanceJobs.Wpf.Persistence.Jobs;

internal static class JobsRepositoryExtensions
{
    public static int UpdateExistingJobs(this IJobsRepository repository,
        IReadOnlyCollection<Job> newJobsData)
    {
        if (newJobsData.Count == 0)
        {
            return 0;
        }
        var minJobId = newJobsData.Min(j => j.Id);
        var query = from dbJob in repository.Jobs
                    where dbJob.Id.CompareTo(minJobId) >= 0
                    join newJob in newJobsData on (dbJob.Id, dbJob.SourceName) equals (newJob.Id, newJob.SourceName) into gnj
                    from newJob in gnj.DefaultIfEmpty(Job.AsExpired(dbJob))
                    select (Current: dbJob, NewJob: newJob);

        var updatesCount = 0;
        query.ToList().ForEach(j =>
        {
            if (UpdateJob(j.Current, j.NewJob))
            {
                updatesCount++;
            }
        });
        return updatesCount;

    }
    private static bool UpdateJob(Job current, Job newJob)
    {
        var result = current.Update(j => j.JobTitle, newJob.JobTitle)
            | current.Update(j => j.OffersCount, newJob.OffersCount)
            | current.Update(j => j.ContentShort, newJob.ContentShort)
            | current.Update(j => j.ContentFull, newJob.ContentFull)
            | current.Update(j => j.OfferDueDate, newJob.OfferDueDate)
            | current.Update(j => j.FullOfferDetailsUrl, newJob.FullOfferDetailsUrl)
            | current.Update(j => j.Category, newJob.Category)
            | current.Update(j => j.Budget, newJob.Budget)
            | current.Update(j => j.Skills, newJob.Skills);
        return result;
    }
    public static int AddMissingJobs(this IJobsRepository repository,
        IEnumerable<Job> newJobs,
        string sourceName)
    {
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var notNewEntryTimestamp = currentTimestamp - TimeSpan.FromDays(1).TotalMilliseconds;

        var sourceJobs = repository.Jobs
            .Where(j => j.SourceName == sourceName)
            .ToArray();

        var existingIds = sourceJobs
            .Select(s => s.Id)
            .ToHashSet();

        var toAdd = newJobs.Where(j => existingIds.Contains(j.Id) is false).ToList();
        if (toAdd.Count == 0)
        {
            return 0;
        }
        toAdd.ForEach(j =>
        {
            j.IsNew = true;
            j.AddedTimestampMs = currentTimestamp;
            repository.Jobs.Add(j);
        });
        return toAdd.Count;
    }
    public static Job? GetNewestJob(this IJobsRepository repository,
        string sourceName)
    {
        var newestEntry = repository.Jobs
            .Where(j => j.SourceName == sourceName)
            .Where(s => s.OfferDueDate != null && Regex.IsMatch(s.OfferDueDate, @"\d{4}-\d{2}-\d{2}", RegexOptions.NonBacktracking))
            .OrderByDescending(s => s.Id)
            .FirstOrDefault();
        return newestEntry;
    }
    public static void MaintainJobs(this IJobsRepository repository)
    {
        var changesCount = repository.RemoveExpiredJobs();
        changesCount += repository.UncheckIsNewForOldJobs();
        changesCount += repository.RemoveDuplicates();

        if (changesCount > 0)
        {
            repository.Save();
        }
    }
    public static int UncheckIsNewForOldJobs(this IJobsRepository repository)
    {
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var notNewEntryTimestamp = currentTimestamp - TimeSpan.FromDays(1).TotalMilliseconds;

        var toUncheck = repository.Jobs
            .Where(j => j.AddedTimestampMs <= notNewEntryTimestamp)
            .Where(j => j.IsNew)
            .ToList();
        toUncheck
            .ForEach(j => j.IsNew = false);
        return toUncheck.Count;
    }
    public static int RemoveExpiredJobs(this IJobsRepository repository)
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");

        var toRemove = repository.Jobs
            .Where(j => j.OfferDueDate == null || j.OfferDueDate.CompareTo(today) < 0)
            .ToList();
        toRemove.ForEach(j => repository.Jobs.Remove(j));
        return toRemove.Count;
    }

    public static int RemoveDuplicates(this IJobsRepository repository)
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");

        var toRemove = repository.Jobs
            .GroupBy(j => new { j.Id, j.SourceName })
            .SelectMany(j => j.Skip(1))
            .ToList();
        toRemove.ForEach(j => repository.Jobs.Remove(j));
        return toRemove.Count;
    }
}
