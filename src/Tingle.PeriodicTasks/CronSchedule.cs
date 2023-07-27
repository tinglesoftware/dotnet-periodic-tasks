using Cronos;
using System.ComponentModel;

namespace Tingle.PeriodicTasks;

/// <summary>Represents a CRON schedule.</summary>
[TypeConverter(typeof(CronScheduleTypeConverter))]
public readonly struct CronSchedule : IEquatable<CronSchedule>, IConvertible
{
    ///
    public static readonly CronSchedule Yearly = new("0 0 1 1 *");
    ///
    public static readonly CronSchedule Weekly = new("0 0 * * 0");
    ///
    public static readonly CronSchedule Monthly = new("0 0 1 * *");
    ///
    public static readonly CronSchedule Daily = new("0 0 * * *");
    ///
    public static readonly CronSchedule Hourly = new("0 * * * *");

    ///
    public static CronSchedule RandomHourly() => new($"{Random.Shared.Next(0, 59)} * * * *");

    private readonly CronExpression expression;

    /// <summary>Creates an instance of <see cref="CronSchedule"/>.</summary>
    /// <param name="value">The CRON schedule e.g. <c>0 3 * * *</c></param>
    public CronSchedule(string value) : this(ParseExpression(value)) { }

    internal CronSchedule(CronExpression expression)
    {
        this.expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    /// <summary>Calculates next occurrence starting after <paramref name="from"/> in the given <paramref name="zone"/>.</summary>
    /// <param name="from"></param>
    /// <param name="zone"></param>
    /// <returns></returns>
    public DateTimeOffset? GetNextOccurrence(DateTimeOffset from, TimeZoneInfo zone) => expression.GetNextOccurrence(from, zone);

    private static CronExpression ParseExpression(string value)
    {
        var spaces = value.Count(c => c == ' ');
        return CronExpression.Parse(value, spaces == 5 ? CronFormat.IncludeSeconds : CronFormat.Standard);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CronSchedule schedule && Equals(schedule);

    /// <inheritdoc/>
    public bool Equals(CronSchedule other) => expression == other.expression;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(expression);

    /// <inheritdoc/>
    public override string ToString() => expression.ToString();

    /// <inheritdoc/>
    public static bool operator ==(CronSchedule left, CronSchedule right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(CronSchedule left, CronSchedule right) => !(left == right);

    /// <summary>Converts a <see cref="CronSchedule"/> to a <see cref="CronExpression"/>.</summary>
    public static implicit operator CronExpression(CronSchedule s) => s.expression;

    /// <summary>Converts a <see cref="CronSchedule"/> to a <see cref="string"/>.</summary>
    public static implicit operator string(CronSchedule schedule) => schedule.expression.ToString();

    /// <summary>Converts a <see cref="string"/> to a <see cref="CronSchedule"/>.</summary>
    public static implicit operator CronSchedule(string expression) => new(expression);

    /// <summary>Converts a <see cref="CronExpression"/> to a <see cref="CronSchedule"/>.</summary>
    public static implicit operator CronSchedule(CronExpression expression) => new(expression);

    /// <summary>Converts a <see cref="TimeOnly"/> to a <see cref="CronSchedule"/>.</summary>
    public static implicit operator CronSchedule(TimeOnly time) => new($"{(time.Second > 0 ? time.Second : "*")} {time.Minute} {time.Hour} * * *");

    #region IConvertible

    TypeCode IConvertible.GetTypeCode() => TypeCode.Object;
    bool IConvertible.ToBoolean(IFormatProvider? provider) => throw new InvalidCastException();
    byte IConvertible.ToByte(IFormatProvider? provider) => throw new InvalidCastException();
    char IConvertible.ToChar(IFormatProvider? provider) => throw new InvalidCastException();
    DateTime IConvertible.ToDateTime(IFormatProvider? provider) => throw new InvalidCastException();
    decimal IConvertible.ToDecimal(IFormatProvider? provider) => throw new InvalidCastException();
    double IConvertible.ToDouble(IFormatProvider? provider) => throw new InvalidCastException();
    short IConvertible.ToInt16(IFormatProvider? provider) => throw new InvalidCastException();
    int IConvertible.ToInt32(IFormatProvider? provider) => throw new InvalidCastException();
    long IConvertible.ToInt64(IFormatProvider? provider) => throw new InvalidCastException();
    sbyte IConvertible.ToSByte(IFormatProvider? provider) => throw new InvalidCastException();
    float IConvertible.ToSingle(IFormatProvider? provider) => throw new InvalidCastException();
    string IConvertible.ToString(IFormatProvider? provider) => ToString();

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
    {
        return Type.GetTypeCode(conversionType) switch
        {
            TypeCode.Object when conversionType == typeof(object) => this,
            TypeCode.Object when conversionType == typeof(CronSchedule) => this,
            TypeCode.String => ((IConvertible)this).ToString(provider),
            _ => throw new InvalidCastException(),
        };
    }

    ushort IConvertible.ToUInt16(IFormatProvider? provider) => throw new InvalidCastException();
    uint IConvertible.ToUInt32(IFormatProvider? provider) => throw new InvalidCastException();
    ulong IConvertible.ToUInt64(IFormatProvider? provider) => throw new InvalidCastException();

    #endregion
}
