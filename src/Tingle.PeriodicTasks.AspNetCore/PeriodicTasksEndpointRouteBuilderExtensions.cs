using Microsoft.AspNetCore.Routing;
using Tingle.PeriodicTasks.AspNetCore;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// /// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add periodic tasks endpoints.
/// </summary>
public static class PeriodicTasksEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps incoming requests to the paths for periodic tasks.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the routes to.</param>
    /// <param name="prefix">The path prefix for the endpoints exposed.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> for endpoints associated with the service.</returns>
    public static IEndpointConventionBuilder MapPeriodicTasks(this IEndpointRouteBuilder endpoints, string prefix = "/periodic-tasks")
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        ValidateServicesRegistered(endpoints);

        // in .NET 7 use route groups to avoid the prefix?
        prefix = prefix.TrimEnd('/');
        var builders = new[]
        {
            endpoints.MapGet($"{prefix}/registrations", (PeriodicTasksEndpointsHandler handler) => handler.List()),
            endpoints.MapGet($"{prefix}/registrations/{{name}}", (PeriodicTasksEndpointsHandler handler, string name) => handler.Get(name)),
            endpoints.MapGet($"{prefix}/registrations/{{name}}/history", (PeriodicTasksEndpointsHandler handler, string name) => handler.GetHistory(name)),
            endpoints.MapPost($"{prefix}/execute", (PeriodicTasksEndpointsHandler handler, PeriodicTaskExecutionRequest request) => handler.ExecuteAsync(request)),
        };

        IEndpointConventionBuilder builder = new PeriodicTasksEndpointConventionBuilder(builders);
        return builder.WithGroupName("periodic-tasks").WithDisplayName("Periodic Tasks");
    }

    private static void ValidateServicesRegistered(IEndpointRouteBuilder endpoints)
    {
        if (endpoints.ServiceProvider.GetService(typeof(PeriodicTasksMarkerService)) is null)
        {
            throw new InvalidOperationException("Unable to find the required services. Please add all the required services by calling " +
                "'builder.AddAspNetCore' inside the call to 'services.AddPeriodicTasks(...)' in the application startup code.");
        }
    }
}
