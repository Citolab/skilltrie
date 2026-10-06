/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.QualityAlgorithm;

/// <summary>
/// The contract for algorithms that calculate the quality of an item
/// </summary>
public interface IQualityAlgorithm
{
    /// <summary>
    /// A calculation that updates the quality of an item based on recent history
    /// </summary>
    /// <param name="itemId">The item whose quality should be updated</param>
    /// <returns></returns>
    Task UpdateQuality(int itemId); 

    /// <summary>
    /// A calculation that gives from all context of an item the quality score of the item. 
    /// Does not update the score inside of the database
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns>A decimal score for the quality of the item</returns>
    Task<decimal> CalculateQuality(int itemId); 
}