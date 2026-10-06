/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers.GameEventHandlers;
using API.Tools.Badges;
using Models;

namespace APITests.Tools.Badges.FakeBadgeImages;

public class FakeWithoutParams2 : BadgeImage
{
    public override string BadgeIdentifier()
    {
        return "fake 4";
    }

    public override int CalculateNewProgress(GameEventData data, BadgeProgress currentProgress)
    {
        throw new NotImplementedException();
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return new List<GameEvent>();
    }
}