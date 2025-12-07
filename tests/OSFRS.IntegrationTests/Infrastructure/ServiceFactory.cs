using OSFRS.Backend.Data;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Backend.Helpers;
using OSFRS.Backend.Interfaces.Helper;
using OSFRS.Backend.Interfaces.Logging;
using OSFRS.Backend.Interfaces.Repository;
using OSFRS.Backend.Interfaces.Service;
using OSFRS.Backend.Interfaces.Validator;
using OSFRS.Backend.Repositories;
using OSFRS.Backend.Services;
using OSFRS.Backend.Validators.Auth;
using OSFRS.Backend.Validators.Facilities;
using OSFRS.Backend.Validators.Maintenance;
using OSFRS.Backend.Validators.Reservations;
using OSFRS.Backend.Validators.Usage;
using OSFRS.IntegrationTests.Infrastructure.Logging;
using OSFRS.Models.Entities;

namespace OSFRS.IntegrationTests.Infrastructure;

public class ServiceFactory
{
    private readonly OSFRSDbContext _db;

    public ServiceFactory(OSFRSDbContext db)
    {
        _db = db;
    }

    private IAppLogger<T> Logger<T>()
        where T : class => new TestLogger<T>();

    // -------------------------------------------------------------
    // HELPERS (cached)
    // -------------------------------------------------------------
    private IPasswordHasher? _hasher;

    public IPasswordHasher PasswordHasher() => _hasher ??= new PasswordHasher();

    private IJwtTokenGenerator? _jwt;

    public IJwtTokenGenerator Jwt() => _jwt ??= new JwtTokenGenerator();

    // -------------------------------------------------------------
    // REPOSITORIES (cached)
    // -------------------------------------------------------------
    private IUserRepository? _users;

    public IUserRepository UserRepo() =>
        _users ??= new UserRepository(_db, Logger<BaseRepository<User>>());

    private IReservationRepository? _res;

    public IReservationRepository ReservationRepo() =>
        _res ??= new ReservationRepository(_db, Logger<BaseRepository<Reservation>>());

    private IFacilityRepository? _fac;

    public IFacilityRepository FacilityRepo() =>
        _fac ??= new FacilityRepository(_db, Logger<BaseRepository<Facility>>());

    private IMaintenanceRepository? _maint;

    public IMaintenanceRepository MaintenanceRepo() =>
        _maint ??= new MaintenanceRepository(_db, Logger<BaseRepository<MaintenanceRecord>>());

    private IUsageRepository? _usage;

    public IUsageRepository UsageRepo() =>
        _usage ??= new UsageRepository(_db, Logger<BaseRepository<UsageRecord>>());

    private IReportRepository? _report;

    public IReportRepository ReportRepo() =>
        _report ??= new ReportRepository(_db, Logger<ReportRepository>());

    private IAnalyticsRepository? _analytics;

    public IAnalyticsRepository AnalyticsRepo() =>
        _analytics ??= new AnalyticsRepository(_db, Logger<AnalyticsRepository>());

    // -------------------------------------------------------------
    // VALIDATORS (cached)
    // -------------------------------------------------------------
    private IValidator<LoginRequestDto>? _loginValidator;

    public IValidator<LoginRequestDto> LoginValidator() =>
        _loginValidator ??= new UserLoginValidator();

    private IValidator<UserRegistrationDto>? _registrationValidator;

    public IValidator<UserRegistrationDto> RegistrationValidator() =>
        _registrationValidator ??= new UserRegistrationValidator(UserRepo());

    private IValidator<CreateFacilityDto>? _createFacilityValidator;

    public IValidator<CreateFacilityDto> CreateFacilityValidator() =>
        _createFacilityValidator ??= new CreateFacilityValidator();

    private IValidator<(CreateReservationDto dto, int userId)>? _createReservationValidator;

    public IValidator<(CreateReservationDto dto, int userId)> CreateReservationValidator() =>
        _createReservationValidator ??= new CreateReservationValidator(
            FacilityRepo(),
            ReservationRepo(),
            MaintenanceRepo()
        );

    private IValidator<(
        UpdateReservationDto dto,
        Reservation existing,
        bool isAdmin,
        int userId
    )>? _updateReservationValidator;

    public IValidator<(
        UpdateReservationDto dto,
        Reservation existing,
        bool isAdmin,
        int userId
    )> UpdateReservationValidator() =>
        _updateReservationValidator ??= new UpdateReservationValidator(
            MaintenanceRepo(),
            ReservationRepo()
        );

