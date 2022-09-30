namespace Tingle.PeriodicTasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public record PeriodicTaskExecutionAttempt
{
    public virtual string? Id { get; set; }
    public virtual string? Name { get; set; }
    public virtual string? ApplicationName { get; set; }
    public virtual string? EnvironmentName { get; set; }
    public virtual string? MachineName { get; set; }
    public virtual DateTimeOffset Start { get; set; }
    public virtual DateTimeOffset End { get; set; }
    public virtual bool Successful { get; set; }
    public virtual string? ExceptionMessage { get; set; }
    public virtual string? ExceptionStackTrace { get; set; }
}
