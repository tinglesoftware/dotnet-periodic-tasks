using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tingle.PeriodicTasks.AspNetCore;

internal class PeriodicTasksEndpointsHandler(IOptions<PeriodicTasksHostOptions> hostOptionsAccessor, IOptionsMonitor<PeriodicTaskOptions> optionsMonitor)
{
    private readonly PeriodicTasksHostOptions hostOptions = hostOptionsAccessor?.Value ?? throw new ArgumentNullException(nameof(hostOptionsAccessor));

    public List<PeriodicTaskRegistration> GetRegistrations()
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

    public PeriodicTaskRegistration? GetRegistration(string name)
    {
        name = PeriodicTasksBuilder.TrimCommonSuffixes(name, true);
        return GetRegistrations().SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<PeriodicTaskExecutionAttempt?> ExecuteAsync(IServiceProvider provider,
                                                                  PeriodicTaskRegistration registration,
                                                                  PeriodicTaskExecutionRequest request,
                                                                  CancellationToken cancellationToken = default)
    {
        // find the task type
        var name = registration.Name ?? throw new InvalidOperationException("The name of the periodic task must be provided!");
        var type = hostOptions.Registrations[name];

        // make the runner type
        var genericRunnerType = typeof(IPeriodicTaskRunner<>);
#pragma warning disable IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
        var runnerType = genericRunnerType.MakeGenericType(type);
#pragma warning restore IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
        var runner = (IPeriodicTaskRunner)provider.GetRequiredService(runnerType);

        // execute
        return await runner.ExecuteAsync(name: name,
                                         throwOnError: request.Throw,
                                         awaitExecution: request.Wait,
                                         cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
