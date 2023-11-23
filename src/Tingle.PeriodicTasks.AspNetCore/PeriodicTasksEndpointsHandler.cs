using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tingle.PeriodicTasks.Internal;

namespace Tingle.PeriodicTasks.AspNetCore;

internal class PeriodicTasksEndpointsHandler(PeriodicTaskRunnerCreator creator, IOptions<PeriodicTasksHostOptions> hostOptionsAccessor, IOptionsMonitor<PeriodicTaskOptions> optionsMonitor)
{
    private readonly PeriodicTasksHostOptions hostOptions = hostOptionsAccessor?.Value ?? throw new ArgumentNullException(nameof(hostOptionsAccessor));

    public List<PeriodicTaskRegistration> GetRegistrations()
    {
        var registrations = hostOptions.Registrations;
        var results = new List<PeriodicTaskRegistration>();
        foreach (var (name, type) in registrations)
        {
            var options = optionsMonitor.Get(name);
            results.Add(new()
            {
                Name = name,
                Type = type.FullName,
                Description = options.Description,
                Enable = options.Enable,
                ExecuteOnStartup = options.ExecuteOnStartup,
                Schedule = options.Schedule?.ToString(),
                Timezone = options.Timezone,
                LockTimeout = options.LockTimeout,
                AwaitExecution = options.AwaitExecution,
                Deadline = options.Deadline,
                ExecutionIdFormat = options.ExecutionIdFormat,
                LockName = options.LockName,
            });
        }
        return results;
    }

    public PeriodicTaskRegistration? GetRegistration(string name)
    {
        name = PeriodicTasksBuilder.TrimCommonSuffixes(name, true);
        return GetRegistrations().SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<PeriodicTaskExecutionAttempt?> ExecuteAsync(PeriodicTaskRegistration registration,
                                                                  PeriodicTaskExecutionRequest request,
                                                                  CancellationToken cancellationToken = default)
    {
        // create the runner
        var name = registration.Name ?? throw new InvalidOperationException("The name of the periodic task must be provided!");
        var runner = creator.Create(name);

        // execute
        return await runner.ExecuteAsync(name: name, throwOnError: request.Throw, awaitExecution: request.Wait, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
