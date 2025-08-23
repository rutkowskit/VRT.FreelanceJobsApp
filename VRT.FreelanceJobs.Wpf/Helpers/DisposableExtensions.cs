using System.Diagnostics.Contracts;

namespace VRT.FreelanceJobs.Wpf.Helpers;
internal static class DisposableExtensions
{
    [Pure]
    public static void DisposeAll<T>(this IEnumerable<T> disposables) where T : IDisposable
    {
        foreach (var disposable in disposables)
        {
            disposable?.Dispose();
        }
    }

    [Pure]
    public static async Task DisposeAll<T>(this IAsyncEnumerable<T> disposables) where T : IAsyncDisposable
    {
        await foreach (var disposable in disposables)
        {
            await disposable.DisposeAsync();
        }
    }

    public static T AsyncDisposeWith<T>(this T obj, ICollection<IAsyncDisposable> disposables)
        where T : notnull, IAsyncDisposable
    {
        disposables.Add(obj);
        return obj;
    }

    public static T DisposeWith<T>(this T obj, ICollection<IAsyncDisposable> disposables)
        where T : notnull, IDisposable
    {
        disposables.Add(DisposableAsyncWrapper.From(obj));
        return obj;
    }
}

file sealed class DisposableAsyncWrapper : IAsyncDisposable
{
    private readonly IDisposable _disposable;

    private DisposableAsyncWrapper(IDisposable disposable)
    {
        _disposable = disposable;
    }

    public ValueTask DisposeAsync()
    {
        if (_disposable is IAsyncDisposable a)
        {
            return a.DisposeAsync();
        }
        _disposable.Dispose();
        return ValueTask.CompletedTask;
    }
    public static DisposableAsyncWrapper From(IDisposable disposable) => new(disposable);
}
