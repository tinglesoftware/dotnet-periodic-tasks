using Tingle.EventBus;
using Tingle.PeriodicTasks;
using Tingle.PeriodicTasks.EventBus;

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
                       builder.AddTask<DatabaseCleanerTask>(o => o.Enable = false);
                       builder.AddTask<DnsCheckerTask>(o => o.Enable = false);
                   });

                   services.AddEventBus(builder =>
                   {
                       builder.AddInMemoryTransport();
                       builder.AddPeriodicTasksTrigger();
                       builder.Configure(options => options.Naming.UseFullTypeNames = false);
                   });

                   services.AddHostedService<PublisherService>();
               })
               .Build();

await host.RunAsync();

class DatabaseCleanerTask : IPeriodicTask
{
    private readonly ILogger logger;

    public DatabaseCleanerTask(ILogger<DatabaseCleanerTask> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(string name, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Cleaned up old records from the database");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}

class DnsCheckerTask : IPeriodicTask
{
    private readonly ILogger logger;

    public DnsCheckerTask(ILogger<DnsCheckerTask> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(string name, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("All DNS records are fine");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}

class PublisherService : BackgroundService
{
    private readonly IEventPublisher publisher;

    public PublisherService(IEventPublisher publisher)
    {
        this.publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // wait for host to have started

        var delay = TimeSpan.FromSeconds(5);
        var times = 5;

        var rnd = new Random(DateTimeOffset.UtcNow.Millisecond);

        for (var i = 0; i < times; i++)
        {
            var evt = new TriggerPeriodicTaskEvent
            {
                Name = rnd.Next(1, 10) > 5 ? "dnschecker" : "databasecleaner",
                Throw = true,
                Wait = true, // short lived one ...
            };

            await publisher.PublishAsync(evt, cancellationToken: stoppingToken);

            await Task.Delay(delay, stoppingToken);
        }
    }
}
