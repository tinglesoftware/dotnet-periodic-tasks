namespace Tingle.PeriodicTasks.AspNetCore;

/// <summary>
/// The data transfer object for the response typically found in "/registrations" and "/registrations/{name}" responses.
/// </summary>
public sealed record PeriodicTaskRegistration
{
    /// <summary>Name of the periodic task.</summary>
    public required string Name { get; init; }

    /// <summary>Full name of the periodic task type.</summary>
    public required string? Type { get; init; }

    /// <summary>Description of the periodic task.</summary>
    public string? Description { get; init; }

    /// <summary>Whether the periodic task is enabled.</summary>
    public bool Enable { get; init; }

    /// <summary>Whether the periodic task should be executed on startup.</summary>
    public bool ExecuteOnStartup { get; init; }

    /// <summary>Schedule for the periodic task.</summary>
    public string? Schedule { get; init; }

    /// <summary>Timezone for the periodic task.</summary>
    public string? Timezone { get; init; }

    /// <summary>Lock timeout for the periodic task.</summary>
    public TimeSpan? LockTimeout { get; init; }

    /// <summary>Whether to await execution to complete.</summary>
    public bool AwaitExecution { get; init; }

    /// <summary>Deadline for the periodic task.</summary>
    public TimeSpan? Deadline { get; init; }

    /// <summary>Execution ID format for the periodic task.</summary>
    public PeriodicTaskIdFormat? ExecutionIdFormat { get; init; }

    /// <summary>Lock name for the periodic task.</summary>
    public string? LockName { get; init; }
}
