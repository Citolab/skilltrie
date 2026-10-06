/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Tools;

namespace APITests.Tools;

public class FakeTimeProvider(DateTime utcNow) : ITimeProvider
{
    public DateTime UtcNow { get; } = utcNow;
}

public class TimeToolTests
{
    private static readonly TimeZoneInfo amsterdam =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");

    private static readonly DateTime fakeUtcNow = new DateTime(2024, 6, 1, 10, 25, 42, DateTimeKind.Utc);

    [Fact]
    public void Today_ReturnsDateInConfiguredTimezone()
    {
        // UTC 23:30 on the 14th = 00:30 on the 15th in Amsterdam (UTC+1 in winter)
        var fakeUtc = new DateTime(2024, 3, 14, 23, 30, 0, DateTimeKind.Utc);
        var tool = new TimeTool(new FakeTimeProvider(fakeUtc), amsterdam);

        var result = tool.Today();

        Assert.Equal(new DateTime(2024, 3, 15), result); // date rolled over
    }

    [Fact]
    public void Today_TimeIsAlwaysMidnight()
    {
        var tool = new TimeTool(new FakeTimeProvider(fakeUtcNow), amsterdam);

        var result = tool.Today();

        Assert.Equal(TimeSpan.Zero, result.TimeOfDay);
    }

    [Fact]
    public void Today_KindIsUtc()
    {
        var tool = new TimeTool(new FakeTimeProvider(fakeUtcNow), amsterdam);

        Assert.Equal(DateTimeKind.Utc, tool.Today().Kind);
    }

    [Fact]
    public void Now_ReturnsTimeConvertedToConfiguredTimezone()
    {
        // UTC noon = 14:00 Amsterdam time in summer (UTC+2)
        var fakeUtc = new DateTime(2024, 7, 1, 12, 0, 0, DateTimeKind.Utc);
        var tool = new TimeTool(new FakeTimeProvider(fakeUtc), amsterdam);

        var result = tool.Now();

        Assert.Equal(new DateTime(2024, 7, 1, 14, 0, 0), result);
    }

    [Fact]
    public void Now_KindIsUtc()
    {
        var tool = new TimeTool(new FakeTimeProvider(fakeUtcNow), amsterdam);

        Assert.Equal(DateTimeKind.Utc, tool.Now().Kind);
    }

    [Fact]
    public void CustomTimezone_IsRespected()
    {
        var tokyo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo"); // UTC+9, no DST
        var fakeUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var tool = new TimeTool(new FakeTimeProvider(fakeUtc), tokyo);

        // UTC midnight = 09:00 in Tokyo, still Jan 1st
        Assert.Equal(new DateTime(2024, 1, 1, 9, 0, 0), tool.Now());
        Assert.Equal(new DateTime(2024, 1, 1), tool.Today());
    }
}