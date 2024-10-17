using System.IO;
using System.Text;
using System.Text.Json;
using Useme.Clients.Wpf.Persistence.Jobs;
using VRT.FreelanceJobs.Wpf.Helpers;

namespace VRT.FreelanceJobs.Wpf.Persistence.Jobs;

internal sealed class JsonFileRepository : IJobsRepository
{
    private List<Job>? _jobs;
    public List<Job> Jobs => _jobs ??= Load<Job>();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public bool IsDirty => Jobs.Any(j => j.IsDirty);

    public void Save()
    {
        _semaphore.Wait();
        var toSave = Jobs.Where(j => j.IsDirty).ToList();
        try
        {
            if (toSave.Count == 0)
            {
                return;
            }
            toSave.ForEach(j => j.IsDirty = false);
            File.WriteAllText(GetFilePath<Job>(), JsonSerializer.Serialize(Jobs));
        }
        catch (Exception)
        {
            // rollback
            toSave.ForEach(j => j.IsDirty = true);
        }
        finally
        {
            _semaphore.Release();
        }

    }
    private List<T> Load<T>()
    {
        var fileName = GetFilePath<T>();
        if (File.Exists(fileName) is false)
        {
            return [];
        }
        try
        {
            _semaphore.Wait();
            var json = File.ReadAllText(fileName, Encoding.UTF8);
            return JsonSerializer.Deserialize<List<T>>(json) ?? [];
        }
        catch
        {
            return [];
        }
        finally
        {
            _semaphore.Release();
        }
    }
    private static string GetFilePath<T>()
    {
        var dbDir = Path.Combine(DirectoryHelpers.GetExecutingAssemblyDirectory(), "Db");
        Directory.CreateDirectory(dbDir);
        return Path.Combine(dbDir, $"{typeof(T).Name}.json");
    }
}
