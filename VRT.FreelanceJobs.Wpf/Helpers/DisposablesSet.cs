namespace VRT.FreelanceJobs.Wpf.Helpers;

public sealed class DisposablesSet : HashSet<IAsyncDisposable>, IAsyncDisposable, IAsyncEnumerable<IAsyncDisposable>
{
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAll();
    }

    public async IAsyncEnumerator<IAsyncDisposable> GetAsyncEnumerator(CancellationToken cancellationToken)
    {
        await Task.Yield();
        foreach (var disposable in this)
        {
            if (disposable is IAsyncDisposable asyncDisposable)
            {
                yield return asyncDisposable;
            }
        }
    }
}