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
using Newtonsoft.Json;

namespace API.Handlers.BadgeHandlers;

public class BackToBackFailBadge : BadgeImage
{
    [BadgeParameter(EntryColumn = "Amount")] 
    public int FailureAmount;
    
    public override string BadgeIdentifier()
    {
        return FailureAmount + nameof(BackToBackFailBadge);
    }

    public override int CalculateNewProgress(GameEventData gameEventData, BadgeProgress currentProgress)
    {
        TestCompletedData testCompletedData = (TestCompletedData) gameEventData;
        
        // If anything goes wrong with parsing or registry is still null, reinitialize dict.
        Dictionary<int, int> topicsFailed = (currentProgress.Registry != null ? JsonConvert
            .DeserializeObject<Dictionary<int, int>>(currentProgress.Registry) : new Dictionary<int, int>()) ?? new Dictionary<int, int>();

        int topicId = testCompletedData.Level.TopicId;
        if (!topicsFailed.TryGetValue(topicId, out int failureTimes)) 
            topicsFailed.Add(topicId, 0);
        
        topicsFailed[topicId] = testCompletedData.Mastered ? 0 : failureTimes + 1;
        currentProgress.Registry = JsonConvert.SerializeObject(topicsFailed);

        return Math.Max(currentProgress.Progress, topicsFailed[topicId]);
    }

    public override int ProgressNeeded()
    {
        return FailureAmount;
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.TestCompleted];
    }

    protected override string Name()
    {
        return "Back To Back Failure";
    }

    protected override string? Description()
    {
        return $"Fail a topic {FailureAmount} times in a row in order to achieve this badge.";
    }

    protected override string? Stamp()
    {
        return "failed stamp";
    }
}