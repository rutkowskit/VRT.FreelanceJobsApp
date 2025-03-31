using Refit;

namespace VRT.FreelanceJobs.Wpf.Services.Upwork;

[Headers(
    "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36",
    "Referer: https://www.upwork.com")]
internal interface IUpworkJobsService
{
    [Get("/nx/search/jobs?page={page}&per_page=50&q={category}&sort=recency")]
    Task<ApiResponse<string>> GetJobEntries(string category, string? page = "1");

    [Get("/search/jobs/url?page={page}&per_page=50&q={category}&sort=recency")]
    [Headers("Accept: application/json, text/plain, */*")]
    Task<ApiResponse<string>> GetJobEntries2(string category, string? page = "1");
}
