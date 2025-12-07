using Bogus;
using OSFRS.Backend.DTOs.Analytics;
using OSFRS.Backend.DTOs.Auth;
using OSFRS.Backend.DTOs.Facilities;
using OSFRS.Backend.DTOs.Maintenance;
using OSFRS.Backend.DTOs.Reservations;
using OSFRS.Models.Entities;

namespace OSFRS.UnitTests.TestUtils;

public static class FakeData
{
    public static Faker<CreateReservationDto> CreateReservationDto() =>
        new Faker<CreateReservationDto>()
            .RuleFor(x => x.FacilityId, f => f.Random.Int(1, 50))
            .RuleFor(x => x.StartTime, f => DateTime.UtcNow.AddHours(1))
            .RuleFor(x => x.EndTime, f => DateTime.UtcNow.AddHours(2));

    public static Faker<UpdateReservationDto> UpdateReservationDto() =>
        new Faker<UpdateReservationDto>()
            .RuleFor(
                x => x.StartTime,
                f =>
                    f.Random.Bool() // 50% chance null
                        ? f.Date.FutureOffset(1).UtcDateTime // random future start time
                        : (DateTime?)null
            )
            .RuleFor(
                x => x.EndTime,
                f =>
                    f.Random.Bool()
                        ? f.Date.FutureOffset(2).UtcDateTime // random future end time
                        : (DateTime?)null
            )
            .RuleFor(
                x => x.Status,
                f =>
                    f.Random.Bool(0.30f) // 30% chance to include a status
                        ? f.PickRandom(new[] { "Pending", "Approved", "Cancelled" })
                        : null
            );

    public static Faker<Reservation> Reservation() =>
        new Faker<Reservation>()
            .RuleFor(r => r.Id, f => f.Random.Int())
            .RuleFor(r => r.FacilityId, f => f.Random.Int(1, 50))
            .RuleFor(r => r.StartTime, f => DateTime.UtcNow.AddHours(1))
            .RuleFor(r => r.EndTime, f => DateTime.UtcNow.AddHours(2));

    public static Faker<CreateFacilityDto> CreateFacilityDto() =>
        new Faker<CreateFacilityDto>()
            .RuleFor(f => f.Name, _ => "Sports Hall")
            .RuleFor(f => f.Type, _ => "Gym")
            .RuleFor(f => f.Capacity, _ => 20)
            .RuleFor(f => f.Status, _ => "Available");

    public static Faker<CreateMaintenanceRecordDto> CreateMaintenanceDto() =>
        new Faker<CreateMaintenanceRecordDto>()
            .RuleFor(x => x.FacilityId, f => f.Random.Int(1, 50))
            .RuleFor(x => x.Description, f => f.Lorem.Sentence())
            .RuleFor(x => x.StartTime, f => f.Date.FutureOffset().UtcDateTime)
            .RuleFor(x => x.EndTime, (f, x) => x.StartTime.AddHours(f.Random.Int(1, 5)))
            .RuleFor(x => x.Status, f => "Scheduled");

    public static Faker<UpdateMaintenanceRecordDto> UpdateMaintenanceDto() =>
        new Faker<UpdateMaintenanceRecordDto>()
            .RuleFor(x => x.Description, f => f.Random.Bool() ? f.Lorem.Sentence() : null)
            .RuleFor(
                x => x.StartTime,
                f => f.Random.Bool() ? f.Date.FutureOffset().UtcDateTime : null
            )
            .RuleFor(
                x => x.EndTime,
                f => f.Random.Bool() ? f.Date.FutureOffset().UtcDateTime : null
            )
            .RuleFor(
                x => x.Status,
                f => f.Random.Bool() ? f.PickRandom("Pending", "Scheduled", "InProgress") : null
            );

    public static Faker<MaintenanceRecord> MaintenanceRecord() =>
        new Faker<MaintenanceRecord>()
            .RuleFor(x => x.Id, f => f.Random.Int(1, 999))
            .RuleFor(x => x.FacilityId, f => f.Random.Int(1, 50))
            .RuleFor(x => x.Description, f => f.Lorem.Sentence())
            .RuleFor(x => x.StartTime, f => f.Date.FutureOffset(-2).UtcDateTime)
            .RuleFor(x => x.EndTime, (f, x) => x.StartTime.AddHours(f.Random.Int(1, 5)))
            .RuleFor(x => x.Status, f => "Scheduled");

    public static Faker<Facility> Facility() =>
        new Faker<Facility>()
            .RuleFor(x => x.Id, f => f.Random.Int(1, 50))
            .RuleFor(x => x.Name, f => f.Company.CompanyName())
            .RuleFor(x => x.Status, f => "Available")
            .RuleFor(x => x.UpdatedAt, f => DateTime.UtcNow);

