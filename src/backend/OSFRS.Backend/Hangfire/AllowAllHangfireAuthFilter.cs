using Hangfire.Dashboard;

public class AllowAllHangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}