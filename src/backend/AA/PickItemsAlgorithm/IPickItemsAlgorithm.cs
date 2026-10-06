/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.PickItemsAlgorithm;


/// <summary>
/// An interface for a pick item algorithm which takes as input a list of items (for example, available items for a user)
/// and returns the "best" subset of those items
/// </summary>
public interface IPickItemsAlgorithm
{
    public List<Item> PickItems(ICollection<Item> availableItems);
}