    public static Faker<UpdateFacilityDto> UpdateFacilityDto() =>
        new Faker<UpdateFacilityDto>()
            .RuleFor(x => x.Name, f => f.Random.Bool() ? f.Company.CompanyName() : null)
            .RuleFor(
                x => x.Type,
                f => f.Random.Bool() ? f.PickRandom("Gym", "Pool", "Court") : null
            )
            .RuleFor(x => x.Capacity, f => f.Random.Bool() ? f.Random.Int(5, 200) : null)
            .RuleFor(
                x => x.Status,
                f => f.Random.Bool() ? f.PickRandom("Available", "Unavailable") : null
            );

    public static Faker<LoginRequestDto> LoginRequest() =>
        new Faker<LoginRequestDto>()
            .RuleFor(x => x.UsernameOrEmail, f => f.Internet.Email())
            .RuleFor(x => x.Password, f => f.Internet.Password());

    public static Faker<UserRegistrationDto> RegistrationDto() =>
        new Faker<UserRegistrationDto>()
            .RuleFor(x => x.Name, f => f.Person.FullName)
            .RuleFor(x => x.Username, f => f.Internet.UserName())
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.Password, f => f.Internet.Password());

    public static Faker<User> User() =>
        new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Int(1, 300))
            .RuleFor(u => u.Username, f => f.Internet.UserName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, _ => "hashed")
            .RuleFor(u => u.Role, _ => "User")
            .RuleFor(u => u.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(u => u.UpdatedAt, _ => DateTime.UtcNow);

    public static Faker<UpdatedProfileDto> UpdatedProfileDto() =>
        new Faker<UpdatedProfileDto>()
            .RuleFor(x => x.Name, f => f.Random.Bool() ? f.Person.FullName : null)
            .RuleFor(x => x.Username, f => f.Random.Bool() ? f.Internet.UserName() : null)
            .RuleFor(x => x.Email, f => f.Random.Bool() ? f.Internet.Email() : null);

    public static Faker<TrendPointDto> TrendPoint() =>
        new Faker<TrendPointDto>()
            .RuleFor(x => x.Timestamp, f => f.Date.PastOffset().UtcDateTime)
            .RuleFor(x => x.Count, f => f.Random.Int(0, 200));

    public static List<TrendPointDto> SimpleDaily(params int[] counts)
    {
        var day = DateTime.UtcNow.Date;

        return counts
            .Select(
                (c, i) =>
                    new TrendPointDto
                    {
                        Timestamp = day.AddDays(i), // timestamp
                        Count = c, // count
                    }
            )
            .ToList();
    }

    public record DailyCountDto(DateTime Timestamp, int Count);

    public static Faker<DailyCountDto> DailyCount() =>
        new Faker<DailyCountDto>().CustomInstantiator(f => new DailyCountDto(
            f.Date.PastOffset().UtcDateTime,
            f.Random.Int(0, 200)
        ));

    public static Faker<UsageRecord> UsageRecord() =>
        new Faker<UsageRecord>()
            .RuleFor(x => x.Id, f => f.Random.Int(1, 9999))
            .RuleFor(x => x.EventType, f => f.Random.Word())
            .RuleFor(x => x.Timestamp, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(x => x.UserId, f => f.Random.Int(1, 10))
            .RuleFor(x => x.FacilityId, f => f.Random.Int(1, 10))
            .RuleFor(x => x.AggregatedData, _ => "{}");

    public static List<UsageRecord> SimpleRecords(int count)
    {
        var f = new Faker();
        return Enumerable
            .Range(1, count)
            .Select(_ => new UsageRecord
            {
                Id = f.Random.Int(1, 9999),
                EventType = "TestEvent",
                Timestamp = DateTime.UtcNow.Date.AddDays(f.Random.Int(0, 10)),
                UserId = 1,
                FacilityId = 1,
                AggregatedData = null,
            })
            .ToList();
    }

    public static Faker<UsageEventDto> UsageEvent() =>
        new Faker<UsageEventDto>()
            .RuleFor(x => x.EventType, f => f.Random.Word())
            .RuleFor(x => x.UserId, f => f.Random.Int(1, 10))
            .RuleFor(x => x.FacilityId, f => f.Random.Int(1, 10))
            .RuleFor(x => x.Timestamp, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(x => x.Metadata, _ => new());

    public static Faker<UserRegistrationDto> UserRegistrationDto() =>
        new Faker<UserRegistrationDto>()
            .RuleFor(x => x.Name, f => f.Name.FirstName())
            .RuleFor(x => x.Username, f => "gencho_" + f.Random.Int(1, 9999))
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.Password, f => "StrongPassword123!");

    public static string WeakPassword() => "abc123";

    public static string InvalidEmail() => "not-an-email";

    public static string InvalidUsername() => "??bad name!!";
}
