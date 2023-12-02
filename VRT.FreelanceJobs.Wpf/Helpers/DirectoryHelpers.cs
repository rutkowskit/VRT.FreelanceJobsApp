using System.IO;
using System.Reflection;

namespace VRT.FreelanceJobs.Wpf.Helpers;
public static class DirectoryHelpers
{
    public static string GetExecutingAssemblyDirectory()
    {
        var codeBase = Assembly.GetExecutingAssembly().Location;
        var uri = new UriBuilder(codeBase);
        var path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory();
    }
}