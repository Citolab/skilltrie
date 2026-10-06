/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.QualityAlgorithm;

/// <summary>
/// The simples implementation of the new items algorithm.
/// </summary>
/// <param name="context">The database context</param>
[Algorithm("Simple QA")]
public class SimpleQA(AppDbContext context) : IQualityAlgorithm
{
    public Task UpdateQuality(int itemId)
    {
        throw new NotImplementedException();
    }

    public Task<decimal> CalculateQuality(int itemId)
    {
        throw new NotImplementedException(); 
    }
}