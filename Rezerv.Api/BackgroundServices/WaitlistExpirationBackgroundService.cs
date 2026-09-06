using Rezerv.Application.Interfaces;
namespace Rezerv.Api.BackgroundServices;

public class WaitlistExpirationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WaitlistExpirationBackgroundService> _logger;

    public WaitlistExpirationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<WaitlistExpirationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Waitlist expiration background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var waitlistService = scope.ServiceProvider.GetRequiredService<IWaitListService>();

                await waitlistService.ExpireWaitlistEntriesAsync(stoppingToken);

                _logger.LogInformation("Waitlist expiration check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error occurred while expiring waitlist entries.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(1),
                stoppingToken);
        }
    }
}