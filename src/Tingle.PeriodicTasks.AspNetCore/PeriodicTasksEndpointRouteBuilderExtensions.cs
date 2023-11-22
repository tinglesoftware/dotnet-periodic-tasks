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
    /// <returns>A <see cref="IEndpointConventionBuilder"/> for endpoints associated with periodic tasks.</returns>
    [RequiresUnreferencedCode(MapEndpointTrimmerWarning)]
    public static IEndpointConventionBuilder MapPeriodicTasks(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        ValidateServicesRegistered(endpoints);

        var routeGroup = endpoints.MapGroup("");

        routeGroup.MapGet("/registrations", (PeriodicTasksEndpointsHandler handler) => handler.List());

        routeGroup.MapGet("/registrations/{name}", (PeriodicTasksEndpointsHandler handler, string name) => handler.Get(name));

        routeGroup.MapGet("/registrations/{name}/history", (PeriodicTasksEndpointsHandler handler, string name) => handler.GetHistory(name));

        routeGroup.MapPost("/execute", (PeriodicTasksEndpointsHandler handler, HttpContext context, PeriodicTaskExecutionRequest request) => handler.ExecuteAsync(context, request));

        return new PeriodicTasksEndpointConventionBuilder(routeGroup);
    }

    private static void ValidateServicesRegistered(IEndpointRouteBuilder endpoints)
    {
        if (endpoints.ServiceProvider.GetService(typeof(PeriodicTasksMarkerService)) is null)
        {
            throw new InvalidOperationException("Unable to find the required services. Please add all the required services by calling " +
                "'builder.AddAspNetCore' inside the call to 'services.AddPeriodicTasks(...)' in the application startup code.");
        }
    }

    // Wrap RouteGroupBuilder with a non-public type to avoid a potential future behavioral breaking change.
    private class PeriodicTasksEndpointConventionBuilder(RouteGroupBuilder inner) : IEndpointConventionBuilder
    {
        private IEndpointConventionBuilder InnerAsConventionBuilder => inner;

        public void Add(Action<EndpointBuilder> convention) => InnerAsConventionBuilder.Add(convention);
        public void Finally(Action<EndpointBuilder> finallyConvention) => InnerAsConventionBuilder.Finally(finallyConvention);
    }
}
