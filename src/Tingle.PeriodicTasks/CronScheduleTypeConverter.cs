using System.ComponentModel;
using System.Globalization;

namespace Tingle.PeriodicTasks;

/// <summary>
/// Provides a type converter to convert <see cref="CronSchedule"/> objects to and from <see cref="string"/> objects.
/// </summary>
internal class CronScheduleTypeConverter : TypeConverter
{
    /// <inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);

    /// <inheritdoc/>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string);

    /// <inheritdoc/>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value is string s ? new CronSchedule(s) : base.ConvertFrom(context, culture, value);
    }

    /// <inheritdoc/>
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        return destinationType == typeof(string) && value is CronSchedule cs
            ? cs.ToString()
            : base.ConvertTo(context, culture, value, destinationType);
    }
}
