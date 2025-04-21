using Tingle.PeriodicTasks;

var host = Host.CreateDefaultBuilder(args)
               .ConfigureServices((context, services) =>
               {
                   var environment = context.HostingEnvironment;
                   var configuration = context.Configuration;

                   // register IDistributedLockProvider
                   var path = configuration.GetValue<string?>("DistributedLocking:FilePath")
                           ?? Path.Combine(environment.ContentRootPath, "distributed-locks");
                   services.AddSingleton<Medallion.Threading.IDistributedLockProvider>(provider =>
                   {
                       return new Medallion.Threading.FileSystem.FileDistributedSynchronizationProvider(Directory.CreateDirectory(path));
                   });

                   // register periodic tasks
                   services.AddPeriodicTasks(builder =>
                   {
                       builder.AddTask<DatabaseCleanerTask>(o =>
                       {
                           o.ExecuteOnStartup = true;
                           o.Schedule = "*/1 * * * *";
                       });
                       builder.AddTask<DnsCheckerTask>(o => o.Schedule = "*/5 * * * * *");
                   });
               })
               .Build();

await host.RunAsync();

class DatabaseCleanerTask(ILogger<DatabaseCleanerTask> logger) : IPeriodicTask
{
    public async Task ExecuteAsync(PeriodicTaskExecutionContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Cleaned up old records from the database");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}

class DnsCheckerTask(ILogger<DnsCheckerTask> logger) : IPeriodicTask
{
    public async Task ExecuteAsync(PeriodicTaskExecutionContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("All DNS records are fine");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}
