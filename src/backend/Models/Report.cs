/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models;

/// <summary>
/// The Report Model: represents a single Item's report.
/// has a one-to-many relationship with <see cref="Models.Item"/>
/// </summary>
[Table(nameof(Report))]
[Index(nameof(ItemId), nameof(UserId), nameof(ItemError), IsUnique = true)]
public class Report
{
    public int Id { get; init; }
    public int ItemId { get; init; }
    public int UserId { get; init; }
    public Item Item { get; init; } = null!;
    public User User { get; init; } = null!;
    public ItemError ItemError { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Represents what can be wrong about an <see cref="Item"/>
/// </summary>
public enum ItemError
{
    QTextEmpty,
    QTextIncorrect,
    ATextEmpty,
    ATextIncorrect,
    AnswersWrong,
    GraphicsUnavailable,
    GraphicsFaulty,
    GraphicsUnrelated,
    OtherError
}

