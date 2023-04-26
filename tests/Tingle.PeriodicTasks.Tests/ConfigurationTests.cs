using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tingle.PeriodicTasks.EventBus.Tests;

public class ConfigurationTests
{
    private readonly ITestOutputHelper outputHelper;

    public ConfigurationTests(ITestOutputHelper outputHelper)
    {
        this.outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
    }

    [Fact]
    public void BindFromIConfiguration_Works()
    {
        var host = new HostBuilder()
            .ConfigureLogging(builder => builder.AddXUnit(outputHelper))
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["PeriodicTasks:LockNamePrefix"] = "random_prefix",
                    ["PeriodicTasks:Tasks:dummy:Description"] = "some description here",
                    ["PeriodicTasks:Tasks:dummy:Timezone"] = "Africa/Nairobi",
                    ["PeriodicTasks:Tasks:Tingle.PeriodicTasks.EventBus.Tests.ConfigurationTests+DummyTask:AwaitExecution"] = "false",
                    ["PeriodicTasks:Tasks:Tingle.PeriodicTasks.EventBus.Tests.ConfigurationTests+DummyTask:Deadline"] = "00:15:00",
                    ["PeriodicTasks:Tasks:Tingle.PeriodicTasks.EventBus.Tests.ConfigurationTests+DummyTask:ExecutionIdFormat"] = "Long",
                    ["PeriodicTasks:Tasks:Tingle.PeriodicTasks.EventBus.Tests.ConfigurationTests+DummyTask:LockName"] = "stupid-lock",
                });
            })
            .ConfigureServices(services =>
            {
                services.AddPeriodicTasks(builder =>
                {
                    builder.AddTask<DummyTask>();
                });
            })
            .Build();

        using var scope = host.Services.CreateScope();
        var provider = scope.ServiceProvider;

        var hostOptions = provider.GetRequiredService<IOptions<PeriodicTasksHostOptions>>().Value;
        Assert.Equal("random_prefix", hostOptions.LockNamePrefix);

        var taskOptions = provider.GetRequiredService<IOptionsMonitor<PeriodicTaskOptions>>().Get(Assert.Single(hostOptions.Registrations.Keys));
        Assert.Equal("some description here", taskOptions.Description);
        Assert.Equal("Africa/Nairobi", taskOptions.Timezone);
        Assert.False(taskOptions.AwaitExecution);
        Assert.Equal(TimeSpan.FromMinutes(15), taskOptions.Deadline);
        Assert.Equal(PeriodicTaskIdFormat.Long, taskOptions.ExecutionIdFormat);
        Assert.Equal("stupid-lock", taskOptions.LockName);
    }

    class DummyTask : IPeriodicTask
    {
        public Task ExecuteAsync(PeriodicTaskExecutionContext context, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
