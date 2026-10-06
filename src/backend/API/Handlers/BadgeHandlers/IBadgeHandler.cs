/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers.GameEventHandlers;
using Models;

namespace API.Handlers.BadgeHandlers;

public interface IBadgeHandler
{
    /// <summary>
    /// The identifier of the badge,
    /// basically answering the question, is this badge handler qualified to calculate the progress
    /// </summary>
    public string BadgeIdentifier();
    
    /// <summary>
    /// Calculate the new progress of the badge based on the current progress and the event data.
    /// See <see cref="GameEventData"/>
    /// </summary>
    public int CalculateNewProgress(GameEventData data, BadgeProgress currentProgress);
}