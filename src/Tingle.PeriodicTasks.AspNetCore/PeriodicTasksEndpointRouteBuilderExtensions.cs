using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using Tingle.PeriodicTasks.AspNetCore;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// /// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add periodic tasks endpoints.
/// </summary>
public static class PeriodicTasksEndpointRouteBuilderExtensions
{
    internal const string MapEndpointTrimmerWarning = "This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced.";

    /// <summary>
    /// Maps incoming requests to the paths for periodic tasks.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the routes to.</param>
    /// <param name="prefix">The path prefix for the endpoints exposed.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> for endpoints associated with periodic tasks.</returns>
    [RequiresUnreferencedCode(MapEndpointTrimmerWarning)]
    public static IEndpointConventionBuilder MapPeriodicTasks(this IEndpointRouteBuilder endpoints, string prefix = "/periodic-tasks")
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        ValidateServicesRegistered(endpoints);

        var group = endpoints.MapGroup(prefix);
        group.MapGet("/registrations", (PeriodicTasksEndpointsHandler handler) => handler.List())
             .WithDisplayName("periodic-tasks-list");

        group.MapGet("/registrations/{name}", (PeriodicTasksEndpointsHandler handler, string name) => handler.Get(name))
             .WithDisplayName("periodic-tasks-get");

        group.MapGet("/registrations/{name}/history", (PeriodicTasksEndpointsHandler handler, string name) => handler.GetHistory(name))
             .WithDisplayName("periodic-tasks-history");

        group.MapPost("/execute", (PeriodicTasksEndpointsHandler handler, HttpContext context, PeriodicTaskExecutionRequest request) => handler.ExecuteAsync(context, request))
             .WithDisplayName("periodic-tasks-execute");

        return group.WithGroupName("periodic-tasks");
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
