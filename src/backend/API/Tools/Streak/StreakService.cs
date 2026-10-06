/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace API.Tools.Streak;

/// <summary>
/// Service responsible for calling streak reset at midnight.
/// </summary>
/// <param name="serviceProvider">The service provider used to create scopes for database access.</param>
/// <param name="timeTool">Tool that handles time. Default implementation is injected.</param>
public class StreakService(
    IServiceProvider serviceProvider,
    TimeTool timeTool
    ) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Calculates next midnight in the Amsterdam timezone and waits until then to call a streak reset.
    /// </summary>
    /// <param name="stoppingToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = timeTool.Now(); // We actually use time here for the delay, so use now.
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;

            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var resetService = scope.ServiceProvider.GetRequiredService<IStreakResetService>();
                await resetService.ResetStreaks();
            }
        }
    }
}
