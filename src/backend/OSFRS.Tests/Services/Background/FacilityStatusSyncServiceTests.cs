// using Moq;
// using OSFRS.Backend.Interfaces;
// using OSFRS.Backend.Interfaces.Logging;
// using OSFRS.Backend.Services.Background;

// public class FacilityStatusSyncServiceTests
// {
//     [Fact]
//     public async Task ExecuteAsync_ShouldCallSync_once_WhenCancelledImmediately()
//     {
//         var mockMaintenance = new Mock<IMaintenanceService>();
//         var mockLogger = new Mock<IAppLogger<FacilityStatusSyncService>>();

//         var service = new TestableFacilityStatusSyncService(
//             mockMaintenance.Object,
//             mockLogger.Object,
//             TimeSpan.FromMilliseconds(10) 
//         );

//         using var cts = new CancellationTokenSource();

//         cts.CancelAfter(50);

//         await service.StartAsync(cts.Token);

//         mockMaintenance.Verify(m => m.SyncFacilityStatusesAsync(), Times.Once);
//     }

//     private class TestableFacilityStatusSyncService : FacilityStatusSyncService
//     {
//         private readonly TimeSpan _testInterval;
//         private readonly IMaintenanceService _maintenanceService;

//         public TestableFacilityStatusSyncService(
//             IMaintenanceService service,
//             IAppLogger<FacilityStatusSyncService> logger,
//             TimeSpan interval
//         ) : base(service, logger)
//         {
//             _maintenanceService = service;
//             _testInterval = interval;
//         }

//         protected override async Task ExecuteAsync(CancellationToken token)
//         {
//             try
//             {
//                 await _maintenanceService.SyncFacilityStatusesAsync();
//             }
//             catch { }

//             await Task.Delay(_testInterval, token);
//         }
//     }
// }