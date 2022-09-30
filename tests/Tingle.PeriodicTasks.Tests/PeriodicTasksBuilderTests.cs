using Microsoft.Extensions.DependencyInjection;

namespace Tingle.PeriodicTasks.Tests;

public class PeriodicTasksBuilderTests
{
    [Theory]
    [InlineData("RecordCleanupJob", "RecordCleanup")]
    [InlineData("RecordCleanupTask", "RecordCleanup")]
    [InlineData("RecordCleanupJobTask", "RecordCleanup")]
    [InlineData("RecordCleanup", "RecordCleanup")] // unchanged
    public void TrimCommonSuffixes_Works(string typeName, string expected)
    {
        var options = new PeriodicTasksBuilder(new ServiceCollection());
        var actual = options.TrimCommonSuffixes(typeName, true);
        Assert.Equal(expected, actual);
    }
}
