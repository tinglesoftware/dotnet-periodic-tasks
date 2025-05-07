using System.ComponentModel.DataAnnotations;

namespace Tingle.PeriodicTasks.Tests;

public class CronExpressionAttributeTests
{
    [Theory]
    [InlineData("0 2 * * 1-5", true)]
    [InlineData("14 10 2 * * 1-5", true)] // with seconds
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("A", false)]
    public void CronExpression_Validation_Works(string? testValue, bool expected)
    {
        var obj = new TestModel(testValue);
        var context = new ValidationContext(obj);
        var results = new List<ValidationResult>();
        var actual = Validator.TryValidateObject(obj, context, results, true);
        Assert.Equal(expected, actual);

        // if expected it to pass, the results should be empty
        if (expected) Assert.Empty(results);
        else
        {
            var val = Assert.Single(results);
            var memeberName = Assert.Single(val.MemberNames);
            Assert.Equal(nameof(TestModel.SomeValue), memeberName);
            Assert.NotNull(val.ErrorMessage);
            Assert.NotEmpty(val.ErrorMessage);
            Assert.EndsWith("must be a valid CRON Expression.", val.ErrorMessage);
        }
    }

    record TestModel([property: CronExpression] string? SomeValue);
}
