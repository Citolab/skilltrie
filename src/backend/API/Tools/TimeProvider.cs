/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace API.Tools;

/// <summary>
/// Time Provider that provides the current time in UTC, need to inject this for testability.
/// </summary>
public interface ITimeProvider
{
    DateTime UtcNow { get; }
}

/// <summary>
/// An implementation of time provider that returns DateTime.UtcNow.
/// </summary>
public class SystemTimeProvider : ITimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
