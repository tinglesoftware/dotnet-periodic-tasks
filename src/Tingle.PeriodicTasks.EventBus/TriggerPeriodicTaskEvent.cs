using Tingle.EventBus.Configuration;

namespace Tingle.PeriodicTasks.EventBus;

/// <summary>The event used for triggering periodic tasks via the EventBus.</summary>
[EntityKind(EntityKind.Queue)]
internal class TriggerPeriodicTaskEvent
{
    /// <summary>
    /// The name of the registered task to execute.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Whether to await execution to complete.
    /// </summary>
    public bool Wait { get; set; }

    /// <summary>
    /// Whether to throw an exception if one is encountered.
    /// </summary>
    public bool Throw { get; set; }
}
