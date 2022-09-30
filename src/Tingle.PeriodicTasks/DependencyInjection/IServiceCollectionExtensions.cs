namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods on <see cref="IServiceCollection"/> for PeriodicTasks.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Add Periodic Tasks services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to add services to.</param>
    /// <returns>An <see cref="PeriodicTasksBuilder"/> to continue setting up the Periodic Tasks.</returns>
    public static PeriodicTasksBuilder AddPeriodicTasks(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        return new PeriodicTasksBuilder(services);
    }

    /// <summary>
    /// Add Periodic Tasks services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to add services to.</param>
    /// <param name="setupAction">An optional action for setting up the periodic tasks.</param>
    /// <returns>An <see cref="PeriodicTasksBuilder"/> to continue setting up the Periodic Tasks.</returns>
    public static IServiceCollection AddPeriodicTasks(this IServiceCollection services, Action<PeriodicTasksBuilder>? setupAction = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var builder = services.AddPeriodicTasks();

        setupAction?.Invoke(builder);

        return services;
    }
}
