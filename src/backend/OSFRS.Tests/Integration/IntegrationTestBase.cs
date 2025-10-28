using Microsoft.AspNetCore.Mvc.Testing;

namespace OSFRS.Tests.Integration;

public class IntegrationTestBase : IClassFixture<TestApplicationFactory>
{
    protected readonly HttpClient _client;
    protected readonly TestApplicationFactory _factory;

    public IntegrationTestBase(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
}