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
    /// <param name="cancellationToken">The token to trigger termination or shutdown.</param>
    /// <returns></returns>
    public static Task RunOrExecutePeriodicTaskAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        if (host is null) throw new ArgumentNullException(nameof(host));

        return host.ShouldExecutePeriodicTask(out var taskName)
            ? host.ExecutePeriodicTaskAsync(taskName, cancellationToken)
            : host.RunAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if the host is configured to execute a periodic task rather than run the host.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/> to use.</param>
    /// <param name="taskName">
    /// The name of the periodic task to configured to execute. It can be passed uninitialized.
    /// <br/>
    /// When the method returns <see langword="true"/>, this value can be used with
    /// <see cref="ExecutePeriodicTaskAsync(IHost, string, CancellationToken)"/>.
    /// </param>
    /// <returns></returns>
    public static bool ShouldExecutePeriodicTask(this IHost host, [NotNullWhen(true)] out string? taskName)
    {
        if (host is null) throw new ArgumentNullException(nameof(host));

        var configuration = host.Services.GetRequiredService<IConfiguration>();
        return configuration.TryGetPeriodicTaskName(out taskName);
    }

    /// <summary>Execute a periodic task.</summary>
    /// <param name="host">The <see cref="IHost"/> to use.</param>
    /// <param name="name">The name of the periodic task.</param>
    /// <param name="cancellationToken">The token to trigger termination.</param>
    /// <returns></returns>
    public static Task ExecutePeriodicTaskAsync(this IHost host, string name, CancellationToken cancellationToken = default)
    {
        if (host is null) throw new ArgumentNullException(nameof(host));
        if (name is null) throw new ArgumentNullException(nameof(name));

        // find the task registration
        var provider = host.Services;
        var options = provider.GetRequiredService<IOptions<PeriodicTasksHostOptions>>().Value;
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
        return runner.ExecuteAsync(name: name, throwOnError: true, cancellationToken: cts.Token);
    }
}
