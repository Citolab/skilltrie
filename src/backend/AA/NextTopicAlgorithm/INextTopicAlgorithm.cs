/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.NextTopicAlgorithm;
/// <summary>
/// Contract for the algorithms that defines what topic the user can view next.
/// </summary>
public interface INextTopicAlgorithm
{
    /// <summary>
    /// A task that returns the recommended topic for a <see cref="User"/>.
    /// </summary>
    /// <param name="userId">The user for whom to return the available topics</param>
    /// <returns>The recommended topic for the user</returns>
    Task<Scope?> RecommendedTopic(int userId);


    /// <summary>
    /// A task that returns a list of topics that the user has unlocked
    /// </summary>
    /// <param name="userId">The user for whom to return the available topics</param>
    /// <returns>A list of topics that the user may access</returns>
    Task<List<Scope>> UnlockedTopics(int userId);

    /// <summary>
    /// A task that checks whether a given topic is unlocked by a given user
    /// </summary>
    /// <param name="userId">The user for whom to check</param>
    /// <param name="topicId">The topic to check</param>
    /// <returns> <c>true</c> if the topic is unlocked; otherwise, <c>false</c>.</returns>
    Task<bool> TopicIsUnlocked(int userId, int topicId);
}
