/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers.GameEventHandlers;
using API.Tools;
using API.Tools.Badges;
using API.Tools.Badges.BadgeImageCollecting;
using Models;

namespace API.Handlers.BadgeHandlers;

public class TestDoneBeforeBadge : BadgeImage
{
    [BadgeParameter(EntryColumn = "TimeBorder")]
    public TimeSpan BeforeTime;
    
    public override string BadgeIdentifier()
    {
        return $"{nameof(TestDoneBeforeBadge)}-{BeforeTime.Hours:D2}:{BeforeTime.Minutes:D2}:{BeforeTime.Seconds:D2}";
    }

    /// <summary>
    /// Sets to 1 (badge achieved) if it is right now earlier than the BeforeTime parameter, otherwise 0 (not achieved)
    /// </summary>
    public override int CalculateNewProgress(GameEventData data, BadgeProgress currentProgress)
    {
        return data.FiredAt.TimeOfDay < BeforeTime ? 1 : 0;
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.TestCompleted];
    }
    
    protected override string? Name()
    {
        return "Early bird";
    }

    protected override string? Description()
    {
        return $"Do a test before {BeforeTime.Hours:D2}:{BeforeTime.Minutes:D2}:{BeforeTime.Seconds:D2} in order to achieve this badge";
    }

    protected override string? Stamp()
    {
        return "early bird stamp";
    }
}