using Tingle.PeriodicTasks.Internal;

namespace Tingle.PeriodicTasks.Tests;

public class PeriodicTaskIdGeneratorTests
{
    [Theory]
    [InlineData(PeriodicTaskIdFormat.Guid)]
    [InlineData(PeriodicTaskIdFormat.GuidNoDashes)]
    [InlineData(PeriodicTaskIdFormat.Long)]
    [InlineData(PeriodicTaskIdFormat.LongHex)]
    [InlineData(PeriodicTaskIdFormat.DoubleLong)]
    [InlineData(PeriodicTaskIdFormat.DoubleLongHex)]
    [InlineData(PeriodicTaskIdFormat.Random)]
    public void Generate_Works(PeriodicTaskIdFormat format)
    {
        var generator = new PeriodicTaskIdGenerator();
        var id = generator.Generate("cake", format);
        Assert.NotNull(id);
    }

    internal class SampleEvent
    {
        public string? Value1 { get; set; }
        public string? Value2 { get; set; }
    }
}
