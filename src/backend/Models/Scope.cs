/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Models;

public enum ScopeType
{
    Topic, 
    Domain,
    Subject,
}


[Table(nameof(Scope))]
public class Scope
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required] 
    public ScopeType Type { get; set; }

    // Navigation properties
    [IgnoreDataMember]
    public ICollection<ScopeMembership> Ancestors { get; set; } = [];
    [IgnoreDataMember]
    public ICollection<ScopeMembership> Descendants { get; set; } = [];
    [IgnoreDataMember]
    public ICollection<ScopeEdge> FromScopes { get; set; } = [];
    [IgnoreDataMember]
    public ICollection<ScopeEdge> ToScopes { get; set; } = [];

    [IgnoreDataMember]
    public ICollection<Item> Items { get; } = [];
}


[Table(nameof(ScopeMembership))]
[Index(nameof(AncestorId))]
[Index(nameof(DescendantId))]
public class ScopeMembership
{
    public int AncestorId { get; set; }
    public int DescendantId { get; set; }
    public int Depth { get; set; } = 0;


    [IgnoreDataMember]
    [ForeignKey(nameof(AncestorId))]
    public Scope Ancestor { get; set; } = null!;

    [IgnoreDataMember]
    [ForeignKey(nameof(DescendantId))]
    public Scope Descendant { get; set; } = null!;
}


[Table(nameof(ScopeEdge))]
[Index(nameof(FromScopeId))]
[Index(nameof(ToScopeId))]
public class ScopeEdge
{
    public int FromScopeId { get; set; }
    public int ToScopeId { get; set; }

    [Range(0, 1)]
    [Column(TypeName = "numeric(7,6)")]
    public decimal Weight { get; set; } = 1m;


    [IgnoreDataMember]
    [ForeignKey(nameof(FromScopeId))]
    public Scope FromScope { get; set; } = null!;

    [IgnoreDataMember]
    [ForeignKey(nameof(ToScopeId))]
    public Scope ToScope { get; set; } = null!;
}

[Table(nameof(UserScopeProgress))]
public class UserScopeProgress
{
    public int UserId { get; set; }
    public int ScopeId { get; set; }

    [Range(0, 1)]
    [Column(TypeName = "numeric(7,6)")]
    public decimal Proficiency { get; set; } = 0m;

    public bool Mastered { get; set; } = false;

    [IgnoreDataMember]
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [IgnoreDataMember]
    [ForeignKey(nameof(ScopeId))]
    public Scope Scope { get; set; } = null!;

    public int GreenBalls { get; set; }
    public int RedBalls { get; set; }
}
