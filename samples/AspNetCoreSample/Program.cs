using Tingle.PeriodicTasks;

var builder = WebApplication.CreateBuilder(args);

// register IDistributedLockProvider
var path = builder.Configuration.GetValue<string?>("DistributedLocking:FilePath")
           ?? Path.Combine(builder.Environment.ContentRootPath, "distributed-locks");
builder.Services.AddSingleton<Medallion.Threading.IDistributedLockProvider>(provider =>
{
    return new Medallion.Threading.FileSystem.FileDistributedSynchronizationProvider(Directory.CreateDirectory(path));
});

builder.Services.AddPeriodicTasks(builder =>
{
    builder.AddTask<CookingTask>(o => o.Schedule = "0 */10 * * * *");
    builder.AddTask<WashingTask>(o => o.Schedule = "0 */15 * * * *");
    builder.AddAspNetCore();
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGroup("/periodic-tasks").MapPeriodicTasks();

await app.RunAsync();

class CookingTask : IPeriodicTask
{
    private readonly ILogger logger;

    public CookingTask(ILogger<CookingTask> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(PeriodicTaskExecutionContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("The food is ready to eat");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}

class WashingTask : IPeriodicTask
{
    private readonly ILogger logger;

    public WashingTask(ILogger<WashingTask> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(PeriodicTaskExecutionContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("I will do the cleaning later. Have you eaten yet?");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}
