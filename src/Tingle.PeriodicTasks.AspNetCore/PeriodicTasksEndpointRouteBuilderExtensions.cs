using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using Tingle.PeriodicTasks;
using Tingle.PeriodicTasks.AspNetCore;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// /// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add periodic tasks endpoints.
/// </summary>
public static class PeriodicTasksEndpointRouteBuilderExtensions
{
    private const string MapEndpointUnreferencedCodeWarning = "This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced.";
    private const string MapEndpointDynamicCodeWarning = "This API may perform reflection on the supplied delegate and its parameters. These types may require generated code and aren't compatible with native AOT applications.";

    /// <summary>
    /// Maps incoming requests to the paths for periodic tasks.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the routes to.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> for endpoints associated with periodic tasks.</returns>
    [RequiresUnreferencedCode(MapEndpointUnreferencedCodeWarning)]
    [RequiresDynamicCode(MapEndpointDynamicCodeWarning)]
    public static IEndpointConventionBuilder MapPeriodicTasks(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        ValidateServicesRegistered(endpoints);

        var routeGroup = endpoints.MapGroup("");

        routeGroup.MapGet("/registrations", Ok<List<PeriodicTaskRegistration>> ([FromServices] PeriodicTasksEndpointsHandler handler) => TypedResults.Ok(handler.GetRegistrations()));

        routeGroup.MapGet("/registrations/{name}", Ok<PeriodicTaskRegistration> ([FromServices] PeriodicTasksEndpointsHandler handler, [FromRoute] string name) => TypedResults.Ok(handler.GetRegistration(name)));

        routeGroup.MapGet("/registrations/{name}/history",
                          async Task<Results<Ok<IReadOnlyList<PeriodicTaskExecutionAttempt>>, ProblemHttpResult>> (HttpContext context,
                                                                                                                   [FromServices] IPeriodicTaskExecutionAttemptsStore attemptsStore,
                                                                                                                   [FromServices] PeriodicTasksEndpointsHandler handler,
                                                                                                                   [FromRoute] string name) =>
                          {
                              var registration = handler.GetRegistration(name);
                              if (registration is null) return RegistrationNotFound(name);

                              name = registration.Name;

                              var cancellationToken = context.RequestAborted;
                              var attempts = await attemptsStore.GetAttemptsAsync(name, cancellationToken: cancellationToken).ConfigureAwait(false);
                              return TypedResults.Ok(attempts);
                          });

        routeGroup.MapPost("/execute",
                           async Task<Results<Ok<PeriodicTaskExecutionAttempt>, ProblemHttpResult>> (HttpContext context,
                                                                                                     [FromServices] PeriodicTasksEndpointsHandler handler,
                                                                                                     [FromBody] PeriodicTaskExecutionRequest request) =>
                           {
                               var name = request.Name ?? throw new InvalidOperationException("The name of the periodic task must be provided!");
                               var registration = handler.GetRegistration(name);
                               if (registration is null) return RegistrationNotFound(name);

                               var cancellationToken = context.RequestAborted;
                               var attempt = await handler.ExecuteAsync(registration, request, cancellationToken).ConfigureAwait(false);
                               return TypedResults.Ok(attempt);
                           });

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

    private static ProblemHttpResult RegistrationNotFound(string name)
    {
        return TypedResults.Problem(title: "periodic_task_registration_not_found",
                                    detail: $"No periodic task named '{name}' exists.",
                                    statusCode: 400);
    }
}
