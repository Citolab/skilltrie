/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Models;

/// <summary>
/// The <c>Roles</c> class: contains possible values for roles within the <see cref="Microsoft.AspNetCore.Identity"/> system.
/// Identity uses strings for this, which is why this is not an enum.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}

/// <summary>
/// This <c>ApplicationRole</c> class is an extension on <see cref="IdentityRole{TKey}"/> using <c>int</c> as Id type instead of the default <c>string</c>.
/// Additional fields can be added to this if needed, but this is primarily used to enforce the int primary key type for Ids.
/// </summary>
[Table("AspNetRoles")]
public class ApplicationRole : IdentityRole<int>
{

}
