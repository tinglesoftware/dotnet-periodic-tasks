using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tingle.PeriodicTasks;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for <see cref="CronSchedule"/>
/// </summary>
public class CronScheduleJsonConverter : JsonConverter<CronSchedule>
{
    /// <inheritdoc/>
    public override CronSchedule Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return default;
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new InvalidOperationException("Only strings are supported");
        }

        var value = reader.GetString()!;
        return CronSchedule.Parse(value);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CronSchedule value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
