﻿namespace Tingle.PeriodicTasks;

/// <summary>
/// Sets the description of a periodic task.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PeriodicTaskDescriptionAttribute : Attribute
{
    ///
    public PeriodicTaskDescriptionAttribute(string description)
    {
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    ///
    public string Description { get; }
}
