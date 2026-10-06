/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Controllers.DTOs;
using Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace API.Services;

/// <summary>
/// Service for the <see cref="Setting"/> model.
/// Used for validation and querying <see cref="Setting"/>
/// related data. Stores the active setting.
/// </summary>
/// <param name="context">Database context</param>
/// <param name="cache">IMemoryCache</param>
public class SettingsService(AppDbContext context, IMemoryCache cache)
{
    private const string ActiveSettingKey = "active-setting";

    public Setting ActiveSetting
    {
        get
        {
            if (!cache.TryGetValue(ActiveSettingKey, out Setting? cachedSetting))
            {
                var setting =
                    context.Settings
                        .AsNoTracking()
                        .OrderByDescending(s => s.LastActive)
                        .FirstOrDefault()
                    ??
                    throw new Exception("Cannot get active setting: the setting table is empty");

                cachedSetting = setting;

                cache.Set(ActiveSettingKey, cachedSetting, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                    SlidingExpiration = TimeSpan.FromHours(1)
                });
            }

            return cachedSetting!;
        }
        set
        {
            bool settingTracked = context.Settings.Contains(value);

            if (!settingTracked)
            {
                throw new Exception("This Setting object is not tracked");
            }

            value.LastActive = DateTime.UtcNow;
            context.SaveChanges();

            cache.Set(ActiveSettingKey, value);
        }
    }

    /// <summary>
    /// Update an existing <see cref="Setting"/> from a
    /// <see cref="Setting"/>.
    /// </summary>
    /// <param name="newSetting">The new Setting</param>
    /// <exception cref="Exception">Thrown if the to-be-updated setting cannot be found</exception>
    public async Task<Setting> UpdateFromSetting(Setting newSetting)
    {
        var setting =
            await context.Settings.FindAsync(newSetting.Id)
            ??
            throw new Exception("Setting not found");

        setting.ProfileName = newSetting.ProfileName;
        setting.TimeBeforeRedo = newSetting.TimeBeforeRedo;
        setting.LevelSize = newSetting.LevelSize;
        setting.AiFactor = newSetting.AiFactor;

        await context.SaveChangesAsync();

        // if the setting we're updating happens to be the
        // cached active setting, ensure it is updated too
        if (setting.Id == this.ActiveSetting.Id)
        {
            this.ActiveSetting = setting;
        }

        return setting;
    }

    public async Task AddSetting(Setting setting)
    {
        await context.Settings.AddAsync(setting);
        await context.SaveChangesAsync();
    }

    public Setting[] GetSettings(int offset, int range)
    {
        range = Math.Clamp(range, 1, 50);
        offset = Math.Max(0, offset);

        var page =
            context.Settings
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .Skip(offset)
                .Take(range)
                .ToArray();

        return page;
    }

    // ugly boilerplate as a result of adapting to backend refactor
    public async Task<Setting?> GetSetting(int id)
    {
        return await context.Settings.FindAsync(id);
    }

    /// <summary>
    /// Check whether a <see cref="SettingDto"/> is valid for conversion
    /// to a proper <see cref="Setting"/>.
    /// </summary>
    /// <param name="settingDto">The Setting DTO</param>
    /// <returns></returns>
    public bool Validate(SettingDto settingDto)
    {
        return
            settingDto.TimeBeforeRedo > TimeSpan.Zero &&
            settingDto.LevelSize > 0 &&
            settingDto.AiFactor >= 0 && settingDto.AiFactor <= 1;
    }
}
