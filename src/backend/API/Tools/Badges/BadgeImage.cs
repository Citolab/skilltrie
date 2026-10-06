/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Text.Json;
using API.Handlers.BadgeHandlers;
using API.Handlers.GameEventHandlers;
using API.Tools.Badges.Compilation;
using Models;

namespace API.Tools.Badges;

using BadgeImageMetadata = Dictionary<string, object>;

/// <summary>
/// A class combining both the functionalities of <see cref="IBadgeHandler"/> and <see cref="IBadgeCompile"/>.
/// A class inheriting from this represents the concept of the badge. It includes both the properties of a badge which
/// are stored in the database and the at runtime handling of the progression of the badge
/// </summary>
/// <author>Armand Ayar</author>
public abstract class BadgeImage : IBadgeHandler, IBadgeCompile
{
    public ParameterizedBadgeEntry? CreatedFrom { get; set; }
    
    /// <summary>
    /// Creates a badge which is identified by <see cref="BadgeIdentifier"/>
    /// This where the utility of the interface combination comes in action.
    /// </summary>
    public Badge Compile()
    {
        return new Badge
        {
            Identifier        = this.BadgeIdentifier(),
            Name              = FromMetadataOrDefault("Name", Name()),
            Description       = FromMetadataOrDefault("Description", Description()),
            ProgressNeeded    = ProgressNeeded(),
            TriggerConditions = TriggerConditions(),
            Category          = FromMetadataOrDefault("Category", Category()),
            Stamp             = FromMetadataOrDefault("Stamp", Stamp())
        };
    }
    
    /// <summary>
    /// Checks whether a property is in the metadata dictionary. Also checks whether the type of the value
    /// in metadata corresponds to the given type parameter (or wrapped in a JsonElement).
    /// If not it returns the defaultValue
    /// </summary>
    /// <param name="key">The key of the dictionary to obtain the value</param>
    /// <param name="defaultValue">The default value to return if the above-mentioned requirements are not met</param>
    /// <typeparam name="T">The required type of the metadata value. The type is also permitted to be wrapped
    /// in a <see cref="JsonElement"/></typeparam>
    /// <returns></returns>
    private T FromMetadataOrDefault<T>(string key, T defaultValue)
    {
        BadgeImageMetadata? metadata = CreatedFrom?.Metadata;
        object? value = metadata?.GetValueOrDefault(key);
        if (value == null) return defaultValue;
        
        if (value is JsonElement element)
        {
            try { return element.Deserialize<T>() ?? defaultValue; }
            catch { return defaultValue; }
        }
        
        if (value is T typedValue) return typedValue;

        return defaultValue;
    }
    
    /// <inheritdoc/>
    public abstract string BadgeIdentifier();

    /// <inheritdoc/>
    public abstract int CalculateNewProgress(GameEventData data, BadgeProgress currentProgress);

    /// <summary>
    /// The display name of the badge. Is used for <see cref="Compile"/>, if not available in the metadata.
    /// Can be overwritten in child classes (e.g. in order to use parameters in the name).
    /// </summary>
    protected virtual string? Name() => null;
    
    /// <summary>
    /// The description of the badge, also known as the requirement to achieve the badge.
    /// Is used for <see cref="Compile"/>, if not available in the metadata.
    /// Can be overwritten in child classes (e.g. in order to use parameters in the description).
    /// </summary>
    protected virtual string? Description() => null;
    
    /// <summary>
    /// The amount of progress units needed in order to achieve the badge.
    /// Is used for <see cref="Compile"/>.
    /// Can be overwritten in child classes. If not the return value is 1 which makes the badge act as a boolean
    /// (Achieved or not)
    /// </summary>
    public virtual int ProgressNeeded() => 1;
    
    /// <summary>
    /// The list of <see cref="GameEvent">GameEvents</see> from which this badge will be triggered
    /// </summary>
    protected abstract List<GameEvent> TriggerConditions();
    
    /// <summary>
    /// The category of the badge, can be null and is currently unused anywhere in the application,
    /// but it is a nice property for separation.
    /// Is used for <see cref="Compile"/>, if not available in the metadata.
    /// Can be overwritten in child classes.
    /// </summary>
    protected virtual string? Category() => null;

    /// <summary>
    /// The stamp of a badge is the image which can be displayed in a passport when the badge has been completed.
    /// It can also be the image associated with the badge in general.
    /// </summary>
    /// <returns></returns>
    protected virtual string? Stamp() => null;

    /// <summary>
    /// Checks whether two badge images are equal. Badge images are equal if they gave the same (inherited) type
    /// and if their <see cref="BadgeIdentifier"/> is equal. 
    /// </summary>
    public override bool Equals(object? other)
    {
        if (other?.GetType() != this.GetType()) return false;
        return ((BadgeImage)other).BadgeIdentifier() == BadgeIdentifier();
    }
}