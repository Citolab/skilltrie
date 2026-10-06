/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace API.Controllers.DTOs;

public class ScopeDTO
{
    public int ScopeId { get; set; }
    public string ScopeName { get; set; } = "";
    public int AncestorId { get; set; }
    public string AncestorName { get; set; } = "";
}

public class UserTopicInfoDTO : ScopeDTO
{
    public decimal? Proficiency { get; set; }
    public bool? Mastered { get; set; }
    public bool Available { get; set; }
}

public class ScopeDependencyDTO
{
    public int From { get; set; }
    public int To { get; set; }
    public decimal Weight { get; set; }
}
