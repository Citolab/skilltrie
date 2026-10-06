/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Models;

/// <summary>
/// The User Model: inherits from <c>IdentityUser&lt;int&gt;</c> and expands the default user table with fields for
/// first name, infix and last name.
/// </summary>
public class User : IdentityUser<int>
{
    [Length(1, 255)] public string FirstName { get; set; } = null!;
    [Length(1, 255)] public string? Infix { get; set; }
    [Length(1, 255)] public string LastName { get; set; } = null!;

    [Length(1, 255)] public string DisplayName { get; set; } = null!;

    public DateTimeOffset? LastLoggedIn { get; set; }

    /* ### Navigation Properties ### */
    public ICollection<GroupMember> UserGroupMembers = [];
    public ICollection<UserCurrency> UserCurrencies = [];
    public ICollection<UserCosmetic> UserCosmetics = [];
}

public record UserCreateRecord
{
    public string Role { get; set; } = Roles.User;
    public string FirstName { get; set; } = null!;
    public string? Infix { get; set; }
    public string LastName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
}

[Table(nameof(UserStreak))]
public class UserStreak
{
    public int UserId { get; set; }

    public DateTime LastDayCompleted { get; set; }

    public int CurrentStreak { get; set; }
    public int HighestStreak { get; set; }

    [IgnoreDataMember]
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}

[Table(nameof(UserTour))]
public class UserTour
{
    public int UserId { get; set; }
    public string TourKey { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}