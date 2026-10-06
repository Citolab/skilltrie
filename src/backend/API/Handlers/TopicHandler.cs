/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers.DTOs;
using Models;

namespace API.Handlers;

public class TopicHandler
{

    /// <summary>
    /// Get the default proficiencies to fill the user with some data, currently generates random data based on the user's Id.
    /// </summary>
    /// <param name="userId">The Id of the user whose data is to be generated</param>
    /// <param name="topicIds">The topics with which to fill</param>
    /// <returns>The calculated proficiencies</returns>
    public List<UserScopeProgress> GetDefaultProficiencies(int userId, int[] topicIds)
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
}
