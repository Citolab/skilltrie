/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

global using BadgeParameterInfo = (System.Reflection.FieldInfo field, API.Tools.Badges.BadgeImageCollecting.BadgeParameterAttribute attribute);

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Models;

namespace API.Tools.Badges.BadgeImageCollecting;

public static class BadgeImageTypeHelper
{
    /// <summary>
    /// Gets all the <see cref="BadgeParameterAttribute"/> attributes of the badges along with its <see cref="FieldInfo"/>.
    /// This represents all the required parameters of a badge.
    /// </summary>
    public static List<BadgeParameterInfo> GetParameters(this Type badgeImageType)
    {
        if (!badgeImageType.IsSubclassOf(typeof(BadgeImage))) return new ();
        return badgeImageType
            .GetFields()
            .Select(f => (field: f, attribute: f.GetCustomAttribute<BadgeParameterAttribute>()))
            .Where(f => f.attribute != null).ToList()!;
    }
    
    /// <summary>
    /// Loads all the parameterized entries from the database
    /// which have column "BadgeImage" set to the class name of this badgeImageType.
    /// </summary>
    public static IEnumerable<ParameterizedBadgeEntry> LoadParameterizedEntriesFrom(this Type badgeImageType, AppDbContext context)
    {
        return context.ParameterizedBadgeEntries
            .Include(e => e.Topic)
            .Where(entry => entry.BadgeImage == badgeImageType.Name);
    }

    /// <summary>
    /// Whether <see cref="GetParameters"/> returns more than zero parameters. If not the badge is not parameterized.
    /// </summary>
    public static bool IsParameterized(this Type badgeImageType)
    {
        return badgeImageType.GetParameters().ToList().Count != 0;
    }
    
    /// <summary>
    /// Collect all badge image classes which are available and returns them as a list of <see cref="Type">Types</see>
    /// </summary>
    public static IEnumerable<Type> GetAllBadgeImageTypes(this Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BadgeImage)) && t.IsClass && !t.IsAbstract);   
    }
}