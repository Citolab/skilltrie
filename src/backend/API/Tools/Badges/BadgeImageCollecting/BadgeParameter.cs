/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Tools.Badges.BadgeImageCollecting;

/// <summary>
/// An attribute representing the requirement of a badge parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class BadgeParameterAttribute : Attribute
{
    /// <summary>
    /// The column of the <see cref="ParameterizedBadgeEntry"/> which stores the value of this parameter.
    /// </summary>
    public required string EntryColumn;
}