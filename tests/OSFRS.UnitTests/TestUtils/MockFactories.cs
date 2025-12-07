using Moq;
using OSFRS.Backend.Interfaces.Helper;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Interfaces.Validator;

namespace OSFRS.UnitTests.TestUtils;

public static class MockFactories
{
    public static Mock<IReservationRepository> ReservationRepo() => new();

    public static Mock<IFacilityRepository> FacilityRepo() => new();

    public static Mock<IMaintenanceRepository> MaintenanceRepo() => new();

    public static Mock<IUserRepository> UserRepo() => new();

    public static Mock<IAnalyticsRepository> AnalyticsRepo() => new();

    public static Mock<IUsageRepository> UsageRepo() => new();

    public static Mock<IReportRepository> ReportRepo() => new();

    public static Mock<IUsageService> UsageService() => new();

    public static Mock<IValidator<T>> Validator<T>() => new();

    public static Mock<IUpdateValidator<T, TEntity>> UpdateValidator<T, TEntity>() => new();

    public static Mock<IJwtTokenGenerator> Jwt() => new();

    public static Mock<IPasswordHasher> Hasher() => new();

    public static Mock<IAppLogger<T>> Logger<T>()
        where T : class => new();
}
