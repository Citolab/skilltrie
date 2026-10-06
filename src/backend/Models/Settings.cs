/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Models;

[Index(nameof(ProfileName), IsUnique = true)]
public class Setting
{
    public int Id { get; init; }

    [Length(1, 255)]
    public string ProfileName { get; set; } = null!;
    public TimeSpan TimeBeforeRedo { get; set; } = TimeSpan.FromHours(1);
    public DateTime LastActive { get; set; } = DateTime.MinValue; // last time this setting was active
    public int LevelSize { get; set; } = 10; // default number of questions per level
    public double AiFactor { get; set; } = 0.2d; // percentage of test questions that are 'unstable'
}
