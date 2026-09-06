namespace Rezerv.Application.Interfaces;

public interface IDistributedLockService
{
    Task<IAsyncDisposable?> AcquireAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);
}