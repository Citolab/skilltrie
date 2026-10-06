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

public class TopicMasteredBadge : BadgeImage
{
    [BadgeParameter(EntryColumn = "Topic")]
    public Scope topic;

    public override string BadgeIdentifier()
    {
        return topic.Id + nameof(TopicMasteredBadge);
    }

    /// <summary>
    /// Checks whether the topic has been completed or not. If yes, set progress to 1 (badge achieved), otherwise 0 (not achieved).
    /// </summary>
    public override int CalculateNewProgress(GameEventData gameEventData, BadgeProgress currentProgress)
    {
        TestCompletedData testCompletedData = (TestCompletedData) gameEventData;
        if (testCompletedData.Level.TopicId != topic.Id) return currentProgress.Progress;
        return testCompletedData.Mastered ? 1 : 0;
    }

    public override int ProgressNeeded()
    {
        return 1;
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.TestCompleted];
    }
    
    protected override string? Name()
    {
        return $"{topic.Name} stamp";
    }

    protected override string? Description()
    {
        return $"Master the {topic.Name} topic in order to achieve this badge";
    }

    protected override string? Stamp()
    {
        return "mastery stamp";
    }
}