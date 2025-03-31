using Refit;

namespace VRT.FreelanceJobs.Wpf.Services.Useme;

[Headers(
    "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36",
    "Referer: https://www.useme.com/")]
internal interface IUsemeJobsService
{
    [Get("/pl/jobs/category/{category}/?page={page}")]
    Task<ApiResponse<string>> GetJobEntries(string category, string? page = null);
}
