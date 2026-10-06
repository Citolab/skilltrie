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
using Newtonsoft.Json;

namespace Models;

/// <summary>
/// The Item model. Represent a single QTI Item (question).
/// Stores all data necessary to convert it back to QTI XML format.
/// Also stores application specific data/statistics.
/// </summary>
[Table(nameof(Item))]
public class Item
{
    public enum ItemSource 
    { 
        LLM,
        Imported,
        Databank
    }

    public int Id { get; init; }

    /// <summary>
    /// The type of an Item: multiple choice, open, etc.
    /// </summary>
    public enum ItemType
    {
        MultipleChoice,
        Open,
        Numerical
    }

    /// <summary>
    /// The language of the question text and answers for an Item
    /// </summary>
    public enum Language
    {
        English,
        Dutch
    }

    public bool Active { get; set; } = true;
    public ItemType Type { get; set; }
    public ItemSource Source { get; set; }
    public Language Lang { get; set; }
    public string? AnswerExplanation { get; set; } // explanation of the correct answer
    public string QuestionText { get; set; } = null!;
    public int AppearanceCount { get; set; }

    [Length(1, 255)]
    public string ResponseType { get; set; } = null!; // e.g. "conceptual", "interpreting graph", "interpreting output", etc.

    [Length(1, 255)]
    public string? Level { get; set; } // interpret this as difficulty 

    /* ### Navigation Properties ### */

    [IgnoreDataMember]
    public ICollection<ScopeItem> ScopeItems { get; set; } = [];

    [IgnoreDataMember]
    public ICollection<Scope> Scopes { get; set; } = [];

    [IgnoreDataMember]
    public ICollection<ItemAnswer> Answers { get; set; } = [];

    [IgnoreDataMember]
    public ICollection<Report> Reports { get; set; } = [];
}

[Table(nameof(ItemUrn))]
public class ItemUrn
{
    public int ItemId { get; init; }
    public int GreenBalls { get; set; } // Item property for urnings algorithm
    public int RedBalls { get; set; } // Item property for urnings algorithm
    
    [IgnoreDataMember]
    [ForeignKey(nameof(ItemId))]
    public Item Item { get; set; } = null!;
    
}

/// <summary>
/// An answer, or multiple answers belonging to an item.
/// Has a many-to-one relationship with <see cref="Models.Item"/>
/// </summary>
[Table(nameof(ItemAnswer))]
public class ItemAnswer
{
    public int Id { get; init; }
    public int ItemId { get; init; } // foreign key
    [JsonProperty("answer_text")]
    public string? AnswerText { get; set; }
    public int Chosen { get; set; }
    [JsonProperty("correct")]
    public bool Correct { get; set; }

    [Length(1, 255)]
    public string? AnswerIdentifier { get; set; }

    /* ### Navigation Properties ### */

    [IgnoreDataMember]
    public Item Item { get; init; } = null!;
}


/// <summary>
/// Join table between <see cref="Models.Scope"/> and <see cref="Models.Item"/>.
/// </summary>
[Table(nameof(ScopeItem))]
[Index(nameof(ScopeId), nameof(ItemId), IsUnique = true)]
public class ScopeItem
{
    public int ItemId { get; set; }
    [IgnoreDataMember]
    [ForeignKey(nameof(ItemId))]
    public Item Item { get; set; } = null!;

    public int ScopeId { get; set; }
    [IgnoreDataMember]
    [ForeignKey(nameof(ScopeId))]
    public Scope Scope { get; set; } = null!;
}




