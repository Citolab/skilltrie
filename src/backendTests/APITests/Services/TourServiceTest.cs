/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using API.Services;

namespace APITests.Services;

public class TourServiceTests
{
    private static User CreateTestUser(AppDbContext ctx, int id)
    {
        var user = new User { Id = id, FirstName = "John", LastName = "Pork", DisplayName = "JohnPork" };
        ctx.Users.Add(user);
        ctx.SaveChanges();
        return user;
    }

    [Fact(DisplayName = "GetTourSeen: returns false when user has not seen the tour")]
    public void GetTourSeen_ReturnsFalse_WhenNotSeen()
    {
        var ctx = SqliteInMemoryContextFactory.Create();

        var svc = new TourService(ctx);

        Assert.False(svc.GetTourSeen(1, "qti-tour"));
    }

    [Fact(DisplayName = "GetTourSeen: returns true when user has seen the tour")]
    public void GetTourSeen_ReturnsTrue_WhenSeen()
    {
        var ctx = SqliteInMemoryContextFactory.Create();

        CreateTestUser(ctx, 1);
        ctx.UserTours.Add(new UserTour { UserId = 1, TourKey = "qti-tour" });
        ctx.SaveChanges();

        var svc = new TourService(ctx);

        Assert.True(svc.GetTourSeen(1, "qti-tour"));
    }

    [Fact(DisplayName = "GetTourSeen: returns false for different user with same tour key")]
    public void GetTourSeen_ReturnsFalse_WhenDifferentUser()
    {
        var ctx = SqliteInMemoryContextFactory.Create();

        CreateTestUser(ctx, 1);
        ctx.UserTours.Add(new UserTour { UserId = 1, TourKey = "qti-tour" });
        ctx.SaveChanges();

        var svc = new TourService(ctx);

        Assert.False(svc.GetTourSeen(2, "qti-tour"));
    }

    [Fact(DisplayName = "GetTourSeen: returns false for same user with different tour key")]
    public void GetTourSeen_ReturnsFalse_WhenDifferentTourKey()
    {
        var ctx = SqliteInMemoryContextFactory.Create();

        CreateTestUser(ctx, 1);
        ctx.UserTours.Add(new UserTour { UserId = 1, TourKey = "qti-tour" });
        ctx.SaveChanges();

        var svc = new TourService(ctx);

        Assert.False(svc.GetTourSeen(1, "home-tour"));
    }

    [Fact(DisplayName = "MarkTourSeen: adds a record for the user and tour key")]
    public void MarkTourSeen_AddsRecord()
    {
        var ctx = SqliteInMemoryContextFactory.Create();

        CreateTestUser(ctx, 1);

        var svc = new TourService(ctx);
        svc.MarkTourSeen(1, "qti-tour");

        Assert.True(ctx.UserTours.Any(t => t.UserId == 1 && t.TourKey == "qti-tour"));
    }

    [Fact(DisplayName = "MarkTourSeen: does not add a duplicate record when already seen")]
    public void MarkTourSeen_DoesNotDuplicate_WhenAlreadySeen()
    {
        var ctx = SqliteInMemoryContextFactory.Create();

        CreateTestUser(ctx, 1);
        ctx.UserTours.Add(new UserTour { UserId = 1, TourKey = "qti-tour" });
        ctx.SaveChanges();

        var svc = new TourService(ctx);
        svc.MarkTourSeen(1, "qti-tour");

        Assert.Equal(1, ctx.UserTours.Count(t => t.UserId == 1 && t.TourKey == "qti-tour"));
    }

    [Fact(DisplayName = "MarkTourSeen: adds records independently for different users")]
    public void MarkTourSeen_AddsIndependently_ForDifferentUsers()
    {
        var ctx = SqliteInMemoryContextFactory.Create();

        CreateTestUser(ctx, 1);
        CreateTestUser(ctx, 2);

        var svc = new TourService(ctx);
        svc.MarkTourSeen(1, "qti-tour");
        svc.MarkTourSeen(2, "qti-tour");

        Assert.True(ctx.UserTours.Any(t => t.UserId == 1 && t.TourKey == "qti-tour"));
        Assert.True(ctx.UserTours.Any(t => t.UserId == 2 && t.TourKey == "qti-tour"));
    }

    [Fact(DisplayName = "MarkTourSeen: adds records independently for different tour keys")]
    public void MarkTourSeen_AddsIndependently_ForDifferentTourKeys()
    {
        var ctx = SqliteInMemoryContextFactory.Create();

        CreateTestUser(ctx, 1);

        var svc = new TourService(ctx);
        svc.MarkTourSeen(1, "qti-tour");
        svc.MarkTourSeen(1, "homepage-tour");

        Assert.True(ctx.UserTours.Any(t => t.UserId == 1 && t.TourKey == "qti-tour"));
        Assert.True(ctx.UserTours.Any(t => t.UserId == 1 && t.TourKey == "homepage-tour"));
    }
}