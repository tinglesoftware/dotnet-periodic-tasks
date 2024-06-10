﻿using System.Diagnostics;
using System.Reflection;

namespace Tingle.PeriodicTasks.Diagnostics;

///
public static class PeriodicTasksActivitySource
{
    private static readonly AssemblyName AssemblyName = typeof(PeriodicTasksActivitySource).Assembly.GetName();
    private static readonly string ActivitySourceName = AssemblyName.Name!;
    private static readonly Version Version = AssemblyName.Version!;
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName, Version.ToString());

    /// <summary>
    /// Creates a new activity if there are active listeners for it, using the specified
    /// name, activity kind, and parent Id.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="kind"></param>
    /// <param name="parentId"></param>
    /// <returns></returns>
    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal, string? parentId = null)
    {
        return parentId is not null
            ? ActivitySource.StartActivity(name: name, kind: kind, parentId: parentId)
            : ActivitySource.StartActivity(name: name, kind: kind);
    }
}

/// <summary>
/// Names for activities generated by PeriodicTasks
/// </summary>
public static class ActivityNames
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string Execute = "Tingle.PeriodicTasks.Execute";
    public const string ExecuteAttempt = "Tingle.PeriodicTasks.ExecuteAttempt";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

/// <summary>
/// Names for tags added to activities generated by PeriodicTasks
/// </summary>
public static class ActivityTagNames
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string PeriodicTaskType = "periodictask.type";
    public const string PeriodicTaskName = "periodictask.name";
    public const string PeriodicTaskSchedule = "periodictask.schedule";
    public const string PeriodicTaskTimezone = "periodictask.timezone";
    public const string PeriodicTaskDeadline = "periodictask.deadline";
    public const string PeriodicTaskAttemptNumber = "periodictask.attempt_number";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}