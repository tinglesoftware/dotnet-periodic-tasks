using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using Tingle.PeriodicTasks;

namespace Microsoft.Extensions.Hosting;

/// <summary>Extensions for <see cref="IHost"/>.</summary>
public static class IHostExtensions
{
    /// <summary>
    /// Execute a periodic task whose name is specified in the <c>PERIODIC_TASK_NAME</c> configuration value.
    /// If none is present or if <see langowrd="null"/>, the host is run instead using <c>host.RunAsync(cancellationToken)</c>.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/> to use.</param>
    /// <param name="throwOnError">Whether to throw an exception on failure.</param>
    /// <param name="awaitExecution">
    /// Gets or sets whether the task execution should be awaited.
    /// This overrides the value in <see cref="PeriodicTaskOptions.AwaitExecution"/>.
    /// </param>
    /// <param name="cancellationToken">The token to trigger termination or shutdown.</param>
    /// <returns></returns>
    public static Task RunOrExecutePeriodicTaskAsync(this IHost host,
                                                     bool throwOnError = true,
                                                     bool? awaitExecution = null,
                                                     CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);

        return host.TryGetPeriodicTaskName(out var taskName)
            ? host.ExecutePeriodicTaskAsync(taskName, throwOnError, awaitExecution, cancellationToken)
            : host.RunAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the name of the periodic task configured to run on startup, if any.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/> to use.</param>
    /// <param name="name">
    /// When this method returns, contains the name of the periodic task to run on startup,
    /// if the configuration key is found with a non-null value; otherwise, the default 
    /// value for the type of the value parameter.
    /// This parameter is passed uninitialized.
    /// <br/>
    /// This value can be used with
    /// <see cref="ExecutePeriodicTaskAsync(IHost, string, bool, bool?, CancellationToken)"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="IConfiguration"/> used by the <see cref="IHost"/>
    /// contains a non-null value for the <c>PERIODIC_TASK_NAME</c> configuration key;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryGetPeriodicTaskName(this IHost host, [NotNullWhen(true)] out string? name)
    {
        ArgumentNullException.ThrowIfNull(host);

        var configuration = host.Services.GetRequiredService<IConfiguration>();
        return configuration.TryGetPeriodicTaskName(out name);
    }

    /// <summary>Execute a periodic task.</summary>
    /// <param name="host">The <see cref="IHost"/> to use.</param>
    /// <param name="name">The name of the periodic task.</param>
    /// <param name="throwOnError">Whether to throw an exception on failure.</param>
    /// <param name="awaitExecution">
    /// Gets or sets whether the task execution should be awaited.
    /// This overrides the value in <see cref="PeriodicTaskOptions.AwaitExecution"/>.
    /// </param>
    /// <param name="cancellationToken">The token to trigger termination.</param>
    /// <returns></returns>
    public static Task ExecutePeriodicTaskAsync(this IHost host,
                                                string name,
                                                bool throwOnError = true,
                                                bool? awaitExecution = null,
                                                CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(name);

        // Create scoped provider so that services can be disposed at the end.
        // This is useful for scenarios such as logging.
        using var scope = host.Services.CreateScope();
        var provider = scope.ServiceProvider;

        // find the task registration
        var options = provider.GetRequiredService<IOptions<PeriodicTasksHostOptions>>().Value;
        name = PeriodicTasksBuilder.TrimCommonSuffixes(name, true);
        if (!options.Registrations.TryGetValue(name, out var type))
        {
            throw new InvalidOperationException($"A periodic task with the name '{name}' does not exist."
                + $" Ensure you call services.AddPeriodicTasks(builder => builder.AddTask(...)) when configuring your host.");
        }

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var genericRunnerType = typeof(IPeriodicTaskRunner<>);
        var runnerType = genericRunnerType.MakeGenericType(type);
        var runner = (IPeriodicTaskRunner)provider.GetRequiredService(runnerType);
        return runner.ExecuteAsync(name: name,
                                   throwOnError: throwOnError,
                                   awaitExecution: awaitExecution,
                                   cancellationToken: cts.Token);
    }
}
