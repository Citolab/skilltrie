/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Tools;

namespace IntegrationTests;

/// <summary>
/// Fake time provider to ensure the time can always be the same during tests
/// </summary>
/// <param name="utcNow"></param>
public class MockTimeProvider(DateTime utcNow) : ITimeProvider
{
    public DateTime UtcNow { get; } = utcNow;
}