/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers.GameEventHandlers;
using API.Tools.Badges;
using Models;

namespace API.Handlers.BadgeHandlers;

public class TestFailedBadge : BadgeImage
{
    public override string BadgeIdentifier()
    {
        return nameof(TestFailedBadge);
    }

    /// <summary>
    /// Sets to 1 (badge achieved) if the user answered all questions wrong in a test (regardless of topic), otherwise 0 (not achieved)
    /// </summary>
    public override int CalculateNewProgress(GameEventData gameEventData, BadgeProgress currentProgress)
    {
        TestCompletedData testCompletedData = (TestCompletedData) gameEventData;
        return testCompletedData.Level.UserAnswer.All(a => !a.Correct && !string.IsNullOrEmpty(a.Answer)) ? 1 : 0;
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.TestCompleted];
    }

    protected override string? Stamp()
    {
        return "test failed stamp.svg";
    }

    protected override string? Name()
    {
        return "Epic fail";
    }

    protected override string? Description()
    {
        return "Fail all items in a test in order to achieve this badge";
    }
}