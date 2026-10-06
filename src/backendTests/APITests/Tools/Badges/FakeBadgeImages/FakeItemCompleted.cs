/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers.GameEventHandlers;
using API.Tools.Badges;
using API.Tools.Badges.BadgeImageCollecting;
using APITests.Handlers.GameEventHandler;
using Models;

namespace APITests.Tools.Badges.FakeBadgeImages;

public class FakeItemCompleted : BadgeImage
{
    [BadgeParameter(EntryColumn = "Amount")]
    public int ItemAmount;
    
    public override string BadgeIdentifier()
    {
        return "fake 1_" + ItemAmount;
    }

    public override int CalculateNewProgress(GameEventData data, BadgeProgress currentProgress)
    {
        return currentProgress.Progress + (((FakeItemCorrectData)data).Correct ? 1 : 0);
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.ItemAnswered];
    }
}