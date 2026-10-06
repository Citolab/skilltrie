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

public class StreakProlongedBadge : BadgeImage
{
    [BadgeParameter(EntryColumn = "StreakCount")]
    public int StreakCount;
    
    public override string BadgeIdentifier()
    {
        return StreakCount + nameof(StreakProlongedBadge);
    }

    /// <summary>
    /// Updates the progress to the current streak of the user. If the progress was already higher, it keeps the
    /// progress the same, as there has been a time the user had a higher streak.
    /// </summary>
    public override int CalculateNewProgress(GameEventData gameEventData, BadgeProgress currentProgress)
    {
        StreakProlongedData streakProlongedData = (StreakProlongedData) gameEventData;
        return Math.Max(currentProgress.Progress, streakProlongedData.Streak.CurrentStreak);
    }

    public override int ProgressNeeded()
    {
        return StreakCount;
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.StreakProlonged];
    }
    
    protected override string? Name()
    {
        return "Fire";
    }

    protected override string? Description()
    {
        return $"Complete a streak of {StreakCount} days in order to achieve this badge";
    }

    protected override string? Stamp()
    {
        if (StreakCount == 10) return "10 day streak stamp";
        return base.Stamp();
    }
}