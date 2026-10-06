/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.NewItemsAlgorithm;

/// <summary>
/// Contract for the algorithms that define when new items are needed, and how many.
/// </summary>
public interface INewItemsAlgorithm
{
    /// <summary>
    /// Calculates the number of items for each type that should be generated in the next batch
    /// </summary>
    /// <param name="topicId"></param>
    /// <param name="userId"></param>
    /// <returns><see cref="NewItemsRecord"></see></returns>
    Task<NewItemsRecord> NewItemBatch(int topicId, int userId);
}

/// <summary>
/// Record containing how many items of each type should be generated
/// </summary>
public record NewItemsRecord
{
    public required int TopicId { get; set; }
    public required int UserId { get; set; }
    public int TotalNewItems { get; set; } = 0;
    public int OpenEnded { get; set; } = 0;
    public int MultipleChoice { get; set; } = 0;
    public int Numerical { get; set; } = 0;
}