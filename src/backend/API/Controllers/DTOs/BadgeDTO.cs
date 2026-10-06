/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace API.Controllers.DTOs;

public class UserBadgeFlatResult
{
    public int? Progress { get; set; }
    public int? ProgressNeeded { get; set; }
    public string? Name { get; set; }
    public string? Identifier { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? StampImage { get; set; }
    public string? FlagKey { get; set; }
    public string? FlagVariant { get; set; }
    public DateTime? DateAccomplished { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public int? Page { get; set; }
}

public class UserBadgeDTO
{
    public int? Progress { get; set; } = 0;
    public int? ProgressNeeded { get; set; }
    public string? Name { get; set; }
    public string Identifier { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public StampDTO? Stamp { get; set; }
    public string? StampImage { get; set; }
    
    public static List<UserBadgeDTO> FromFlatBadge(IEnumerable<UserBadgeFlatResult> result)
    {
        return result.Select(r => new UserBadgeDTO
        {
            Progress = r.Progress,
            ProgressNeeded = r.ProgressNeeded,
            Name = r.Name,
            Identifier = r.Identifier,
            Description = r.Description,
            Category = r.Category,
            StampImage = r.StampImage,
            Stamp = r.DateAccomplished == null
                ? null
                : new StampDTO
                {
                    DateAccomplished = r.DateAccomplished,
                    X = (int)r.X!,
                    Y = (int)r.Y!,
                    Page = (int)r.Page!
                }
        }).ToList();
    }
}

public class StampDTO
{
    public DateTime? DateAccomplished { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Page { get; set; }
}