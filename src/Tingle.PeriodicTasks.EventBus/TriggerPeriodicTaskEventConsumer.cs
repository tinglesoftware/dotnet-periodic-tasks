using Tingle.EventBus;
using Tingle.PeriodicTasks.Internal;

namespace Tingle.PeriodicTasks.EventBus;

internal class TriggerPeriodicTaskEventConsumer(PeriodicTaskRunnerCreator creator) : IEventConsumer<TriggerPeriodicTaskEvent>
{
    public async Task ConsumeAsync(EventContext<TriggerPeriodicTaskEvent> context, CancellationToken cancellationToken)
    {
        // create the runner
        var request = context.Event;
        var name = request.Name ?? throw new InvalidOperationException("The request in the event must have a name");
        var runner = creator.Create(name);

        // execute
        await runner.ExecuteAsync(name: name, throwOnError: request.Throw, awaitExecution: request.Wait, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
