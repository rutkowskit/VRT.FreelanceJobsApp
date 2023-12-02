using System.IO;
using System.Text;
using System.Text.Json;
using Useme.Clients.Wpf.Persistence.Jobs;
using VRT.FreelanceJobs.Wpf.Helpers;

namespace VRT.FreelanceJobs.Wpf.Persistence.Jobs
{
    internal class JsonFileRepository : IJobsRepository
    {
        private List<Job>? _jobs;
        public List<Job> Jobs => _jobs ??= Load<Job>();

        public bool IsDirty => Jobs.Any(j => j.IsDirty);

        public void Save()
        {
            Jobs.Where(j => j.IsDirty).ToList().ForEach(j => j.IsDirty = false);
            File.WriteAllText(GetFilePath<Job>(), JsonSerializer.Serialize(Jobs));
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
                var json = File.ReadAllText(fileName, Encoding.UTF8);
                return JsonSerializer.Deserialize<List<T>>(json) ?? [];
            }
            catch
            {
                return [];
            }
        }
        private static string GetFilePath<T>()
        {
            var dbDir = Path.Combine(DirectoryHelpers.GetExecutingAssemblyDirectory(), "Db");
            Directory.CreateDirectory(dbDir);
            return Path.Combine(dbDir, $"{typeof(T).Name}.json");
        }

    }
}
