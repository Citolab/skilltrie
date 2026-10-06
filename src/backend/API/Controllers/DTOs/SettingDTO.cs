/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Controllers.DTOs;

public class SettingDto
{
    public int Id { get; init; }
    public string ProfileName { get; set; } = null!;
    public TimeSpan TimeBeforeRedo { get; set; }
    public DateTime LastActive { get; set; }
    public int LevelSize { get; set; }
    public double AiFactor { get; set; }

    /// <summary>
    /// Create a new <see cref="Setting"/> entity from a <see cref="SettingDto"/>.
    /// </summary>
    /// <param name="settingDto">The Setting DTO</param>
    /// <returns>A new <see cref="Setting"/> object.</returns>
    public static Setting FromDto(SettingDto settingDto)
    {
        Setting setting = new()
        {
            Id = settingDto.Id,
            ProfileName = settingDto.ProfileName,
            TimeBeforeRedo = settingDto.TimeBeforeRedo,
            LevelSize = settingDto.LevelSize,
            AiFactor = settingDto.AiFactor
        };

        return setting;
    }

    /// <summary>
    /// Convert a <see cref="Setting"/> entity
    /// to a <see cref="SettingDto"/>.
    /// </summary>
    /// <param name="setting">The <see cref="Setting"/> entity</param>
    /// <returns>The Setting DTO</returns>
    public static SettingDto CreateDto(Setting setting)
    {
        return new()
        {
            Id = setting.Id,
            TimeBeforeRedo = setting.TimeBeforeRedo,
            LastActive = setting.LastActive,
            ProfileName = setting.ProfileName,
            LevelSize = setting.LevelSize,
            AiFactor = setting.AiFactor
        };
    }
}
