/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.UserMasteryAlgorithm;

/// <summary>
/// Contract for the algorithms that define how user masteries are updated / given out
/// </summary>
public interface IUserMasteryAlgorithm
{
    /// <summary>
    /// A function that checks the mastery of a given user, using data from the database. 
    /// </summary>
    /// <param name="userId">User whose mastery to update</param>
    /// <param name="topicId">Topic to check if it is mastered</param>
    /// <param name="testId">The most recently made test, which is queried for the results</param>
    /// <returns></returns>
    Task<bool> CalculateMastery(int userId, int topicId, int testId);
}