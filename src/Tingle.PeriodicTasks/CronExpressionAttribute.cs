namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// Specifies that a data field value is a valid CRON expression.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class CronExpressionAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CronExpressionAttribute"/> class.
    /// </summary>
    public CronExpressionAttribute() : base("The field {0} must be a valid CRON Expression.") { }

    /// <inheritdoc/>
    public override bool IsValid(object? value)
    {
        if (value is not string s || string.IsNullOrEmpty(s)) return true;

        return Tingle.PeriodicTasks.CronSchedule.TryParse(value: s, out _);
    }
}
