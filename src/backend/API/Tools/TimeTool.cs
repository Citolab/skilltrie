/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
namespace API.Tools;

/// <summary>
/// Tool that handles different functions that use Time. This way every component handles time the same way.
/// </summary>
/// <param name="tz">Timezone used by the components. Make sure it's in IANA format. Defaults to Europe/Amsterdam.</param>
/// <param name="timeProvider">Time provider used to get time of the system used.</param>
public class TimeTool(
    ITimeProvider timeProvider,
    TimeZoneInfo? tz = null
    )
{
    private readonly TimeZoneInfo _tz =
        tz ?? TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam"); // Default cannot be set as a parameter value - TimeZoneInfo.FindSystemTimeZoneById is not a compile-time constant.

    /// <summary>
    /// Gives todays date in the tools timezone.
    /// </summary>
    /// <returns>A DateTime with the date of today in the timezone given to the TimeTool (defaulted to AMS). Time will always be 00:00:00.
    /// DateTime is specified to UTC to avoid issues with time zone conversions and comparisons, even though date is based on the given timezone. Besides, the database currently only accepts UTC.</returns>
    public DateTime Today()
    {
        return DateTime.SpecifyKind(
                    TimeZoneInfo.ConvertTimeFromUtc(timeProvider.UtcNow, _tz).Date,
                    DateTimeKind.Utc
                );
    }

    /// <summary>
    /// Gives date and time right now in the tools timezone.
    /// </summary>
    /// <returns>A DateTime with the date and time of right now in the timezone given to the TimeTool (defaulted to AMS).
    /// DateTime is specified to UTC to avoid issues with time zone conversions and comparisons, even though date is based on the given timezone. Besides, the database currently only accepts UTC.</returns>
    public DateTime Now()
    {
        return DateTime.SpecifyKind(
                    TimeZoneInfo.ConvertTimeFromUtc(timeProvider.UtcNow, _tz),
                    DateTimeKind.Utc
                );
    }
}
