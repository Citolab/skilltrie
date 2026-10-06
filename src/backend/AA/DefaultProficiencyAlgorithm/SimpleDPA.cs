/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */


using Microsoft.EntityFrameworkCore;
using Models;

namespace AA.DefaultProficiencyAlgorithm;

/// <summary>
/// The simplest implementation for the Default proficiciency algorithm
/// Just returns a random number for the proficiency. 
/// </summary>
/// <param name="context">The database context, inserted via DI</param>
[Algorithm("Simple DPA")]
public class SimpleDPA(AppDbContext context) : IDefaultProficiencyAlgorithm
{
    /// <summary>
    /// Calculation helper function, compute for each topic some random decimal.
    /// </summary>
    /// <param name="userId">The userId for whom to create defaults</param>
    /// <param name="topicIds">A list of topics for which to create a default</param>
    /// <returns>A list of the new user proficiencies</returns>
    private List<UserScopeProgress> _calculate(int userId, int[] topicIds)
    {
        List<UserScopeProgress> proficiencies = new();

        Random r = new Random(userId);

        for (int d = 0; d < topicIds.Count(); d++)
        {
            UserScopeProgress prof = new UserScopeProgress()
            {
                UserId = userId,
                ScopeId = topicIds[d],
                Proficiency = decimal.Round(r.Next(500, 1001) / 1000m, 3)
            };

            proficiencies.Add(prof);
        }
        return proficiencies;
    }

    /// <summary>
    /// Method to find topics to assign default values, create defaults and then add them to the database.
    /// </summary>
    /// <param name="userId">The user for whom to create defaults</param>
    /// <returns></returns>
    /// <exception cref="Exception">When no user is found, throw an exception</exception>
    public async Task CreateDefaultProficiencies(int userId)
    {
        if (!await context.Users.AnyAsync(u => u.Id == userId))
            throw new Exception($"User with Id '{userId}' not found.");

        var topicIds = context.Scopes.Select(t => t.Id).ToArray();

        List<Models.UserScopeProgress> proficiencies = _calculate(userId, topicIds);

        context.UserScopeProgress.AddRange(proficiencies);

        await context.SaveChangesAsync();
    }
}