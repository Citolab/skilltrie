/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using Models;

namespace API.Controllers.DTOs;

/// <summary>
/// Defines the properties returned when requesting a <see cref="User"/> via API call.
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Role { get; set; } = Roles.User;
    public string FirstName { get; set; } = null!;
    public string? Infix { get; set; }
    public string LastName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;

    public static UserDto CreateUserDto(User user, string role)
    {
        return new UserDto
        {
            Id = user.Id,
            Role = role,
            FirstName = user.FirstName,
            Infix = user.Infix,
            LastName = user.LastName,
            DisplayName = user.DisplayName!,
            Email = user.Email!
        };
    }
}

/// <summary>
/// Defines the properties when users request a <see cref="User"/> via API call.
/// </summary>
public class PublicUserDto
{
    public int Id { get; set; }
    public string Role { get; set; } = Roles.User;
    public string DisplayName { get; set; } = null!;

    public static PublicUserDto CreatePublicUserDto(User user, string role)
    {
        return new PublicUserDto
        {
            Id = user.Id,
            Role = role,
            DisplayName = user.DisplayName!
        };
    }
}

/// <summary>
/// Defines the properties needed for creating or updating a <see cref="User"/> via API call
/// </summary>
public record UserCreateDto
{
    public string Role { get; set; } = Roles.User;
    [MaxLength(255)]
    public required string FirstName { get; set; } = null!;
    [MaxLength(255)]
    public string? Infix { get; set; }
    [MaxLength(255)]
    public required string LastName { get; set; } = null!;
    [MaxLength(255)]
    public required string DisplayName { get; set; } = null!;
    [MaxLength(255)]
    public string Password { get; set; } = null!;
    [MaxLength(255)]
    [EmailAddress]
    public required string Email { get; set; } = null!;

    public static UserCreateRecord CreateRecordFromDto(UserCreateDto udto)
    {
        if (new[] { udto.FirstName, udto.LastName, udto.DisplayName, udto.Email }.Any(string.IsNullOrWhiteSpace))
            throw new Exception("Empty credentials");

        return new UserCreateRecord()
        {
            Role = udto.Role,
            FirstName = udto.FirstName.Trim(),
            Infix = udto.Infix?.Trim(),
            LastName = udto.LastName.Trim(),
            DisplayName = udto.DisplayName.Trim(),
            Password = udto.Password,
            Email = udto.Email.Trim().ToLower()
        };
    }
}

/// <summary>
/// Small data transfer object of <see cref="User"/> for login, containing just email and password.
/// </summary>
public class LoginRequestDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

/// <summary>
/// Small data transfer object of <see cref="User"/> for login response, containing just email and id.
/// </summary>
public class AuthResponseDto
{
    public required string Email { get; set; }
    public required int Id { get; set; }
}

public class UserStreakDto
{
    public required int UserId { get; set; }

    public required DateTime LastDayCompleted { get; set; }

    public required int CurrentStreak { get; set; }
    public required int HighestStreak { get; set; }

    public static UserStreakDto CreateUserStreakDto(UserStreak streak)
    {
        return new UserStreakDto
        {
            UserId = streak.UserId,
            LastDayCompleted = streak.LastDayCompleted,
            CurrentStreak = streak.CurrentStreak,
            HighestStreak = streak.HighestStreak
        };
    }
}
