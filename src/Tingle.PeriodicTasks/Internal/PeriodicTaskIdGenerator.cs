namespace Tingle.PeriodicTasks.Internal;

internal class PeriodicTaskIdGenerator : IPeriodicTaskIdGenerator
{
    /// <inheritdoc/>
    public string Generate(string name, PeriodicTaskIdFormat format)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var id = Guid.NewGuid();
        var bytes = id.ToByteArray();

        var id_str = format switch
        {
            PeriodicTaskIdFormat.Guid => id.ToString(),
            PeriodicTaskIdFormat.GuidNoDashes => id.ToString("n"),
            PeriodicTaskIdFormat.Long => BitConverter.ToUInt64(bytes, 0).ToString(),
            PeriodicTaskIdFormat.LongHex => BitConverter.ToUInt64(bytes, 0).ToString("x"),
            PeriodicTaskIdFormat.DoubleLong => $"{BitConverter.ToUInt64(bytes, 0)}{BitConverter.ToUInt64(bytes, 8)}",
            PeriodicTaskIdFormat.DoubleLongHex => $"{BitConverter.ToUInt64(bytes, 0):x}{BitConverter.ToUInt64(bytes, 8):x}",
            PeriodicTaskIdFormat.Random => Convert.ToBase64String(bytes),
            _ => throw new NotSupportedException($"'{nameof(PeriodicTaskIdFormat)}.{format}' is not supported."),
        };

        return $"{name}|{id_str}";
    }
}
