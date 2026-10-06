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

public class FakeRandom : BadgeImage
{
    [BadgeParameter(EntryColumn = "Amount")]
    public int ItemAmount;

    [BadgeParameter(EntryColumn = "StreakCount")]
    public int StreakCount;
    
    public override string BadgeIdentifier()
    {
        return "fake 2_" + ItemAmount + "_" + StreakCount;
    }

    public override int CalculateNewProgress(GameEventData data, BadgeProgress currentProgress)
    {
        if (currentProgress.Progress == 0) currentProgress.Progress = 2;
        return (int)Math.Pow(currentProgress.Progress, ((FakeItemCorrectData)data).Correct ? 2d : 1d);
    }

    public override int ProgressNeeded()
    {
        return ItemAmount * StreakCount;
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return new List<GameEvent> { GameEvent.ItemAnswered, GameEvent.StreakProlonged};
    }
}