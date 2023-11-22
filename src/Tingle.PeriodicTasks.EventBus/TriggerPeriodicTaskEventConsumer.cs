using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tingle.EventBus;

namespace Tingle.PeriodicTasks.EventBus;

internal class TriggerPeriodicTaskEventConsumer(IServiceScopeFactory scopeFactory,
                                                IOptions<PeriodicTasksHostOptions> optionsAccessor,
                                                ILogger<TriggerPeriodicTaskEventConsumer> logger) : IEventConsumer<TriggerPeriodicTaskEvent>
{
    private readonly PeriodicTasksHostOptions options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));

    public async Task ConsumeAsync(EventContext<TriggerPeriodicTaskEvent> context, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var provider = scope.ServiceProvider;
        var request = context.Event;

        var name = request.Name ?? throw new InvalidOperationException("The request in the event must have a name");
        name = PeriodicTasksBuilder.TrimCommonSuffixes(name, true);
        if (!options.Registrations.TryGetValue(name, out var type))
        {
            logger.PeriodicTaskNotFound(name);
            return;
        }

        // make the runner type
        var genericRunnerType = typeof(IPeriodicTaskRunner<>);
        var runnerType = genericRunnerType.MakeGenericType(type);
        var runner = (IPeriodicTaskRunner)provider.GetRequiredService(runnerType);

        // execute
        await runner.ExecuteAsync(name: name,
                                  throwOnError: request.Throw,
                                  awaitExecution: request.Wait,
                                  cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
