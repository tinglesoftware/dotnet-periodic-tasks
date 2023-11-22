using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Tingle.PeriodicTasks;
using Tingle.PeriodicTasks.Internal;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A builder class for adding and configuring the Periodic Tasks in <see cref="IServiceCollection"/>.
/// </summary>
public class PeriodicTasksBuilder
{
    private static readonly Regex trimPattern = new("(Job|Task|JobTask|PeriodicTask)$", RegexOptions.Compiled);

    /// <summary>
    /// Creates an instance of <see cref="PeriodicTasksBuilder"/>.
    /// </summary>
    /// <param name="services"></param>
    public PeriodicTasksBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));

        // Configure the options
        Services.ConfigureOptions<PeriodicTasksHostConfigureOptions>();
        Services.ConfigureOptions<PeriodicTaskConfigureOptions>();

        // Register necessary services
        Services.AddSingleton<IPeriodicTasksConfigurationProvider, DefaultPeriodicTasksConfigurationProvider>();
        Services.AddSingleton<IPeriodicTaskIdGenerator, PeriodicTaskIdGenerator>();
        Services.AddHostedService<PeriodicTasksHost>();
    }

    /// <summary>
    /// The instance of <see cref="IServiceCollection"/> that this builder instance adds to.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>Configure options for the PeriodicTasks host.</summary>
    /// <param name="configure">An <see cref="Action"/> to further configure <see cref="PeriodicTasksHostOptions"/> instances.</param>
    /// <returns></returns>
    public PeriodicTasksBuilder Configure(Action<PeriodicTasksHostOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        Services.Configure(configure);
        return this;
    }

    /// <summary>Add a period task.</summary>
    /// <typeparam name="TTask">The period task to execute.</typeparam>
    /// <param name="name">The name of the task.</param>
    /// <param name="configure">Used to configure the periodic task options.</param>
    /// <returns>The <see cref="PeriodicTasksBuilder"/> instance used to run this task.</returns>
    public PeriodicTasksBuilder AddTask<[DynamicallyAccessedMembers(TrimmingHelper.Task)] TTask>(string name, Action<PeriodicTaskOptions> configure)
        where TTask : class, IPeriodicTask
    {
        ArgumentNullException.ThrowIfNull(configure);

        var tt = typeof(TTask);

        Configure(opt =>
        {
            if (opt.Registrations.TryGetValue(name, out var r))
            {
                throw new InvalidOperationException($"A task with the name '{name}' has already been registered. Names are case insensitive.");
            }

            opt.AddRegistration(name, tt);
        });

        Services.Configure(name, configure);
        return AddTaskRunner<TTask, PeriodicTaskRunner<TTask>>();
    }

    /// <summary>Add a period task.</summary>
    /// <typeparam name="TTask">The period task to execute.</typeparam>
    /// <param name="name">The name of the task.</param>
    /// <returns>The <see cref="PeriodicTasksBuilder"/> instance used to run this task.</returns>
    public PeriodicTasksBuilder AddTask<[DynamicallyAccessedMembers(TrimmingHelper.Task)] TTask>(string name) where TTask : class, IPeriodicTask
    {
        return AddTask<TTask>(name, options => { /* nothing to do */ });
    }

    /// <summary>Add a period task.</summary>
    /// <typeparam name="TTask">The period task to execute.</typeparam>
    /// <param name="name">The name of the task.</param>
    /// <param name="schedule">The execution schedule.</param>
    /// <returns>The <see cref="PeriodicTasksBuilder"/> instance used to run this task.</returns>
    public PeriodicTasksBuilder AddTask<[DynamicallyAccessedMembers(TrimmingHelper.Task)] TTask>(string name, CronSchedule schedule) where TTask : class, IPeriodicTask
    {
        return AddTask<TTask>(name, options => options.Schedule = schedule);
    }

    /// <summary>Add a period task.</summary>
    /// <typeparam name="TTask">The period task to execute.</typeparam>
    /// <param name="configure">Used to configure the periodic task options.</param>
    /// <returns>The <see cref="PeriodicTasksBuilder"/> instance used to run this task.</returns>
    public PeriodicTasksBuilder AddTask<[DynamicallyAccessedMembers(TrimmingHelper.Task)] TTask>(Action<PeriodicTaskOptions> configure) where TTask : class, IPeriodicTask
        => AddTask<TTask>(MakeName<TTask>(), configure);

    /// <summary>Add a period task.</summary>
    /// <typeparam name="TTask">The period task to execute.</typeparam>
    /// <returns>The <see cref="PeriodicTasksBuilder"/> instance used to run this task.</returns>
    public PeriodicTasksBuilder AddTask<[DynamicallyAccessedMembers(TrimmingHelper.Task)] TTask>() where TTask : class, IPeriodicTask
    {
        return AddTask<TTask>(MakeName<TTask>(), options => { /* nothing to do */ });
    }

    /// <summary>Add a period task.</summary>
    /// <typeparam name="TTask">The period task to execute.</typeparam>
    /// <param name="schedule">The execution schedule.</param>
    /// <returns>The <see cref="PeriodicTasksBuilder"/> instance used to run this task.</returns>
    public PeriodicTasksBuilder AddTask<[DynamicallyAccessedMembers(TrimmingHelper.Task)] TTask>(CronSchedule schedule) where TTask : class, IPeriodicTask
    {
        return AddTask<TTask>(MakeName<TTask>(), options => options.Schedule = schedule);
    }

    /// <summary>Add a runner for a task.</summary>
    /// <typeparam name="TTask">The type of task for which the runner is being added.</typeparam>
    /// <typeparam name="TRunner">The type of runner to be added.</typeparam>
    /// <remarks>
    /// This is only required if you need to override the default
    /// runner behaviour which is sufficient in most cases.
    /// </remarks>
    public PeriodicTasksBuilder AddTaskRunner<[DynamicallyAccessedMembers(TrimmingHelper.Task)] TTask, [DynamicallyAccessedMembers(TrimmingHelper.Runner)] TRunner>()
        where TTask : class, IPeriodicTask
        where TRunner : class, IPeriodicTaskRunner<TTask>
    {
        Services.AddSingleton<IPeriodicTaskRunner<TTask>, TRunner>();
        return this;
    }

    /// <summary>Add a runner for a task.</summary>
    /// <typeparam name="TTask">The type of task for which the runner is being added.</typeparam>
    /// <param name="runner">The runner to be registered</param>
    /// <remarks>
    /// This is only required if you need to override the default
    /// runner behaviour which is sufficient in most cases.
    /// </remarks>
    public PeriodicTasksBuilder AddTaskRunner<[DynamicallyAccessedMembers(TrimmingHelper.Task)] TTask>(IPeriodicTaskRunner<TTask> runner)
        where TTask : class, IPeriodicTask
    {
        Services.AddSingleton(runner);
        return this;
    }

    internal static string MakeName<TTask>(bool trim = true) where TTask : class, IPeriodicTask
    {
        var type = typeof(TTask);
        return type.IsGenericType
            ? throw new InvalidOperationException("Names cannot be automatically derived for generic types. Pass a name when adding a task.")
            : TrimCommonSuffixes(type.Name, trim).ToLowerInvariant();
    }

    internal static string TrimCommonSuffixes(string untrimmed, bool trim) => trim ? trimPattern.Replace(untrimmed, "") : untrimmed;
}
