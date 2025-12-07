namespace OSFRS.SecurityTests.Utils;

public abstract class SecurityTestBase : IClassFixture<SecurityWebAppFactory>
{
    protected readonly SecurityWebAppFactory AppFactory;
    protected readonly SecurityTestClientFactory Clients;

    protected HttpClient Anonymous => Clients.CreateAnonymousClient();
    protected HttpClient User => Clients.CreateUserClient();
    protected HttpClient Admin => Clients.CreateAdminClient();

    protected SecurityTestBase(SecurityWebAppFactory factory)
    {
        AppFactory = factory;
        Clients = new SecurityTestClientFactory(factory);
        factory.ResetDatabase();
    }
}
