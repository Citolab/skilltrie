/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers;
using Models;
using API.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace APITests.Services;

public class GroupServiceTests
{
    private readonly AppDbContext db;
    private readonly Mock<IGroupHandler> handler;
    private readonly GroupService service;

    public GroupServiceTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        db = new AppDbContext(options);
        db.Database.EnsureCreated();

        handler = new Mock<IGroupHandler>();
        service = new GroupService(db, handler.Object);
    }

    [Fact]
    public async Task CreateGroup_Succeeds_WhenNameValid()
    {
        handler.Setup(h => h.ValidateGroupName("Test"))
            .Returns((true, null));

        var group = await service.CreateGroup("Test");

        Assert.NotNull(group);
        Assert.Equal("Test", group.Name);

        var saved = await db.Groups.FirstAsync();
        Assert.Equal("Test", saved.Name);
    }

    [Fact]
    public async Task CreateGroup_Throws_WhenNameInvalid()
    {
        handler.Setup(h => h.ValidateGroupName("Bad"))
            .Returns((false, "Invalid"));

        var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateGroup("Bad"));
        Assert.Equal("Invalid", ex.Message);
    }

    [Fact]
    public async Task GetGroup_ReturnsGroup_WhenExists()
    {
        var user = new User { Id = 1, FirstName = "A", LastName = "B", DisplayName = "D" };
        var group = new Group { Id = 10, Name = "G" };
        var member = new GroupMember { GroupId = 10, UserId = 1, User = user };

        db.Users.Add(user);
        db.Groups.Add(group);
        db.GroupMembers.Add(member);
        await db.SaveChangesAsync();

        var result = await service.GetGroup(10);

        Assert.NotNull(result);
        Assert.Single(result.Members);
    }

    [Fact]
    public async Task GetGroup_Throws_WhenMissing()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => service.GetGroup(999));
        Assert.Equal("Group 999 not found", ex.Message);
    }

    [Fact]
    public async Task AddMember_Succeeds_WhenValid()
    {
        db.Users.Add(new User { Id = 1, FirstName = "A", LastName = "B", DisplayName = "D" });
        db.Groups.Add(new Group { Id = 10, Name = "group" });
        await db.SaveChangesAsync();

        handler.Setup(h => h.ValidateAddMember(true, true, false))
            .Returns((true, null));

        var member = await service.AddMember(10, 1);

        Assert.NotNull(member);
        Assert.Equal(10, member.GroupId);
        Assert.Equal(1, member.UserId);
    }

    [Fact]
    public async Task AddMember_Throws_WhenValidationFails()
    {
        handler.Setup(h => h.ValidateAddMember(false, false, false))
            .Returns((false, "Nope"));

        var ex = await Assert.ThrowsAsync<Exception>(() => service.AddMember(10, 1));
        Assert.Equal("Nope", ex.Message);
    }

    [Fact]
    public async Task RemoveMember_Succeeds_WhenMemberExists()
    {
        var u = new User { Id = 1, FirstName = "A", LastName = "B", DisplayName = "D" };
        var g = new Group { Id = 10, Name = "group" };

        db.Users.Add(u);
        db.Groups.Add(g);
        db.GroupMembers.Add(new GroupMember { GroupId = 10, UserId = 1 });
        await db.SaveChangesAsync();

        await service.RemoveMember(10, 1);

        Assert.Empty(db.GroupMembers);
    }

    [Fact]
    public async Task RemoveMember_Throws_WhenMissing()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => service.RemoveMember(10, 1));
        Assert.Equal("User is not in group", ex.Message);
    }

    [Fact]
    public async Task GetGroups_ReturnsPagedResults()
    {
        db.Groups.AddRange(
            new Group { Id = 1, Name = "A" },
            new Group { Id = 2, Name = "B" }
        );
        await db.SaveChangesAsync();

        var result = await service.GetGroups(0, 1);

        Assert.Single(result);
    }

    [Fact]
    public async Task UpdateGroup_Succeeds_WhenValid()
    {
        db.Groups.Add(new Group { Id = 10, Name = "Old" });
        await db.SaveChangesAsync();

        handler.Setup(h => h.ValidateGroupName("New"))
            .Returns((true, null));

        var group = await service.UpdateGroup(10, "New");

        Assert.Equal("New", group.Name);
    }

    [Fact]
    public async Task UpdateGroup_Throws_WhenNameInvalid()
    {
        handler.Setup(h => h.ValidateGroupName("Bad"))
            .Returns((false, "Invalid"));

        var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateGroup(10, "Bad"));
        Assert.Equal("Invalid", ex.Message);
    }

    [Fact]
    public async Task UpdateGroup_Throws_WhenGroupMissing()
    {
        handler.Setup(h => h.ValidateGroupName("New"))
            .Returns((true, null));

        var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateGroup(999, "New"));
        Assert.Equal("Group 999 not found", ex.Message);
    }

    [Fact]
    public async Task DeleteGroup_Succeeds_WhenExists()
    {
        var u = new User { Id = 1, FirstName = "A", LastName = "B", DisplayName = "D" };
        var g = new Group { Id = 10, Name = "group" };

        db.Users.Add(u);
        db.Groups.Add(g);
        db.GroupMembers.Add(new GroupMember { GroupId = 10, UserId = 1 });
        await db.SaveChangesAsync();

        await service.DeleteGroup(10);

        Assert.Empty(db.Groups);
        Assert.Empty(db.GroupMembers);
    }

    [Fact]
    public async Task DeleteGroup_Throws_WhenMissing()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteGroup(999));
        Assert.Equal("Group 999 not found", ex.Message);
    }

    [Fact]
    public async Task GetGroupProficiency_ReturnsZero_WhenNoMembers()
    {
        var (avg, filter) = await service.GetGroupProficiency(10, null, null);

        Assert.Equal(0, avg);
        Assert.Equal("All", filter);
    }

    [Fact]
    public async Task GetGroupProficiency_FiltersByTopicId()
    {
        var u = new User { Id = 1, FirstName = "A", LastName = "B", DisplayName = "D" };
        var g = new Group { Id = 10, Name = "group" };
        var t = new Scope { Id = 5, Name = "wow" };

        db.Users.Add(u);
        db.Groups.Add(g);
        db.Scopes.Add(t);
        db.GroupMembers.Add(new GroupMember { GroupId = 10, UserId = 1 });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 5, Proficiency = 80 });
        await db.SaveChangesAsync();

        var (avg, filter) = await service.GetGroupProficiency(10, 5, null);

        Assert.Equal(80, avg);
        Assert.Equal("5", filter);
    }

    [Fact]
    public async Task GetGroupProficiency_FiltersByTopicIds()
    {
        var u = new User { Id = 1, FirstName = "A", LastName = "B", DisplayName = "D" };
        var g = new Group { Id = 10, Name = "group" };
        var t = new Scope { Id = 5, Name = "wow" };

        db.Users.Add(u);
        db.Groups.Add(g);
        db.Scopes.Add(t);
        db.GroupMembers.Add(new GroupMember { GroupId = 10, UserId = 1 });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 5, Proficiency = 80 });
        await db.SaveChangesAsync();

        var (avg, filter) = await service.GetGroupProficiency(10, null, new List<int> { 5 });

        Assert.Equal(80, avg);
        Assert.Equal("5", filter);
    }

    [Fact]
    public async Task GetGroupProficiency_UsesAllTopics_WhenNoFilter()
    {
        var u = new User { Id = 1, FirstName = "A", LastName = "B", DisplayName = "D" };
        var g = new Group { Id = 10, Name = "group" };
        var t = new Scope { Id = 5, Name = "wow" };

        db.Users.Add(u);
        db.Groups.Add(g);
        db.Scopes.Add(t);
        db.GroupMembers.Add(new GroupMember { GroupId = 10, UserId = 1 });
        db.UserScopeProgress.Add(new UserScopeProgress { UserId = 1, ScopeId = 5, Proficiency = 80 });
        await db.SaveChangesAsync();

        var (avg, filter) = await service.GetGroupProficiency(10, null, null);

        Assert.Equal(80, avg);
        Assert.Equal("All", filter);
    }
}
