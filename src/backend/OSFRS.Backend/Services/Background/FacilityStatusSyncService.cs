using OSFRS.Backend.Interfaces;
using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.Backend.Services.Background;

public class FacilityStatusSyncService : BackgroundService
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IAppLogger<FacilityStatusSyncService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public FacilityStatusSyncService(IMaintenanceService maintenanceService, IAppLogger<FacilityStatusSyncService> logger)
    {
        _maintenanceService = maintenanceService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        _logger.LogInformation("Facility Status Sync Service started.");

        while (!token.IsCancellationRequested)
        {
            try
            {
                await _maintenanceService.SyncFacilityStatusesAsync();
                _logger.LogInformation("Facility status sync cycle completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during facility status sync cycle.");
            }

            await Task.Delay(_interval, token);
        }

        _logger.LogInformation("Facility Status Sync Service stopped.");
    }
}