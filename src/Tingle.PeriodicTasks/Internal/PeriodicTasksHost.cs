﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tingle.PeriodicTasks.Internal;

/// <summary>Host for <see cref="IPeriodicTaskRunner"/> instances.</summary>
internal class PeriodicTasksHost(IHostApplicationLifetime lifetime,
                                 IOptions<PeriodicTasksHostOptions> optionsAccessor,
                                 PeriodicTaskRunnerCreator creator,
                                 ILogger<PeriodicTasksHost> logger) : BackgroundService
{
    private readonly PeriodicTasksHostOptions options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await WaitForAppStartupAsync(lifetime, stoppingToken).ConfigureAwait(false))
        {
            logger.ApplicationDidNotStartup();
            return;
        }

        // create the tasks
        var tasks = new List<Task>();
        foreach (var (name, registration) in options.Registrations)
        {
            var runner = creator.Create(registration);
            tasks.Add(runner.RunAsync(name, stoppingToken));
        }

        // execute all runners in parallel
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static async Task<bool> WaitForAppStartupAsync(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
    {
        var startedTcs = new TaskCompletionSource<object>();
        var cancelledTcs = new TaskCompletionSource<object>();

        // register result setting using the cancellation tokens
        lifetime.ApplicationStarted.Register(() => startedTcs.SetResult(new { }));
        stoppingToken.Register(() => cancelledTcs.SetResult(new { }));

        var completedTask = await Task.WhenAny(startedTcs.Task, cancelledTcs.Task).ConfigureAwait(false);

        // if the completed task was the "app started" one, return true
        return completedTask == startedTcs.Task;
    }
}
