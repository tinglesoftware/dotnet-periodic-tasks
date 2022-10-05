namespace Tingle.PeriodicTasks.AspNetCore;

internal sealed record PeriodicTaskRegistration
{
    public PeriodicTaskRegistration() { }

    public PeriodicTaskRegistration(string name, Type type, PeriodicTaskOptions options)
    {
        Name = name;
        Type = type.FullName;
        Enable = options.Enable;
        ExecuteOnStartup = options.ExecuteOnStartup;
        Schedule = options.Schedule.ToString();
        Timezone = options.Timezone.Id;
        LockTimeout = options.LockTimeout;
        AwaitExecution = options.AwaitExecution;
        Deadline = options.Deadline;
        ExecutionIdFormat = options.ExecutionIdFormat;
        LockName = options.LockName;
    }

    public string? Name { get; set; }
    public string? Type { get; set; }
    public bool Enable { get; set; }
    public bool ExecuteOnStartup { get; set; }
    public string? Schedule { get; set; }
    public string? Timezone { get; set; }
    public TimeSpan? LockTimeout { get; set; }
    public bool AwaitExecution { get; set; }
    public TimeSpan Deadline { get; set; }
    public PeriodicTaskIdFormat ExecutionIdFormat { get; set; }
    public string? LockName { get; set; }
}
