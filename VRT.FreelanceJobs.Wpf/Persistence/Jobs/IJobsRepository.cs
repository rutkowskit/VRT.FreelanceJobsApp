namespace VRT.FreelanceJobs.Wpf.Persistence.Jobs;

public interface IJobsRepository
{
    List<Job> Jobs { get; }
    bool IsDirty { get; }
    void Save();
}
