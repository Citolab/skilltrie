/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Models;

public enum GameEvent
{
    StreakProlonged,
    TestCompleted,
    ItemAnswered,
    UserRegistered,
    CharacterSelected,
    ItemReported
}

[Table(nameof(Badge))]
public class Badge
{
    [Key]
    public string Identifier { get; set; }
    
    public string? Name { get; set; }
    
    public string? Description { get; set; }
    
    public int ProgressNeeded { get; set; }

    [NotMapped]
    public List<GameEvent> TriggerConditions { get; set; } = [];
    
    public string? Category { get; set; }
    
    public string? Stamp { get; set; }

    [IgnoreDataMember]
    public BadgeState BadgeState { get; set; }
}

[Table(nameof(BadgeTriggerCondition))]
[PrimaryKey(nameof(BadgeIdentifier), nameof(TriggerCondition))]
public class BadgeTriggerCondition
{
    public string BadgeIdentifier { get; set; }
    
    public GameEvent TriggerCondition { get; set; }
    
    [IgnoreDataMember]
    [ForeignKey(nameof(BadgeIdentifier))]
    public Badge Badge { get; set; } = null!;
}

public enum BadgePhase
{
    Staging = 0,
    Disabled = 1,
    Published = 2,
}

[Table(nameof(BadgeState))]
public class BadgeState
{
    [Key]
    public string BadgeIdentifier { get; init; }
    
    public DateTime? OpenFrom { get; set; }
    
    public DateTime? OpenUntil { get; set; }
    
    public string? FlagKey { get; set; }
    
    public string? FlagVariant { get; set; }

    public BadgePhase Phase { get; set; } = BadgePhase.Staging;
    
    public bool Excluded { get; set; }
    
    [IgnoreDataMember]
    [ForeignKey(nameof(BadgeIdentifier))]
    public Badge Badge { get; set; } = null!;
}

[Table(nameof(ParameterizedBadgeEntry))]
public class ParameterizedBadgeEntry
{
    public int Id { get; set; }
    public string BadgeImage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = [];
    
    [Range(1, int.MaxValue)]
    public int? Amount { get; set; }
    public int? ScopeId { get; set; }
    public int? StreakCount { get; set; }
    [Column(TypeName = "time")]
    public TimeSpan? TimeBorder { get; set; }
    
    [IgnoreDataMember]
    [ForeignKey(nameof(ScopeId))]
    public Scope? Topic { get; set; } = null!;
}

[Table(nameof(BadgeProgress))]
[PrimaryKey(nameof(UserId), nameof(BadgeIdentifier))]
public class BadgeProgress
{
    public int UserId { get; set; }
    public string BadgeIdentifier { get; set; }
    public int Progress { get; set; }
    public int? StampId { get; set; }
    public string? Registry { get; set; }

    [IgnoreDataMember] 
    [ForeignKey(nameof(StampId))]
    public BadgeStamp? Stamp { get; set; }
    
    [IgnoreDataMember]
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    
    [IgnoreDataMember]
    [ForeignKey(nameof(BadgeIdentifier))]
    public Badge Badge { get; set; } = null!;
}

[Table(nameof(BadgeStamp))]
public class BadgeStamp
{
    public int Id { get; set; }
    public int Page { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public DateTime DateAccomplished { get; set; }
}
