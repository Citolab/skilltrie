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

public class WelcomeBadge : BadgeImage
{
    public override string BadgeIdentifier()
    {
        return nameof(WelcomeBadge);
    }

    /// <summary>
    /// Returns 1 (badge achieved), as <see cref="GameEvent.UserRegistered"/> being triggered means this badge has been achieved.
    /// </summary>
    public override int CalculateNewProgress(GameEventData data, BadgeProgress currentProgress)
    {
        if (data is UserRegisteredData) return 1;
        else return currentProgress.Progress;
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.UserRegistered];
    }

    protected override string? Name()
    {
        return "Welcome!";
    }

    protected override string? Description()
    {
        return "Create an account in order to achieve this badge";
    }

    protected override string? Stamp()
    {
        return "welcome stamp";
    }
}