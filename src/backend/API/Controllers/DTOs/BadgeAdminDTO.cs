/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Controllers.DTOs;

public record ParameterDTO(string Name, string Type);

public class AddParameterizedBadgeDTO
{
    public required string BadgeImage { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Stamp { get; set; }

    public int? Amount { get; set; }
    public int? TopicId { get; set; }
    public int? StreakCount { get; set; }
    public TimeSpan? TimeBorder { get; set; }
    
    /// <summary>
    /// Creates a <see cref="ParameterizedBadgeEntry"/> from a <see cref="AddParameterizedBadgeDTO"/>
    /// </summary>
    public static ParameterizedBadgeEntry ConstructEntryFromDto(AddParameterizedBadgeDTO dto)
    {
        ParameterizedBadgeEntry entry = new ParameterizedBadgeEntry
        {
            BadgeImage = dto.BadgeImage,
            Amount = dto.Amount,
            StreakCount = dto.StreakCount,
            ScopeId = dto.TopicId,
            TimeBorder = dto.TimeBorder,
            Metadata = new Dictionary<string, object>()
        };
        if (dto.Name != null) entry.Metadata.Add("Name", dto.Name);
        if (dto.Description != null) entry.Metadata.Add("Description", dto.Description);
        if (dto.Category != null) entry.Metadata.Add("Category", dto.Category);
        if (dto.Stamp != null) entry.Metadata.Add("Stamp", dto.Stamp);

        return entry;
    }
}

public class BadgeInfoDTO
{
    public required string Identifier { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Stamp { get; set; }
    public DateTime? OpenFrom { get; set; }
    public DateTime? OpenUntil { get; set; }
    public string? FlagKey { get; set; }
    public string? FlagVariant { get; set; }
    public BadgePhase Phase { get; set; }

    public static BadgeInfoDTO FromBadge(Badge b)
    {
        return new BadgeInfoDTO
        {
            Identifier = b.Identifier,
            Name = b.Name,
            Description = b.Description,
            Category = b.Category,
            Stamp = b.Stamp,
            OpenFrom = b.BadgeState.OpenFrom,
            OpenUntil = b.BadgeState.OpenUntil,
            FlagKey = b.BadgeState.FlagKey,
            FlagVariant = b.BadgeState.FlagVariant,
            Phase = b.BadgeState.Phase
        };
    }
}

public class UpdateBadgeStateDTO
{
    public required string Identifier { get; set; }
    public DateTime? OpenFrom { get; set; }
    public DateTime? OpenUntil { get; set; }
    public string? FlagKey { get; set; }
    public string? FlagVariant { get; set; }
}