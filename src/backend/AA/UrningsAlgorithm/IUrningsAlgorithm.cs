/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace AA.UrningsAlgorithm;

/// <summary>
/// Default implementation for updating the urnings of items and players
/// </summary>
public interface IUrningsAlgorithm
{
    /// <summary>
    /// A calculation that returns the updates to the Urnings algorithm that need to be applied. 
    /// </summary>
    /// <param name="userId">The UserId whose urn we wish to change</param>
    /// <param name="itemId">The item whose urn we wish to change</param>
    /// <param name="actuallyGreen">If the user actually got the correct result or not. </param>
    /// <returns></returns>
    Task UpdateUrnings(int userId, int itemId, bool actuallyGreen);
}