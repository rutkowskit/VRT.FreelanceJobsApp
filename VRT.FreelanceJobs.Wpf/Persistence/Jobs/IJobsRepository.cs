using Useme.Clients.Wpf.Persistence.Jobs;

namespace VRT.FreelanceJobs.Wpf.Persistence.Jobs;

public interface IJobsRepository
{
    List<Job> Jobs { get; }
    bool IsDirty { get; }
    void Save();
}
