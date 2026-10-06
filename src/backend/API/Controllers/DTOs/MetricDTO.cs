/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Runtime.InteropServices;
using Models;

namespace API.Controllers.DTOs;

/// <summary>
/// Data Transfer Object for the metrics used by the researcher dashboard
/// </summary>
public record MetricDTO
{
    public int StudentCount { get; init; }
    public int ActiveUserCount { get; init; }
    public List<DailyCountDTO> TestsTakenPastWeek { get; init; }
    public int FlaggedItemCount { get; init; }
}

public record DailyCountDTO
{
    public DateTime Date { get; init; }
    public int Count { get; init; }
}