using Rezerv.Application.Interfaces;

namespace Rezerv.Tests;

public class FakeDistributedLockService : IDistributedLockService
{
    public Task<IAsyncDisposable?> AcquireAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IAsyncDisposable?>(
            new FakeDistributedLock());
    }

    private sealed class FakeDistributedLock : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}