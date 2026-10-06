/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Handlers;

public class ReportHandler
{
    /// <summary>
    /// Create a new Report for a Dto
    /// </summary>
    /// <param name="itemId">The id of the item to give a report from</param>
    /// <param name="error">The Error of the report</param>
    /// <param name="userId">The id of the user who made the report</param>
    /// <returns>A <see cref="Report"/> model with the given input values</returns>
    public Report CreateReportForDb(int itemId, ItemError error, int userId)
    {
        return new Report()
        {
            ItemId = itemId,
            ItemError = error,
            UserId = userId
        };
    }
}
