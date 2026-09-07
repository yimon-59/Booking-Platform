using Rezerv.Application.Interfaces;

namespace Rezerv.Api.BackgroundJobs;

public class WaitlistExpirationJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WaitlistExpirationJob> _logger;

    public WaitlistExpirationJob(
        IServiceScopeFactory scopeFactory,
        ILogger<WaitlistExpirationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var waitlistService =
                scope.ServiceProvider
                    .GetRequiredService<IWaitListService>();

            await waitlistService.ExpireWaitlistEntriesAsync();

            _logger.LogInformation(
                "Waitlist expiration job completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while expiring waitlist entries.");

            throw;
        }
    }
}