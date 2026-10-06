/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace AA.UrningsAlgorithm;

/// <summary>
/// An implementation that does literally nothing and returns nothing with which nothing is done
/// </summary>
[Algorithm("Null UA")]
public class NullUA : IUrningsAlgorithm
{
    public Task UpdateUrnings(int userId, int itemId, bool actuallyGreen)
    {
        return Task.CompletedTask; // Does nothing safely
    }
}