using Hangfire.Dashboard;

/// <summary>
/// Authorization filter that allows unrestricted access to the Hangfire Dashboard.
/// </summary>
/// <remarks>
/// This implementation always returns <c>true</c>, meaning:
/// <list type="bullet">
///   <item>Anyone can view the dashboard</item>
///   <item>No authentication or role checks are performed</item>
///   <item>Intended only for development or internal environments</item>
/// </list>
/// For production deployments, replace this with a secure authorization filter
/// that validates users or roles.
/// </remarks>
public class AllowAllHangfireAuthFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// Determines whether the current request is authorized to access the dashboard.
    /// </summary>
    /// <param name="context">The Hangfire dashboard request context.</param>
    /// <returns>
    /// Always returns <c>true</c>, allowing unrestricted dashboard access.
    /// </returns>
    public bool Authorize(DashboardContext context) => true;
}