/*
  * This program has been developed by students from the bachelor Computer Science at
  * Utrecht University within the Software Project course.
  * © Copyright Utrecht University (Department of Information and Computing Sciences)
  * Licensed under the MIT License. See the LICENSE file in the project root for details.
  */

using Models;

namespace AA.DefaultMasteryAlgorithm;

/// <summary>
/// Contract for the algorithms that define default masteries for users
/// </summary>
public interface IDefaultMasteryAlgorithm
{
  /// <summary>
  /// Creates default masteries for the specified user.
  /// </summary>
  /// <param name="userId">The unique identifier of the user for whom default masteries will be created.</param>
  /// <returns></returns>
  Task CreateDefaultMasteries(int userId); 
}

