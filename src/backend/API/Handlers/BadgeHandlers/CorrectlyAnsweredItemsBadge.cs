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

public class CorrectlyAnsweredItemsBadge : BadgeImage
{
    [BadgeParameter(EntryColumn = "Amount")]
    public int ItemAmount;

    public override string BadgeIdentifier()
    {
        return ItemAmount + nameof(CorrectlyAnsweredItemsBadge);
    }

    /// <summary>
    /// Adds one progress to the old progress if the item answered by the user was correct.
    /// </summary>
    public override int CalculateNewProgress(GameEventData gameEventData, BadgeProgress currentProgress)
    {
        ItemAnsweredData itemAnsweredData = (ItemAnsweredData) gameEventData;
        return currentProgress.Progress + (itemAnsweredData.Data.Correct ? 1 : 0);
    }

    /// <summary>
    /// The badge has been achieved if the user has answered ItemAmount questions correctly.
    /// </summary>
    /// <returns></returns>
    public override int ProgressNeeded()
    {
        return ItemAmount;
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.ItemAnswered];
    }
    
    protected override string? Name()
    {
        return "Item smasher";
    }

    protected override string? Description()
    {
        return $"Answer {ItemAmount} questions correctly in order to achieve this badge";
    }
}