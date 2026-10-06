/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.UserProficiencyAlgorithm;

/// <summary>
/// Contract for the algorithms that define how user proficiencies are updated / given out
/// </summary>
public interface IUserProficiencyAlgorithm
{
    /// <summary>
    /// Helper method that calculates what the new proficiency should be
    /// </summary>
    /// <param name="proficiency">The current proficiency that we wish to update</param>
    /// <returns>A decimal value of the new proficiency</returns>
    decimal CalculateNewProficiency(decimal proficiency);

    /// <summary>
    /// Method to update the proficiency of a user
    /// </summary>
    /// <param name="userId">The user whose proficiency to update</param>
    /// <param name="topicId">The topic for which proficiency is to be updated</param>
    /// <returns></returns>
    Task UpdateProficiency(int userId, int topicId);
}

