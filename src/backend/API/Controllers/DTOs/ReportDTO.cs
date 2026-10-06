/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Controllers.DTOs;

/// <summary>
/// Data Transfer Object for <see cref="Report"/>
/// </summary>
public record ReportDto
{
    public required int ItemId { get; init; }
    public required string ItemError { get; init; }
}
