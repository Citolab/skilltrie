/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Models;

[Table(nameof(Group))]
public class Group
{
    public int Id { get; set; }

    [Length(1, 255)]
    public string Name { get; set; } = null!;

    // Navigation property: one group has many members
    [IgnoreDataMember]
    public ICollection<GroupMember> Members { get; set; } = [];
}

[Table(nameof(GroupMember))]
[Index(nameof(UserId))]
[Index(nameof(GroupId))]
public class GroupMember
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [IgnoreDataMember]
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public int GroupId { get; set; }

    [IgnoreDataMember]
    [ForeignKey(nameof(GroupId))]
    public Group Group { get; set; } = null!;
}
