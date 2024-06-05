using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tingle.PeriodicTasks.Internal;

internal class PeriodicTaskRunnerCreator(IServiceProvider provider, IOptions<PeriodicTasksHostOptions> optionsAccessor)
{
    private readonly PeriodicTasksHostOptions options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));

    public IPeriodicTaskRunner Create(string name)
    {
        name = PeriodicTasksBuilder.TrimCommonSuffixes(name, true);
        if (!options.Registrations.TryGetValue(name, out var registration))
        {
            throw new InvalidOperationException($"A periodic task with the name '{name}' does not exist."
                + $" Ensure you call services.AddPeriodicTasks(builder => builder.AddTask(...)) when configuring your host.");
        }

        return Create(registration);
    }

    public IPeriodicTaskRunner Create(PeriodicTaskTypeRegistration registration) => (IPeriodicTaskRunner)provider.GetRequiredService(registration.RunnerType);
}
