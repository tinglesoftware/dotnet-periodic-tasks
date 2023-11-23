namespace Tingle.PeriodicTasks;

/// <summary>
/// Represents and execution attempt for a periodic task.
/// </summary>
public record PeriodicTaskExecutionAttempt
{
    /// <summary>Unique identifier for the execution attempt.</summary>
    public virtual string? Id { get; set; }

    /// <summary>Name of the periodic task.</summary>
    public virtual string? Name { get; set; }

    /// <summary>Name of the application where the attempt was made.</summary>
    public virtual string? ApplicationName { get; set; }

    /// <summary>Execution environment under which the attempt was made.</summary>
    public virtual string? EnvironmentName { get; set; }

    /// <summary>Name of the machine where the attempt was made.</summary>
    public virtual string? MachineName { get; set; }

    /// <summary>Start time of the attempt.</summary>
    public virtual DateTimeOffset Start { get; set; }

    /// <summary>End time of the attempt.</summary>
    public virtual DateTimeOffset End { get; set; }

    /// <summary>Whether the attempt was successful.</summary>
    public virtual bool Successful { get; set; }

    /// <summary>Exception message if the attempt was not successful.</summary>
    public virtual string? ExceptionMessage { get; set; }

    /// <summary>Exception stack trace if the attempt was not successful.</summary>
    public virtual string? ExceptionStackTrace { get; set; }
}
