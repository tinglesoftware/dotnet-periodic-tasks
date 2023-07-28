namespace Tingle.PeriodicTasks.Tests;

public class CronScheduleTests
{
    [Theory]
    [InlineData("14:10:54", "54 10 14 * * *")]
    [InlineData("14:10", "* 10 14 * * *")]
    [InlineData("14:00:54", "54 0 14 * * *")]
    public void CanConvertFromTimeOnly(string input, string expected)
    {
        var to = TimeOnly.Parse(input);
        var actual = ((CronSchedule)to).ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RandomHourly_Works()
    {
        var values = Enumerable.Range(0, 10).Select(_ => CronSchedule.RandomHourly()).ToList();

        Assert.All(values, exp =>
        {
            // Ensure format is matched first zero is millisecond
            Assert.Matches(@"^0 [0-9]{1,2} \* \* \* \*$", exp);

            // Ensure the next occurrence is less than 2 hours away
            var now = DateTimeOffset.UtcNow;
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Etc/UTC");
            var next = exp.GetNextOccurrence(now, tz);
            Assert.NotNull(next);
            Assert.InRange((next!.Value - now).TotalHours, 0.0f, 2.0f);
        });
    }

    [Fact]
    public void TestCompareEqual()
    {
        var id1 = new CronSchedule("0 0 * * * *");
        var id2 = id1;
        Assert.False(id1 != id2);
        Assert.True(id1 == id2);

        id2 = new CronSchedule((string)id1);
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void TestIConvertibleMethods()
    {
        object value = CronSchedule.Hourly;
        Assert.Equal(TypeCode.Object, ((IConvertible)value).GetTypeCode());
        Assert.Equal(value, ((IConvertible)value).ToType(typeof(object), null)); // not AreSame because of boxing
        Assert.Equal(value, ((IConvertible)value).ToType(typeof(CronSchedule), null)); // not AreSame because of boxing
        Assert.Throws<InvalidCastException>(() => Convert.ToBoolean(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToByte(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToChar(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToDateTime(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToDecimal(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToDouble(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToInt16(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToInt32(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToInt64(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToSByte(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToSingle(value));
        Assert.Equal("0 0 * * * *", Convert.ToString(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToUInt16(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToUInt32(value));
        Assert.Throws<InvalidCastException>(() => Convert.ToUInt64(value));

        Assert.Equal("0 0 * * * *", ((IConvertible)value).ToType(typeof(string), null));
        Assert.Throws<InvalidCastException>(() => ((IConvertible)value).ToType(typeof(ulong), null));
    }
}
