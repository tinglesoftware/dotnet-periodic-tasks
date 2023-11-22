using Medallion.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tingle.EventBus;
using Tingle.EventBus.Transports.InMemory;

namespace Tingle.PeriodicTasks.EventBus.Tests;

public class TriggerPeriodicTaskEventConsumerTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task ConsumeAsyncWorks()
    {
        var host = new HostBuilder()
            .ConfigureLogging(builder => builder.AddXUnit(outputHelper))
            .ConfigureServices(services =>
            {
                services.AddSingleton<IDistributedLockProvider, DummyDistributedLockProvider>();
                services.AddPeriodicTasks(builder =>
                {
                    builder.AddTask<DummyTask>();
                });

                services.AddEventBus(builder =>
                {
                    builder.AddInMemoryTransport();
                    builder.AddInMemoryTestHarness();
                    builder.AddPeriodicTasksTrigger();

                });
            })
            .Build();

        using var scope = host.Services.CreateScope();
        var provider = scope.ServiceProvider;
        var attemptsStore = provider.GetRequiredService<IPeriodicTaskExecutionAttemptsStore>();
        var publisher = provider.GetRequiredService<IEventPublisher>();
        var harness = provider.GetRequiredService<InMemoryTestHarness>();

        await harness.StartAsync();
        try
        {
            var evt = new TriggerPeriodicTaskEvent
            {
                Name = "dummy",
                Throw = true,
                Wait = true,
            };

            await publisher.PublishAsync(evt);

            Assert.Empty(await harness.FailedAsync(TimeSpan.FromSeconds(3)));

            var evt_ctx_con = Assert.IsType<EventContext<TriggerPeriodicTaskEvent>>(Assert.Single(harness.Consumed()));
            Assert.Equal(evt.Name, evt_ctx_con.Event.Name);

            var attempt = Assert.Single(await attemptsStore.GetAttemptsAsync());
            Assert.Equal(evt.Name, attempt.Name);
            Assert.True(attempt.Successful);
        }
        finally
        {
            await harness.StopAsync();
        }
    }

    class DummyTask(ILogger<DummyTask> logger) : IPeriodicTask
    {
        public async Task ExecuteAsync(PeriodicTaskExecutionContext context, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("This is a dummy task");
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }

    private class DummyDistributedLockProvider : IDistributedLockProvider
    {
        public IDistributedLock CreateLock(string name) => new DummyDistributedLock(name);

        private class DummyDistributedLock(string name) : IDistributedLock
        {
            public string Name { get; } = name;

            public IDistributedSynchronizationHandle Acquire(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
                => new DummyDistributedSynchronizationHandle();

            public ValueTask<IDistributedSynchronizationHandle> AcquireAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
                => ValueTask.FromResult<IDistributedSynchronizationHandle>(new DummyDistributedSynchronizationHandle());

            public IDistributedSynchronizationHandle? TryAcquire(TimeSpan timeout = default, CancellationToken cancellationToken = default)
                => new DummyDistributedSynchronizationHandle();

            public ValueTask<IDistributedSynchronizationHandle?> TryAcquireAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
                => ValueTask.FromResult<IDistributedSynchronizationHandle?>(new DummyDistributedSynchronizationHandle());
        }

        private class DummyDistributedSynchronizationHandle : IDistributedSynchronizationHandle
        {
            public CancellationToken HandleLostToken => CancellationToken.None;
            public void Dispose() { }
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }
}
