/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers.GameEventHandlers;
using API.Tools.Badges;
using API.Tools.Badges.BadgeImageCollecting;
using Models;

namespace API.Handlers.BadgeHandlers;

public class StreakStartedBadge : BadgeImage
{
    public override string BadgeIdentifier()
    {
        return nameof(StreakStartedBadge);
    }

    /// <summary>
    /// Sets to 1 (badge achieved) if there is a streak otherwise 0 (badge achieved)
    /// </summary>
    public override int CalculateNewProgress(GameEventData gameEventData, BadgeProgress currentProgress)
    {
        StreakProlongedData streakProlongedData = (StreakProlongedData) gameEventData;
        return streakProlongedData.Streak.CurrentStreak >= 1 ? 1 : 0;
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.StreakProlonged];
    }
    
    protected override string? Name()
    {
        return "Streak Start";
    }

    protected override string? Description()
    {
        return $"Start a streak in order to achieve this badge";
    }

    protected override string? Stamp()
    {
        return "streak started stamp";
    }
}