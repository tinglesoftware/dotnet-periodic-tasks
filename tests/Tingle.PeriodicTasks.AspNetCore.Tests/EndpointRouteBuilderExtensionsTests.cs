using Medallion.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Tingle.PeriodicTasks.AspNetCore.Tests;

public class EndpointRouteBuilderExtensionsTests(ITestOutputHelper outputHelper)
{
    private static readonly PeriodicTaskRegistration registration = new()
    {
        Name = "dummy",
        Type = typeof(DummyTask).FullName,
        AwaitExecution = true,
        Description = "some description here",
        Enable = true,
        ExecuteOnStartup = false,
        LockName = (typeof(DummyTask).Assembly.GetName().Name + ":dummy").ToLower(),
        LockTimeout = TimeSpan.Zero,
        Schedule = "0 10,40 * * * *",
        Deadline = TimeSpan.FromMinutes(59),
        Timezone = "Africa/Nairobi",
        ExecutionIdFormat = PeriodicTaskIdFormat.GuidNoDashes,
    };

    [Fact] // Matches based on '.Map'
    public async Task IgnoresRequestThatDoesNotMatchPath()
    {
        var builder = new WebHostBuilder()
            .ConfigureLogging(builder => builder.AddXUnit(outputHelper))
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPeriodicTasks(builder => builder.AddAspNetCore());
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGroup("/periodic-tasks").MapPeriodicTasks();
                });
            });
        using var server = new TestServer(builder);
        var client = server.CreateClient();

        var response = await client.GetAsync("/frob");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact] // Matches based on '.Map'
    public async Task MatchIsCaseInsensitive()
    {
        var builder = new WebHostBuilder()
            .ConfigureLogging(builder => builder.AddXUnit(outputHelper))
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPeriodicTasks(builder => builder.AddAspNetCore());
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGroup("/periodic-tasks").MapPeriodicTasks();
                });
            });
        using var server = new TestServer(builder);
        var client = server.CreateClient();

        var response = await client.GetAsync("/PERIODIC-tasks/registrations");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType!.ToString());
        Assert.NotEqual(0, response.Content.Headers.ContentLength);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ListingWorks()
    {
        var builder = new WebHostBuilder()
            .ConfigureLogging(builder => builder.AddXUnit(outputHelper))
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPeriodicTasks(builder =>
                {
                    builder.AddTask<DummyTask>();
                    builder.AddAspNetCore();
                });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGroup("/periodic-tasks").MapPeriodicTasks();
                });
            });
        using var server = new TestServer(builder);
        var client = server.CreateClient();

        var response = await client.GetFromJsonAsync<PeriodicTaskRegistration[]>("/periodic-tasks/registrations");
        Assert.NotNull(response);
        var actual = Assert.Single(response);
        Assert.Equal(registration, actual);
    }

    [Fact]
    public async Task GetWorks()
    {
        var builder = new WebHostBuilder()
            .ConfigureLogging(builder => builder.AddXUnit(outputHelper))
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPeriodicTasks(builder =>
                {
                    builder.AddTask<DummyTask>();
                    builder.AddAspNetCore();
                });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGroup("/periodic-tasks").MapPeriodicTasks();
                });
            });
        using var server = new TestServer(builder);
        var client = server.CreateClient();

        var actual = await client.GetFromJsonAsync<PeriodicTaskRegistration>("/periodic-tasks/registrations/dummy");
        Assert.NotNull(actual);
        Assert.Equal(registration, actual);
    }

    [Fact]
    public async Task GetHistoryWorks()
    {
        var builder = new WebHostBuilder()
            .ConfigureLogging(builder => builder.AddXUnit(outputHelper))
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddPeriodicTasks(builder =>
                {
                    builder.AddTask<DummyTask>();
                    builder.AddAspNetCore();
                });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGroup("/periodic-tasks").MapPeriodicTasks();
                });
            });
        using var server = new TestServer(builder);
        var client = server.CreateClient();

        var response = await client.GetFromJsonAsync<PeriodicTaskExecutionAttempt[]>("/periodic-tasks/registrations/dummy/history");
        Assert.NotNull(response);
        Assert.Empty(response);
    }

    [Fact]
    public async Task ExecuteWorks()
    {
        var builder = new WebHostBuilder()
            .ConfigureLogging(builder => builder.AddXUnit(outputHelper))
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddSingleton<IDistributedLockProvider, DummyDistributedLockProvider>();
                services.AddPeriodicTasks(builder =>
                {
                    builder.AddTask<DummyTask>();
                    builder.AddAspNetCore();
                });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGroup("/periodic-tasks").MapPeriodicTasks();
                });
            });
        using var server = new TestServer(builder);
        var client = server.CreateClient();

        var content = new StringContent("{\"name\":\"dummy\"}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/periodic-tasks/execute", content);
        //var attempt = await response.Content.ReadFromJsonAsync<PeriodicTaskExecutionAttempt>();
        //Assert.Null(attempt);
        Assert.Equal(0, response.Content.Headers.ContentLength);
    }

    [PeriodicTaskSchedule("10,40 * * * *", "Africa/Nairobi")]
    [PeriodicTaskDescription("some description here")]
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
