/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace AA.ItemDisableAlgorithm;

/// <summary>
/// An algorithm to assess whether an item is still valid to display during a test.
/// </summary>
public interface IItemValidityAlgorithm
{
    /// <param name="itemId">the id of the item to asses</param>
    /// <returns>true if the item is still valid according to the algorithm, otherwise false</returns>
    public Task<bool> AssesItemValidity(int itemId);
}