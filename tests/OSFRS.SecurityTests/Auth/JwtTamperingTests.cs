using System.Net;
using System.Text;
using FluentAssertions;
using OSFRS.Models.Entities;
using OSFRS.SecurityTests.Utils;

public class JwtTamperingTests : SecurityTestBase
{
    public JwtTamperingTests(SecurityWebAppFactory factory)
        : base(factory) { }

    private const string EP = "/api/facility";

    // ================================================
    // Helpers
    // ================================================

    private static string Base64UrlEncode(string s) => Base64UrlEncode(Encoding.UTF8.GetBytes(s));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string Base64UrlDecodeToString(string s)
    {
        string padded = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2:
                padded += "==";
                break;
            case 3:
                padded += "=";
                break;
        }
        return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }

    private static string ReplacePart(
        string token,
        Func<string, string> transformHeader = null!,
        Func<string, string> transformPayload = null!
    )
    {
        var parts = token.Split('.');
        var header = parts[0];
        var payload = parts[1];
        var sig = parts[2];

        var newHeader =
            transformHeader != null
                ? Base64UrlEncode(transformHeader(Base64UrlDecodeToString(header)))
                : header;

        var newPayload =
            transformPayload != null
                ? Base64UrlEncode(transformPayload(Base64UrlDecodeToString(payload)))
                : payload;

        // signature intentionally unchanged -> invalid JWT
        return $"{newHeader}.{newPayload}.{sig}";
    }

    private string MakeValidToken(int id = 1)
    {
        return Clients.TokenGenerator.GenerateToken(
            new User
            {
                Id = id,
                Username = $"user{id}",
                Email = $"u{id}@test.com",
                Role = "User",
            }
        );
    }

    // ================================================
    // 1. Modify payload → 401
    // ================================================
    [Fact]
    public async Task Payload_Tampering_Returns_401()
    {
        var token = MakeValidToken(10);

        var tampered = ReplacePart(
            token,
            transformPayload: payload =>
            {
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                    payload
                )!;

                // Force role escalation
                dict["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] = "Admin";

                return System.Text.Json.JsonSerializer.Serialize(dict);
            }
        );

        var client = Clients.CreateClientWithToken(tampered);
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ================================================
    // 2. "alg": "none" attack → 401
    // ================================================
    [Fact]
    public async Task Alg_None_Header_Returns_401()
    {
        var token = MakeValidToken(11);

        var tampered = ReplacePart(
            token,
            transformHeader: header => "{\"alg\":\"none\",\"typ\":\"JWT\"}"
        );

        var client = Clients.CreateClientWithToken(tampered);
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ================================================
    // 3. Remove exp → 401
    // ================================================
    [Fact]
    public async Task Remove_Expiry_Returns_401()
    {
        var token = MakeValidToken(12);

        var tampered = ReplacePart(
            token,
            transformPayload: payload =>
            {
                // remove exp field entirely
                var json = System.Text.Json.JsonDocument.Parse(payload).RootElement;
                var dict = new Dictionary<string, object>();

                foreach (var prop in json.EnumerateObject())
                    if (prop.Name != "exp")
                        dict[prop.Name] = prop.Value.ToString();

                return System.Text.Json.JsonSerializer.Serialize(dict);
            }
        );

        var client = Clients.CreateClientWithToken(tampered);
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ================================================
    // 4. Give future exp but invalid sig → 401
    // ================================================
    [Fact]
    public async Task Future_Expiry_With_Invalid_Signature_Returns_401()
    {
        var token = MakeValidToken(13);

        var tampered = ReplacePart(
            token,
            transformPayload: payload =>
            {
                var json = System.Text.Json.JsonDocument.Parse(payload).RootElement;
                var dict = new Dictionary<string, object>();

                foreach (var prop in json.EnumerateObject())
                    dict[prop.Name] = prop.Value.ToString();

                dict["exp"] = (DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds()).ToString();

                return System.Text.Json.JsonSerializer.Serialize(dict);
            }
        );

        var client = Clients.CreateClientWithToken(tampered);
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ================================================
    // 5. Inject custom admin claim → 401
    // ================================================
    [Fact]
    public async Task Inject_Admin_Claim_Returns_401()
    {
        var token = MakeValidToken(14);

        var tampered = ReplacePart(
            token,
            transformPayload: payload =>
            {
                var obj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                    payload
                )!;
                obj["isAdmin"] = true;
                return System.Text.Json.JsonSerializer.Serialize(obj);
            }
        );

        var client = Clients.CreateClientWithToken(tampered);
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ================================================
    // 6. Break Base64 encoding → 401
    // ================================================
    [Fact]
    public async Task Corrupt_Base64_Returns_401()
    {
        var token = MakeValidToken(15);

        var broken = token.Replace('.', '!'); // breaks formatting

        var client = Clients.CreateClientWithToken(broken);
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ================================================
    // 7. Mix & match header/payload from two tokens → 401
    // ================================================
    [Fact]
    public async Task Header_From_A_Payload_From_B_Returns_401()
    {
        var a = MakeValidToken(16);
        var b = MakeValidToken(17);

        var ha = a.Split('.')[0];
        var pb = b.Split('.')[1];
        var sig = a.Split('.')[2];

        var recombined = $"{ha}.{pb}.{sig}";

        var client = Clients.CreateClientWithToken(recombined);
        var res = await client.GetAsync(EP);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
