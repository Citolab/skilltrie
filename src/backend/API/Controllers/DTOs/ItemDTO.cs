/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.ComponentModel.DataAnnotations;
using Models;

namespace API.Controllers.DTOs;

/// <summary>
/// Full Data Transfer Object for <see cref="Item"/>.
/// simplifies Topics property as simply an array of strings.
/// </summary>
public record ItemDto
{
    public required int Id { get; init; }
    public required bool Active { get; set; } = true;
    public required Item.ItemType Type { get; set; }
    public required Item.ItemSource Source { get; set; }
    public required Item.Language Lang { get; set; }
    public required string? AnswerExplanation { get; set; } = null;
    public required string QuestionText { get; set; } = null!;
    public required int AppearanceCount { get; set; }

    [MaxLength(255)]
    public required string ResponseType { get; set; } = null!;

    [MaxLength(255)]
    public required string? Level { get; set; }

    public required ICollection<Scope> Scopes { get; set; } = [];
    public required ICollection<ItemAnswer> Answers { get; set; } = [];
    public required ICollection<Report> Reports { get; set; } = [];
    /// <summary>
    /// Create an ItemDto from an Item model
    /// </summary>
    /// <param name="item">The Item model to create a dto from</param>
    /// <returns>An ItemDto</returns>
    public static ItemDto CreateItemDto(Item item)
    {
        return new ItemDto()
        {
            Id = item.Id,
            Active = item.Active,
            Type = item.Type,
            Source = item.Source,
            Lang = item.Lang,
            AnswerExplanation = item.AnswerExplanation,
            QuestionText = item.QuestionText,
            AppearanceCount = item.AppearanceCount,
            ResponseType = item.ResponseType,
            Level = item.Level,
            Scopes = item.Scopes,
            Answers = item.Answers,
            Reports = item.Reports,
        };
    }
    /// <summary>
    /// Turn an ItemDto back into an Item model
    /// </summary>
    /// <param name="itemDto">The ItemDto to create the model from</param>
    /// <returns>The item model</returns>
    public static Item FromItemDto(ItemDto itemDto)
    {
        return new Item()
        {
            Id = itemDto.Id,
            Active = itemDto.Active,
            Type = itemDto.Type,
            Source = itemDto.Source,
            Lang = itemDto.Lang,
            AnswerExplanation = itemDto.AnswerExplanation,
            QuestionText = itemDto.QuestionText,
            ResponseType = itemDto.ResponseType,
            Level = itemDto.Level,
            Scopes = itemDto.Scopes,
            Answers = itemDto.Answers,
            Reports = itemDto.Reports,
        };
    }
}

/// <summary>
/// minimized Data Transfer Object for <see cref="Item"/>.
/// Omits answers, questionbody and correct answer explanation
/// </summary>
public class SmallItemDto
{
    public required int Id { get; set; }
    public required bool Active { get; set; }
    public required Item.ItemType Type { get; set; }
    public required Item.ItemSource Source { get; set; }
    public required Item.Language Lang { get; set; }
    public required int AppearanceCount { get; set; }
    public required string QuestionText { get; set; } = null!;

    [Length(1, 255)]
    public required string? ResponseType { get; set; }

    [Length(1, 255)]
    public required string? Level { get; set; }

    public required ICollection<Scope> Topics { get; set; } = [];

    public static SmallItemDto CreateSmallItemDto(Item item)
    {
        return new SmallItemDto()
        {
            Id = item.Id,
            Active = item.Active,
            Type = item.Type,
            Source = item.Source,
            Lang = item.Lang,
            AppearanceCount = item.AppearanceCount,
            QuestionText = item.QuestionText,
            ResponseType = item.ResponseType,
            Level = item.Level,
            Topics = item.Scopes
        };
    }
}
