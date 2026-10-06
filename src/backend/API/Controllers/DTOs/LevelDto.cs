/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Controllers.DTOs;

public record UserAnswerRequestDto
{
    public int ItemId { get; set; }
    public int LevelId { get; set; }
    public string? Answer { get; set; }
    public string? AnswerIdentifier { get; set; }
    public bool Correct { get; set; }
    public string? CompletionStatus { get; set; }

    /// <summary>
    /// Create UserAnswerRequestDto from a model
    /// </summary>
    /// <param name="userAnswerRequest">The model to create the Dto from</param>
    /// <returns>The Dto with the data from the model</returns>
    public static UserAnswerRequestDto CreateDto(UserAnswerRequest userAnswerRequest)
    {
        return new UserAnswerRequestDto()
        {
            ItemId = userAnswerRequest.ItemId,
            LevelId = userAnswerRequest.LevelId,
            Answer = userAnswerRequest.Answer,
            AnswerIdentifier = userAnswerRequest.AnswerIdentifier,
            Correct = userAnswerRequest.Correct,
            CompletionStatus = userAnswerRequest.CompletionStatus
        };
    }

    /// <summary>
    /// Turn a Dto back into a model
    /// </summary>
    /// <param name="dto">The Dto who's data is to be put into the model</param>
    /// <returns>A model with the data from the Dto</returns>
    public static UserAnswerRequest FromDto(UserAnswerRequestDto dto)
    {
        return new UserAnswerRequest()
        {
            ItemId = dto.ItemId,
            LevelId = dto.LevelId,
            Answer = dto.Answer,
            AnswerIdentifier = dto.AnswerIdentifier,
            Correct = dto.Correct,
            CompletionStatus = dto.CompletionStatus
        };
    }
}

public record LevelResponseDto
{
    public string AssessmentXml { get; set; } = string.Empty;
    public int LevelId { get; set; }

    // it's a lot nicer for the frontend to already know which item ids are
    // going to be needed, rather than extract these ids from the AssessmentXML
    // or qti-player which is kind of a backwards practice.
    public int[] ItemIds { get; set; } = [];

    /// <summary>
    /// Create a Dto from a model
    /// </summary>
    /// <param name="levelResponse">The model to create a Dto from</param>
    /// <returns>The Dto with the given model's data</returns>
    public static LevelResponseDto CreateDto(LevelResponse levelResponse)
    {
        return new LevelResponseDto() { AssessmentXml = levelResponse.AssessmentXml, LevelId = levelResponse.LevelId, };
    }

    /// <summary>
    /// Create a model with a Dto
    /// </summary>
    /// <param name="dto">Dto with data to be put into the model</param>
    /// <returns>The model with the Dto's data</returns>
    public static LevelResponse FromDto(LevelResponseDto dto)
    {
        return new LevelResponse() { AssessmentXml = dto.AssessmentXml, LevelId = dto.LevelId, };
    }
}

public record LevelAnswerRequestDto
{
    public int LevelResultId { get; set; }
    public List<UserAnswerRequestDto> Answers { get; set; } = new();

    /// <summary>
    /// Create a Dto with a model
    /// </summary>
    /// <param name="levelAnswerRequest">The models to create a Dto from</param>
    /// <returns>A Dto with the given model's data</returns>
    public static LevelAnswerRequestDto CreateDto(LevelAnswerRequest levelAnswerRequest)
    {
        return new LevelAnswerRequestDto()
        {
            LevelResultId = levelAnswerRequest.LevelResultId,
            Answers = levelAnswerRequest.Answers.Select(x => UserAnswerRequestDto.CreateDto(x)).ToList(),
        };
    }

    /// <summary>
    /// Create a model with a Dto
    /// </summary>
    /// <param name="dto">Dto who's data is to be put into a model</param>
    /// <returns>The model with the Dto's data</returns>
    public static LevelAnswerRequest FromDto(LevelAnswerRequestDto dto)
    {
        return new LevelAnswerRequest()
        {
            LevelResultId = dto.LevelResultId,
            Answers = dto.Answers.Select(dto => UserAnswerRequestDto.FromDto(dto)).ToList()
        };
    }
}
