using Refit;

namespace Useme.Clients.Wpf.Services.Useme;

internal interface IUsemeJobsService
{
    [Get("/pl/jobs/category/{category}/?page={page}")]
    Task<ApiResponse<string>> GetJobEntries(string category, string? page = null);
}
