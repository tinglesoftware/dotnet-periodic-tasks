﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tingle.PeriodicTasks.AspNetCore;

internal class PeriodicTasksEndpointsHandler(IOptions<PeriodicTasksHostOptions> hostOptionsAccessor, IOptionsMonitor<PeriodicTaskOptions> optionsMonitor)
{
    private readonly PeriodicTasksHostOptions hostOptions = hostOptionsAccessor?.Value ?? throw new ArgumentNullException(nameof(hostOptionsAccessor));

    public IResult List() => Results.Ok(GetRegistrations());

    public IResult Get(string name) => Results.Ok(GetRegistration(PeriodicTasksBuilder.TrimCommonSuffixes(name, true)));

    public IResult GetHistory(string name)
    {
        name = PeriodicTasksBuilder.TrimCommonSuffixes(name, true);
        var registration = GetRegistration(name);
        if (registration is null) return RegistrationNotFound(name);

        var attempts = new List<PeriodicTaskExecutionAttempt>(); // will have data once we have storage support
        return Results.Ok(attempts);
    }

    public async Task<IResult> ExecuteAsync(HttpContext context, PeriodicTaskExecutionRequest request)
    {
        var name = request.Name ?? throw new InvalidOperationException("The name of the periodic task must be provided!");
        name = PeriodicTasksBuilder.TrimCommonSuffixes(name, true);

        var registration = GetRegistration(name);
        if (registration is null) return RegistrationNotFound(name);

        // find the task type
        var provider = context.RequestServices;
        var type = hostOptions.Registrations[name];

        // make the runner type
        var genericRunnerType = typeof(IPeriodicTaskRunner<>);
#pragma warning disable IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
        var runnerType = genericRunnerType.MakeGenericType(type);
#pragma warning restore IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
        var runner = (IPeriodicTaskRunner)provider.GetRequiredService(runnerType);

        // execute
        var cancellationToken = context.RequestAborted;
        await runner.ExecuteAsync(name: name,
                                  throwOnError: request.Throw,
                                  awaitExecution: request.Wait,
                                  cancellationToken: cancellationToken).ConfigureAwait(false);

        PeriodicTaskExecutionAttempt? attempt = null; // when attempt is offered, use it
        return Results.Ok(attempt);
    }

    private PeriodicTaskRegistration? GetRegistration(string name)
        => GetRegistrations().SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

    private List<PeriodicTaskRegistration> GetRegistrations()
    {
        var registrations = hostOptions.Registrations;
        var results = new List<PeriodicTaskRegistration>();
        foreach (var (name, type) in registrations)
        {
            var options = optionsMonitor.Get(name);
            results.Add(new PeriodicTaskRegistration(name, type, options));
        }
        return results;
    }

    private static IResult RegistrationNotFound(string name)
    {
        return Results.Problem(title: "periodic_task_registration_not_found",
                               detail: $"No periodic task named '{name}' exists.",
                               statusCode: 400);
    }
}
