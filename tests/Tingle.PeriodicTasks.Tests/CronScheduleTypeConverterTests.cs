using System.ComponentModel;

namespace Tingle.PeriodicTasks.Tests;

public class CronScheduleTypeConverterTests
{
    [Fact]
    public void ConvertsToString()
    {
        var converter = TypeDescriptor.GetConverter(typeof(CronSchedule));
        Assert.NotNull(converter);
        var expected = "0 10 14 * * *";
        var csb = new CronSchedule(expected);
        var actual = converter.ConvertToString(csb);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("10 14 * * *", "0 10 14 * * *")]
    [InlineData("54 0 14 * * *", "54 0 14 * * *")]
    public void ConvertsFromString(string input, string expected)
    {
        var converter = TypeDescriptor.GetConverter(typeof(CronSchedule));
        Assert.NotNull(converter);
        var actual = Assert.IsType<CronSchedule>(converter.ConvertFromString(input));
        Assert.Equal(new CronSchedule(expected), actual);
    }
}
