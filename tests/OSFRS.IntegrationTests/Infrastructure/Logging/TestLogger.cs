using OSFRS.Backend.Interfaces.Logging;

namespace OSFRS.IntegrationTests.Infrastructure.Logging;

public class TestLogger<T> : IAppLogger<T>
{
    public void LogInformation(string message, params object[] args) { }

    public void LogWarning(string message, params object[] args) { }

    public void LogError(Exception exception, string message, params object[] args) { }

    public void LogError(string message, params object[] args) { }
}
