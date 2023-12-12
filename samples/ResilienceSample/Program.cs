using Polly.Retry;
using Polly;
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
                           o.Schedule = "*/1 * * * *";
                           o.ResiliencePipeline = new ResiliencePipelineBuilder()
                                                        .AddRetry(new RetryStrategyOptions
                                                        {
                                                            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                                                            Delay = TimeSpan.FromSeconds(1),
                                                            MaxRetryAttempts = 3,
                                                            BackoffType = DelayBackoffType.Constant,
                                                            OnRetry = args =>
                                                            {
                                                                Console.WriteLine($"Attempt {args.AttemptNumber} failed; retrying in {args.RetryDelay}");
                                                                return ValueTask.CompletedTask;
                                                            },
                                                        })
                                                        .Build();
                       });
                   });
               })
               .Build();

await host.RunAsync();

class DatabaseCleanerTask(ILogger<DatabaseCleanerTask> logger) : IPeriodicTask
{
    public async Task ExecuteAsync(PeriodicTaskExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (Random.Shared.Next(1, 5) > 2) // 60% of the time
        {
            throw new Exception("Failed to clean up old records from the database");
        }

        logger.LogInformation("Cleaned up old records from the database");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}
