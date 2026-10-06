/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
using Microsoft.EntityFrameworkCore;
using Models;

namespace API.Tools.Streak
{
    public interface IStreakResetService
    {
        Task ResetStreaks();
    }

    /// <summary>
    /// Service responsible for resetting user streaks.
    /// </summary>
    /// <param name="context">The database context used to access user streaks.</param>
    public class StreakResetService(
        AppDbContext context,
        TimeTool timeTool
        ) : IStreakResetService
    {
        /// <summary>
        /// Resets the current streak count for users whose streaks have lapsed, updating the database accordingly.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ResetStreaks()
        {
            try
            {
                var today = timeTool.Today();

                var streaks = await context.UserStreaks
                    .Where(s => s.CurrentStreak > 0)
                    .ToListAsync();

                streaks.ForEach(streak =>
                {
                    if ((today - streak.LastDayCompleted.Date).TotalDays > 1)
                        streak.CurrentStreak = 0;
                });

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during streak resetting: {ex}");
            }
        }
    }
}
