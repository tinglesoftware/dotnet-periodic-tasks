using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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

                       builder.UseAttemptStore<MyExecutionAttemptsStore>();
                   });

                   services.AddDbContext<MainDbContext>(options => options.UseInMemoryDatabase("sample"));
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

class MainDbContext(DbContextOptions<MainDbContext> options) : DbContext(options)
{
    public DbSet<MyExecution> Executions { get; set; }
}

class MyExecution // this is custom just to show how to use your own model but you can use PeriodicTaskExecutionAttempt directly
{
    [Key]
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? MachineName { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public bool Successful { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? ExceptionStackTrace { get; set; }
}

class MyExecutionAttemptsStore(MainDbContext dbContext) : IPeriodicTaskExecutionAttemptsStore
{
    public async Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetAttemptsAsync(int? count = null, CancellationToken cancellationToken = default)
    {
        var attempts = dbContext.Executions.AsQueryable();
        if (count is int limit) attempts = attempts.Take(limit);
        return await Transform(attempts).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetSuccessfulAttemptsAsync(int? count = null, CancellationToken cancellationToken = default)
    {
        var attempts = dbContext.Executions.AsQueryable().Where(a => a.Successful);
        if (count is int limit) attempts = attempts.Take(limit);
        return await Transform(attempts).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetAttemptsAsync(string name, int? count = null, CancellationToken cancellationToken = default)
    {
        var attempts = dbContext.Executions.AsQueryable().Where(a => a.Name == name);
        if (count is int limit) attempts = attempts.Take(limit);
        return await Transform(attempts).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetSuccessfulAttemptsAsync(string name, int? count = null, CancellationToken cancellationToken = default)
    {
        var attempts = dbContext.Executions.AsQueryable().Where(x => x.Name == name && x.Successful);
        if (count is int limit) attempts = attempts.Take(limit);
        return await Transform(attempts).ToListAsync(cancellationToken);
    }

    public async Task<PeriodicTaskExecutionAttempt?> GetLastAttemptAsync(string name, CancellationToken cancellationToken = default)
    {
        return await Transform(dbContext.Executions.AsQueryable().Where(a => a.Name == name).OrderByDescending(a => a.Start)).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PeriodicTaskExecutionAttempt?> GetLastSuccessfulAttemptAsync(string name, CancellationToken cancellationToken = default)
    {
        return await Transform(dbContext.Executions.AsQueryable().Where(a => a.Name == name && a.Successful).OrderByDescending(a => a.Start)).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(PeriodicTaskExecutionAttempt attempt, CancellationToken cancellationToken = default)
    {
        // here you can skip the more frequent ones by type or name

        var transformed = new MyExecution
        {
            Id = attempt.Id,
            Name = attempt.Name,
            MachineName = attempt.MachineName,
            Start = attempt.Start,
            End = attempt.End,
            Successful = attempt.Successful,
            ExceptionMessage = attempt.ExceptionMessage,
            ExceptionStackTrace = attempt.ExceptionStackTrace,
        };
        await dbContext.Executions.AddAsync(transformed, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<PeriodicTaskExecutionAttempt> Transform(IQueryable<MyExecution> query)
    {
        return from e in query
               select new PeriodicTaskExecutionAttempt
               {
                   Id = e.Id,
                   Name = e.Name,
                   MachineName = e.MachineName,
                   Start = e.Start,
                   End = e.End,
                   Successful = e.Successful,
                   ExceptionMessage = e.ExceptionMessage,
                   ExceptionStackTrace = e.ExceptionStackTrace,
               };
    }
}
