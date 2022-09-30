namespace Tingle.PeriodicTasks;

///
public interface IPeriodicTaskIdGenerator
{
    /// <summary>
    /// Generate a value for <see cref="PeriodicTaskExecutionAttempt.Id"/>.
    /// </summary>
    /// <param name="name">The name of the task.</param>
    /// <param name="format">The format of the identifier.</param>
    /// <returns></returns>
    string Generate(string name, PeriodicTaskIdFormat format);
}