    private IValidator<(Reservation reservation, int userId)>? _cancelReservationValidator;

    public IValidator<(Reservation reservation, int userId)> CancelReservationValidator() =>
        _cancelReservationValidator ??= new CancelReservationValidator();

    private IValidator<(Facility facility, bool isAvailable)>? _facilityAvailabilityValidator;

    public IValidator<(Facility facility, bool isAvailable)> FacilityAvailabilityValidator() =>
        _facilityAvailabilityValidator ??= new FacilityAvailabilityValidator(MaintenanceRepo());

    private IValidator<(
        string? eventType,
        int? userId,
        int? facilityId,
        DateTime? from,
        DateTime? to
    )>? _usageQueryValidator;

    public IValidator<(
        string? eventType,
        int? userId,
        int? facilityId,
        DateTime? from,
        DateTime? to
    )> UsageQueryValidator() => _usageQueryValidator ??= new UsageQueryValidator();

    private IValidator<CreateMaintenanceRecordDto>? _createMaintenanceValidator;

    public IValidator<CreateMaintenanceRecordDto> CreateMaintenanceValidator() =>
        _createMaintenanceValidator ??= new CreateMaintenanceValidator(
            FacilityRepo(),
            MaintenanceRepo()
        );

    private IUpdateValidator<UpdatedProfileDto, User>? _profileUpdateValidator;

    public IUpdateValidator<UpdatedProfileDto, User> ProfileUpdateValidator() =>
        _profileUpdateValidator ??= new ProfileUpdateValidator(UserRepo());

    private IUpdateValidator<UpdateFacilityDto, Facility>? _updateFacilityValidator;

    public IUpdateValidator<UpdateFacilityDto, Facility> UpdateFacilityValidator() =>
        _updateFacilityValidator ??= new UpdateFacilityValidator(MaintenanceRepo());

    private IUpdateValidator<
        UpdateMaintenanceRecordDto,
        MaintenanceRecord
    >? _updateMaintenanceValidator;

    public IUpdateValidator<
        UpdateMaintenanceRecordDto,
        MaintenanceRecord
    > UpdateMaintenanceValidator() =>
        _updateMaintenanceValidator ??= new UpdateMaintenanceValidator(MaintenanceRepo());

    // -------------------------------------------------------------
    // SERVICES (cached)
    // -------------------------------------------------------------
    private IAuthService? _authService;

    public IAuthService AuthService() =>
        _authService ??= new AuthService(
            UserRepo(),
            PasswordHasher(),
            Jwt(),
            Logger<AuthService>(),
            LoginValidator(),
            RegistrationValidator()
        );

    private IProfileService? _profileService;

    public IProfileService ProfileService() =>
        _profileService ??= new ProfileService(
            UserRepo(),
            PasswordHasher(),
            Logger<ProfileService>(),
            ProfileUpdateValidator()
        );

    private IReservationService? _reservationService;

    public IReservationService ReservationService() =>
        _reservationService ??= new ReservationService(
            ReservationRepo(),
            FacilityRepo(),
            Logger<ReservationService>(),
            CreateReservationValidator(),
            UpdateReservationValidator(),
            CancelReservationValidator()
        );

    private IFacilityService? _facilityService;

    public IFacilityService FacilityService() =>
        _facilityService ??= new FacilityService(
            FacilityRepo(),
            Logger<FacilityService>(),
            CreateFacilityValidator(),
            UpdateFacilityValidator(),
            FacilityAvailabilityValidator()
        );

    private IMaintenanceService? _maintenanceService;

    public IMaintenanceService MaintenanceService() =>
        _maintenanceService ??= new MaintenanceService(
            MaintenanceRepo(),
            FacilityRepo(),
            Logger<MaintenanceService>(),
            CreateMaintenanceValidator(),
            UpdateMaintenanceValidator()
        );

    private IUsageService? _usageService;

    public IUsageService UsageService() =>
        _usageService ??= new UsageService(
            UsageRepo(),
            Logger<UsageService>(),
            UsageQueryValidator()
        );

    private IReportService? _reportService;

    public IReportService ReportService() =>
        _reportService ??= new ReportService(ReportRepo(), Logger<ReportService>());

    private IAnalyticsService? _analyticsService;

    public IAnalyticsService AnalyticsService() =>
        _analyticsService ??= new AnalyticsService(AnalyticsRepo(), Logger<AnalyticsService>());
}
