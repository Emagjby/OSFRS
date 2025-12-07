using System.Net;
using FluentAssertions;
using OSFRS.SecurityTests.Utils;

public class StatisticsAuthorizationTests : SecurityTestBase
{
    private const string BASE = "/api/statistics";

    public StatisticsAuthorizationTests(SecurityWebAppFactory factory)
        : base(factory) { }

    // -------------------------------------------------------------
    // Define endpoints with their correct HTTP method
    // -------------------------------------------------------------
    private readonly (string method, string url)[] Endpoints =
    {
        ("GET", $"{BASE}/events"),
        ("GET", $"{BASE}/aggregate/daily"),
        ("GET", $"{BASE}/aggregate/monthly"),
        ("POST", $"{BASE}/aggregate/run"),
        ("GET", $"{BASE}/reports/daily"),
        ("GET", $"{BASE}/reports/monthly"),
        ("GET", $"{BASE}/export/csv"),
        ("GET", $"{BASE}/export/pdf"),
        ("GET", $"{BASE}/analytics/trends/daily?from=2024-01-01&to=2024-02-01"),
        ("GET", $"{BASE}/analytics/trends/monthly"),
        ("GET", $"{BASE}/analytics/peaks?from=2024-01-01&to=2024-02-01"),
        ("GET", $"{BASE}/analytics/anomalies?from=2024-01-01&to=2024-02-01"),
        ("GET", $"{BASE}/analytics/visualization?from=2024-01-01&to=2024-02-01"),
    };

    // Utility to call proper verb
    private Task<HttpResponseMessage> Call(HttpClient client, string method, string url) =>
        method switch
        {
            "GET" => client.GetAsync(url),
            "POST" => client.PostAsync(url, null),
            _ => throw new NotSupportedException(method),
        };

    // -------------------------------------------------------------
    // Anonymous -> 401
    // -------------------------------------------------------------
    [Fact]
    public async Task Anonymous_Endpoints_Return_401()
    {
        foreach (var (method, url) in Endpoints)
        {
            var res = await Call(Anonymous, method, url);
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    // -------------------------------------------------------------
    // User -> 403
    // -------------------------------------------------------------
    [Fact]
    public async Task User_Endpoints_Return_403()
    {
        foreach (var (method, url) in Endpoints)
        {
            var res = await Call(User, method, url);
            res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }

    // -------------------------------------------------------------
    // Admin -> success (cannot be 401 or 403)
    // -------------------------------------------------------------
    [Fact]
    public async Task Admin_Endpoints_Return_2xx()
    {
        foreach (var (method, url) in Endpoints)
        {
            var res = await Call(Admin, method, url);
            res.StatusCode.Should()
                .NotBe(HttpStatusCode.Unauthorized)
                .And.NotBe(HttpStatusCode.Forbidden);
        }
    }
}
