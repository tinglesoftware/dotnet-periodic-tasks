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
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the routes to.</param>
    /// <param name="prefix">The path prefix for the endpoints exposed.</param>
    /// <returns>A <see cref="PeriodicTasksEndpointConventionBuilder"/> for endpoints associated with the service.</returns>
    public static PeriodicTasksEndpointConventionBuilder MapPeriodicTasks(this IEndpointRouteBuilder builder, string prefix = "/periodic-tasks")
    {
        ArgumentNullException.ThrowIfNull(builder);

        ValidateServicesRegistered(builder);

        // in .NET 7 use route groups to avoid the prefix?
        prefix = prefix.TrimEnd('/');
        var builders = new[]
        {
            builder.MapGet($"{prefix}/registrations", (PeriodicTasksEndpointsHandler handler) => handler.List()),
            builder.MapGet($"{prefix}/registrations/{{name}}", (PeriodicTasksEndpointsHandler handler, string name) => handler.Get(name)),
            builder.MapGet($"{prefix}/registrations/{{name}}/history", (PeriodicTasksEndpointsHandler handler, string name) => handler.GetHistory(name)),
            builder.MapPost($"{prefix}/execute", (PeriodicTasksEndpointsHandler handler, PeriodicTaskExecutionRequest request) => handler.ExecuteOnDemandAsync(request)),
        };

        return new PeriodicTasksEndpointConventionBuilder(builders).WithGroupName("periodic-tasks").WithDisplayName("Periodic Tasks");
    }

    private static void ValidateServicesRegistered(IEndpointRouteBuilder endpoints)
    {
        if (endpoints.ServiceProvider.GetService(typeof(PeriodicTasksEndpointsHandler)) is null)
        {
            throw new InvalidOperationException("Unable to find the required services. Please add all the required services by calling " +
                "'builder.AddAspNetCore' inside the call to 'services.AddPeriodicTasks(...)' in the application startup code.");
        }
    }
}
