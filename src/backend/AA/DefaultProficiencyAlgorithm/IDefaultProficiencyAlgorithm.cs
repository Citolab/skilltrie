/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace AA.DefaultProficiencyAlgorithm;

/// <summary>
/// Contract for the algorithms that define default proficiencies for users
/// </summary>
/// <remarks>
/// This interface may change if we implement an introduction test
/// </remarks> 
public interface IDefaultProficiencyAlgorithm
{
    /// <summary>
    /// Function that adds the default proficiencies of a selected user to the database. 
    /// </summary>
    /// <param name="userId">The id of the user for which we add proficiencies</param>
    /// <returns></returns>
    Task CreateDefaultProficiencies(int userId); 
}

