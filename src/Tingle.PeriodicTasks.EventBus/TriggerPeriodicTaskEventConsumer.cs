using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tingle.EventBus;

namespace Tingle.PeriodicTasks.EventBus;

internal class TriggerPeriodicTaskEventConsumer : IEventConsumer<TriggerPeriodicTaskEvent>
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly PeriodicTasksHostOptions options;

    public TriggerPeriodicTaskEventConsumer(IServiceScopeFactory scopeFactory, IOptions<PeriodicTasksHostOptions> optionsAccessor)
    {
        this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
    }

    public async Task ConsumeAsync(EventContext<TriggerPeriodicTaskEvent> context, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var provider = scope.ServiceProvider;
        var request = context.Event;

        var name = request.Name ?? throw new InvalidOperationException("The request in the event must have a name");
        if (!options.Registrations.TryGetValue(request.Name, out var type))
        {
            throw new InvalidOperationException($"No periodic task named '{name}' exists.");
        }

        // make the runner type
        var genericRunnerType = typeof(IPeriodicTaskRunner<>);
        var runnerType = genericRunnerType.MakeGenericType(type);
        var runner = (IPeriodicTaskRunner)provider.GetRequiredService(runnerType);

        // execute
        var t = runner.ExecuteAsync(name: name, throwOnError: request.Throw, cancellationToken: cancellationToken);
        if (request.Wait)
        {
            await t.ConfigureAwait(false);
        }
    }
}
