/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Reflection;
using AA.StampAlgorithm;
using Models;
using Xunit;

namespace AATests.StampAlgorithm;

public class SimpleStampAlgorithmTest
{
    private readonly AppDbContext _db;
    private SimpleStampAlgorithm _algorithm;
    private int _badgeCounter;

    public SimpleStampAlgorithmTest()
    {
        _db = SqliteInMemoryContextFactory.Create();
        _db.Users.Add(CreateUser());
        _db.SaveChanges();
        SetAlgorithmFields();
    }

    [Fact]
    public async Task FindNewStampPosition_NoStampsYetCreatesOnFirstPage()
    {
        BadgeStamp stamp = await _algorithm.FindNewStampPosition(userId: 1);
        Assert.Equal(0, stamp.Page);
    }
    
    [Fact]
    public async Task FindNewStampPosition_StampsNextPageWhenMaxStampsCrossed()
    {
        SetAlgorithmFields(stampWidth: 1); // ensure the limited with is not making the stamps go to the next page
        await AddStamps([
            (0, 4, 2),
            (0, 3, 1),
            (0, 4, 0),
            (0, 6, 0),
            (0, 3, 0),
        ]);
        BadgeStamp stamp = await _algorithm.FindNewStampPosition(userId: 1);
        Assert.Equal(1, stamp.Page);
    }
    
    [Fact]
    public async Task FindNewStampPosition_ShouldGoToNextPageWhenNoMoreSpace()
    {
        SetAlgorithmFields(maxStamps: 1000000); // ensure the limited allowed stamps is not making the stamp go to the next page
        await AddStamps([
            (0, 4, 2),
            (0, 2, 1),
            (0, 7, 1),
            (0, 5, 0),
            (0, 0, 0),
        ]);
        BadgeStamp stamp = await _algorithm.FindNewStampPosition(userId: 1);
        Assert.Equal(1, stamp.Page);
    }
    
    [Fact]
    public async Task FindNewStampPosition_ShouldPasteOnFirstMiddlePage()
    {
        SetAlgorithmFields(maxStamps: 2);
        // The stamps being colliding/out-of-border is not relevant for this test
        await AddStamps([
            (0, 4, 2),
            (0, 2, 1),
            (1, 7, 1),
            (1, 5, 0),
            (2, 0, 0),
            (2, 6, 0),
            (3, 0, 0),
            (4, 6, 0),
            (4, 6, 0),
            (5, 6, 0),
            (5, 6, 0),
            (6, 6, 0),
        ]);
        BadgeStamp stamp = await _algorithm.FindNewStampPosition(userId: 1);
        Assert.Equal(3, stamp.Page);
    }
    
    [Fact]
    public async Task FindNewStampPosition_ShouldPasteOnFirstMiddlePageAvailable()
    {
        SetAlgorithmFields(maxStamps: 2, passportHeight: 1);
        // The stamps being colliding/out-of-border is not relevant for this test
        await AddStamps([
            (0, 4, 2),
            (0, 2, 1),
            (1, 7, 1),
            (1, 5, 1),
            (2, 0, 1),
            (2, 6, 1),
            (3, 4, 0),
            (4, 6, 1),
            (4, 6, 1),
            (5, 6, 1),
            (5, 6, 1),
            (6, 2, 0),
            (7, 6, 1),
            (7, 6, 1),
        ]);
        BadgeStamp stamp = await _algorithm.FindNewStampPosition(userId: 1);
        Assert.Equal(8, stamp.Page);
    }
    
    [Fact]
    public async Task FindNewStampPosition_DoesNotMixOtherUsersStamps()
    {
        await _db.Users.AddAsync(CreateUser(2));
        await _db.SaveChangesAsync();
        
        SetAlgorithmFields(stampWidth: 1);
        await AddStamps([
            (0, 4, 2),
            (0, 3, 1),
            (0, 4, 0),
            (0, 6, 0),
        ]);
        await AddStamps([
            (0, 4, 2),
            (0, 3, 1),
            (0, 4, 0),
            (0, 6, 0),
        ], userId: 2);
        
        BadgeStamp stamp = await _algorithm.FindNewStampPosition(userId: 1);
        Assert.Equal(0, stamp.Page);
    }

    private void SetAlgorithmFields(
        int passportWidth = 12, int passportHeight = 3, 
        int stampWidth = 5, int stampHeight = 1, 
        int maxStamps = 5
    )
    {
        typeof(SimpleStampAlgorithm)
            .GetField("_passportWidth", BindingFlags.NonPublic | BindingFlags.Static)!
            .SetValue(null, passportWidth);
        
        typeof(SimpleStampAlgorithm)
            .GetField("_passportHeight", BindingFlags.NonPublic | BindingFlags.Static)!
            .SetValue(null, passportHeight);
        
        typeof(SimpleStampAlgorithm)
            .GetField("_stampWidth", BindingFlags.NonPublic | BindingFlags.Static)!
            .SetValue(null, stampWidth);
        
        typeof(SimpleStampAlgorithm)
            .GetField("_stampHeight", BindingFlags.NonPublic | BindingFlags.Static)!
            .SetValue(null, stampHeight);
        
        typeof(SimpleStampAlgorithm)
            .GetField("_maxStamps", BindingFlags.NonPublic | BindingFlags.Static)!
            .SetValue(null, maxStamps);

        // constructor call is needed again.
        _algorithm = new SimpleStampAlgorithm(_db);
    }

    private async Task AddStamps((int page, int x, int y)[] stampPositions, int userId = 1)
    {
        foreach (var (page, x, y) in stampPositions)
        {
            Badge badge = _db.Badges.Add(new Badge
            {
                Identifier = $"SomeRandomBadge{_badgeCounter++}", ProgressNeeded = 1
            }).Entity;
            
            BadgeStamp stamp = _db.BadgeStamps.Add(new BadgeStamp
            {
                X = x, Y = y, Page = page, DateAccomplished = DateTime.UtcNow
            }).Entity;

            _db.BadgeProgresses.Add(new BadgeProgress
            {
                BadgeIdentifier = badge.Identifier, UserId = userId, Progress = 1, Stamp = stamp
            });
        }

        await _db.SaveChangesAsync();
    }
    
    private User CreateUser(int id = 1)
    {
        return new User
        {
            Id = id,
            DisplayName = $"user{id}",
            FirstName = "Test",
            LastName = "User",
            Email = $"user{id}@example.com",
            PasswordHash = "x"
        };
    }
}