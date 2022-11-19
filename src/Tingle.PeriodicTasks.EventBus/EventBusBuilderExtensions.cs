using Tingle.EventBus.Configuration;
using Tingle.PeriodicTasks.EventBus;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Extensions on <see cref="EventBusBuilder"/> for use with Periodic Tasks.</summary>
public static class EventBusBuilderExtensions
{
    /// <summary>Register the necessary services for triggering periodic tasks.</summary>
    /// <param name="builder">The <see cref="EventBusBuilder"/> to extend.</param>
    /// <returns></returns>
    public static EventBusBuilder AddPeriodicTasksTrigger(this EventBusBuilder builder)
    {
        return builder.AddConsumer<TriggerPeriodicTaskEventConsumer>();
    }

    /// <summary>Register the necessary services for triggering periodic tasks.</summary>
    /// <param name="builder">The <see cref="EventBusBuilder"/> to extend.</param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static EventBusBuilder AddPeriodicTasksTrigger(this EventBusBuilder builder, Action<EventRegistration, EventConsumerRegistration> configure)
    {
        return builder.AddConsumer<TriggerPeriodicTaskEventConsumer>(configure);
    }
}
