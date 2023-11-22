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

        routeGroup.MapGet("/registrations", Ok<List<PeriodicTaskRegistration>> ([FromServices] PeriodicTasksDataProvider provider) => TypedResults.Ok(provider.GetRegistrations()));

        routeGroup.MapGet("/registrations/{name}", Ok<PeriodicTaskRegistration> ([FromServices] PeriodicTasksDataProvider provider, [FromRoute] string name) => TypedResults.Ok(provider.GetRegistration(name)));

        routeGroup.MapGet("/registrations/{name}/history",
                          Results<Ok<List<PeriodicTaskExecutionAttempt>>, ProblemHttpResult> ([FromServices] PeriodicTasksDataProvider provider, [FromRoute] string name) =>
                          {
                              var registration = provider.GetRegistration(name);
                              if (registration is null) return RegistrationNotFound(name);


                              var attempts = new List<PeriodicTaskExecutionAttempt>(); // will have data once we have storage support
                              return TypedResults.Ok(attempts);
                          });

        routeGroup.MapPost("/execute",
                           async Task<Results<Ok<PeriodicTaskExecutionAttempt>, ProblemHttpResult>> ([FromServices] IServiceProvider serviceProvider,
                                                                                                     [FromServices] PeriodicTasksDataProvider dataProvider,
                                                                                                     [FromBody] PeriodicTaskExecutionRequest request) =>
                           {
                               var name = request.Name ?? throw new InvalidOperationException("The name of the periodic task must be provided!");
                               var registration = dataProvider.GetRegistration(name);
                               if (registration is null) return RegistrationNotFound(name);

                               var attempt = await dataProvider.ExecuteAsync(serviceProvider, registration, request).ConfigureAwait(false);
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
