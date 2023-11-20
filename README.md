# Simplified periodic task scheduling for .NET

[![NuGet](https://img.shields.io/nuget/v/Tingle.PeriodicTasks.svg)](https://www.nuget.org/packages/Tingle.PeriodicTasks/)
[![GitHub Workflow Status](https://github.com/tinglesoftware/dotnet-periodic-tasks/actions/workflows/release.yml/badge.svg)](https://github.com/tinglesoftware/dotnet-periodic-tasks/actions/workflows/release.yml)
[![Dependabot](https://badgen.net/badge/Dependabot/enabled/green?icon=dependabot)](https://dependabot.com/)
[![license](https://img.shields.io/github/license/tinglesoftware/dotnet-periodic-tasks.svg?style=flat-square)](LICENSE)

This repository contains the code for the `Tingle.PeriodicTasks` libraries. This project exists to simplify the amount of work required to add periodic tasks to .NET projects. The existing libraries seem to have numerous complexities in setup especially when it comes to the use of framework concepts like dependency inject and options configuration. At [Tingle Software](https://tingle.software), we use this for all our periodic tasks that is based on .NET. However, the other libraries have more features such as persistence and user interfaces which are not yet available here.

## Packages

|Package|Description|
|--|--|
|[`Tingle.PeriodicTasks`](https://www.nuget.org/packages/Tingle.PeriodicTasks/)|Basic implementation of periodic tasks in .NET|
|[`Tingle.PeriodicTasks.AspNetCore`](https://www.nuget.org/packages/Tingle.PeriodicTasks.AspNetCore/)|AspNetCore endpoints for managing periodic tasks.|
|[`Tingle.PeriodicTasks.EventBus`](https://www.nuget.org/packages/Tingle.PeriodicTasks.EventBus/)|Support for triggering periodic tasks using events from [Tingle.EventBus](https://github.com/tinglesoftware/eventbus).|

## Documentation

### Getting started

Install the necessary library/libraries using Package Manager

```powershell
Install-Package Tingle.PeriodicTasks
Install-Package Tingle.PeriodicTasks.AspNetCore
Install-Package Tingle.PeriodicTasks.EventBus
```

Install the necessary library/libraries using dotnet CLI

```shell
dotnet add Tingle.PeriodicTasks
dotnet add Tingle.PeriodicTasks.AspNetCore
dotnet add Tingle.PeriodicTasks.EventBus
```

Create a periodic task

```cs
class DatabaseCleanerTask : IPeriodicTask
{
    private readonly ILogger logger;

    public DatabaseCleanerTask(ILogger<DatabaseCleanerTask> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(PeriodicTaskExecutionContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Cleaned up old records from the database");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}
```

Register the periodic task in your `Program.cs` file:

```cs
services.AddPeriodicTasks(builder =>
{
    builder.AddTask<DatabaseCleanerTask>(o => o.Schedule = "*/1 * * * *"); // every minute
});
```

To support running on multiple machines, distributed locking is used. See [library](https://github.com/madelson/DistributedLock) for more information.

You need to register `IDistributedLockProvider` this in your `Program.cs` file which can be backed by multiple sources. For this case, we use file-based locks.

```cs
// register IDistributedLockProvider
services.AddSingleton<Medallion.Threading.IDistributedLockProvider>(provider =>
{
    return new Medallion.Threading.FileSystem.FileDistributedSynchronizationProvider(Directory.CreateDirectory("distributed-locks"));
});
```

### Management via endpoints in AspNetCore

You can choose to manage the periodic tasks in using endpoints in AspNetCore. Update your application setup as follows.

```diff
  var app = builder.Build();

  app.MapGet("/", () => "Hello World!");

+ app.MapPeriodicTasks();

  await app.RunAsync();

```

> Remember to add authorization policies are needed. For example:

```cs
app.MapPeriodicTasks().RequireAuthorization("policy-name-here");
```

Endpoints available:

- `GET {prefix}/registrations`: list the registered periodic tasks
- `GET {prefix}/registrations/{name}`: retrieve the registration of a given periodic task by name
- `GET {prefix}/registrations/{name}/history`: retrieve the execution history of a given periodic task
- `POST {prefix}/execute`: execute a periodic task

  ```jsonc
  {
    "name": "DatabaseCleanerTask", // can also be DatabaseCleaner, databasecleaner
    "wait": true, // Whether to await execution to complete.
    "throw": true // Whether to throw an exception if one is encountered.
  }
  ```

### Triggering via [Tingle.EventBus](https://github.com/tinglesoftware/eventbus)

This helps trigger periodic tasks on demand or from another internal source without blocking. Update your EventBus setup as follows.

```diff
services.AddEventBus(builder =>
{
      builder.AddInMemoryTransport();
+     builder.AddPeriodicTasksTrigger();
});
```

Publish events using the [TriggerPeriodicTaskEvent](https://github.com/tinglesoftware/dotnet-periodic-tasks/blob/main/src/Tingle.PeriodicTasks.EventBus/TriggerPeriodicTaskEvent.cs) type. In JSON:

```jsonc
{
    "id": "...",
    "event": {
        "name": "DatabaseCleanerTask", // can also be DatabaseCleaner, databasecleaner
        "wait": true, // Whether to await execution to complete.
        "throw": true // Whether to throw an exception if one is encountered.
    },
    // omitted for brevity
}
```

### Disabling a task

By default, all registered periodic tasks are enabled. This means that they will always run as per schedule. In some scenarios you may want to disable them by default but execute them on demand [via AspNetCore endpoints](#management-via-endpoints-in-aspnetcore) or [via the EventBus](#triggering-via-tingleeventbus).

To disable a periodic task on registration:

```diff
services.AddPeriodicTasks(builder =>
{
    builder.AddTask<DatabaseCleanerTask>(o =>
    {
+         o.Enable = false;
          o.Schedule = "*/1 * * * *";
    });
});
```

To disable a periodic task via `IConfiguration`, update your `appsettings.json`:

```jsonc
{
  "PeriodicTasks": {
    "Tasks": {
      "DatabaseCleanerTask": { // Can use the FullName of the type
        "Enable": false
      }
    }
  }
}
```

To disable via environment variable:

|Name|Value|
|--|--|
|`PeriodicTasks__Tasks__DatabaseCleanerTask__Enable`|`"false"`|

### Manual triggers

In some cases, you want to run a periodic task instead of your application. This is supported so long as your app uses the `IHost` pattern.

Update your configuration or environment variables to add:

|Name/Key|Value|
|--|--|
|`PERIODIC_TASK_NAME`|`"DatabaseCleanerTask"`|

Next, update your application startup:

```diff
- app.RunAsync();
+ app.RunOrExecutePeriodicTaskAsync();
```

A common practice is to build one application project for your AspNetCore application housing your periodic tasks, but you need to run the database cleanup task in a separate process (e.g. a Kubernetes CronJob or Azure Container App Job). Offloading longer running or greedier tasks to a separate process, or just for easier visibility on sensitive ones is a good thing.

For Azure Container App Job, your bicep file would like:

```bicep
resource job 'Microsoft.App/jobs@2023-05-01' = {
  name: '...'
  properties: {
    environmentId: appEnvironment.id
    configuration: {
      triggerType: 'Schedule'
      scheduleTriggerConfig: {
        cronExpression: '10 * * * *'
      }
    }
    template: {
      containers: [
        {
          image: '...'
          name: containerName
          env: [
            { name: 'PERIODIC_TASK_NAME', value: 'DatabaseCleanerTask' }
            { name: 'EventBus__DefaultTransportWaitStarted', value: 'false' } // Ensure EventBus is available
          ]
        }
      ]
    }
  }
  // omitted for brevity
}
```

## Samples

- [Simple sample setup](./samples/SimpleSample)
- [Using IConfiguration to configure PeriodicTasks](./samples/ConfigSample)
- [Managing periodic tasks in AspNetCore](./samples/AspNetCoreSample)
- [Triggering periodic tasks using Tingle.EventBus](./samples/EventBusSample)

## Issues &amp; Comments

Please leave all comments, bugs, requests, and issues on the Issues page. We'll respond to your request ASAP!

### License

The Library is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refer to the [LICENSE](./LICENSE) file for more information.
