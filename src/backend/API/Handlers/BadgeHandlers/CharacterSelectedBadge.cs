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

public class CharacterSelectedBadge : BadgeImage
{
    public override string BadgeIdentifier()
    {
        return nameof(CharacterSelectedBadge);
    }

    /// <summary>
    /// Returns 1 (badge achieved), as <see cref="GameEvent.CharacterSelected"/> being triggered means this badge has been achieved.
    /// </summary>
    public override int CalculateNewProgress(GameEventData data, BadgeProgress currentProgress)
    {
        return data is CharacterSelectedData ? 1 : currentProgress.Progress;
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.CharacterSelected];
    }

    protected override string? Name()
    {
        return "Customizer!";
    }

    protected override string? Description()
    {
        return "Change your character in order to achieve this badge";
    }

    protected override string? Stamp()
    {
        return "cosmetics stamp";
    }
}