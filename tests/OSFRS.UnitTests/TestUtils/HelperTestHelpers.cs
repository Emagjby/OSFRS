using System.IdentityModel.Tokens.Jwt;
using OSFRS.Backend.DTOs.Reports;
using OSFRS.Models.Entities;

namespace OSFRS.UnitTests.TestUtils;

public static class HelperTestHelpers
{
    public static User FakeUser =>
        new()
        {
            Id = 42,
            Username = "gencho",
            Email = "gencho@test.com",
            Role = "Admin",
        };

    public static JwtSecurityToken Decode(string token) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token);

    public static DateTime TrimToSeconds(DateTime dt) =>
        new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);

    public static ReportResultDto SampleReport()
    {
        var ts = DateTime.UtcNow;

        return new ReportResultDto
        {
            GeneratedAtUtc = ts,
            Daily =
            {
                new ReportEntryDto
                {
                    EventType = "ReservationCreated",
                    Timestamp = ts,
                    Metadata = "meta1",
                },
            },
            Monthly =
            {
                new ReportEntryDto
                {
                    EventType = "FacilityUpdated",
                    Timestamp = ts,
                    Metadata = "meta2",
                },
            },
        };
    }

    public static UsageRecord Rec(string evt, string meta = "meta", DateTime? ts = null) =>
        new()
        {
            EventType = evt,
            Timestamp = ts ?? DateTime.UtcNow,
            AggregatedData = meta,
        };
}
