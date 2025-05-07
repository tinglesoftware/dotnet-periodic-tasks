using System.Text.Json.Serialization;

namespace Tingle.PeriodicTasks.Tests;

[JsonSerializable(typeof(CronScheduleTests.TestModel), TypeInfoPropertyName = "CronScheduleTests_TestModel")]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class TestJsonSerializerContext : JsonSerializerContext { }
