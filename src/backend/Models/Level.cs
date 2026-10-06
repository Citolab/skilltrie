/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace Models;

/// <summary>
/// A LevelResult is what keeps track of a users progress/answers on a level.
/// Essentially just a list of UserAnswers tied to a list
/// </summary>
public class LevelResult
{
    public int Id { get; init; }
    public int UserId { get; set; }
    public int TopicId { get; set; }
    public User User { get; init; } = null!;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /* ### Navigation Properties ### */

    public ICollection<UserAnswer> UserAnswer { get; init; } = null!;

}

/// <summary>
/// An answer given on an item belonging to a specific level(result)
/// </summary>
public class UserAnswer
{
    public int Id { get; init; }
    public int ItemId { get; init; }
    public Item Item { get; set; } = null!;
    public int LevelResultId { get; init; }
    public LevelResult LevelResult { get; init; } = null!;
    public string? Answer { get; set; }
    public bool Correct { get; set; }
    public string? CompletionStatus { get; set; } = "open"; // could be enum
    public string? AnswerIdentifier { get; set; }
    public DateTime AnswerDate { get; init; } = DateTime.UtcNow;
}

public class UserAnswerRequest
{
    public int ItemId { get; set; }
    public int LevelId { get; set; }
    public string? Answer { get; set; }
    public string? AnswerIdentifier { get; set; }
    public bool Correct { get; set; }
    public string? CompletionStatus { get; set; }
}

public class LevelResponse
{
    public string AssessmentXml { get; set; } = string.Empty;
    public int LevelId { get; set; }
}

public class LevelAnswerRequest
{
    public int LevelResultId { get; set; }
    public List<UserAnswerRequest> Answers { get; set; } = new();
}
