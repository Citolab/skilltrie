/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace IntegrationTests;

public abstract class AbstractAsyncLifetime : IAsyncLifetime
{
    protected TestWebApplicationFactory _factory = null!;

    /// <summary>
    /// Creates a new factory for each test
    /// </summary>
    public virtual Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the factory after completing a test
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
}