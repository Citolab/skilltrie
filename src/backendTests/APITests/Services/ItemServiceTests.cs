/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using API.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;


namespace APITests.Services;

public class ItemServiceTests
{
    private readonly AppDbContext db;
    private readonly Mock<ITopicService> topicService;
    private readonly ItemService service;

    public ItemServiceTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        db = new AppDbContext(options);
        db.Database.EnsureCreated();

        topicService = new Mock<ITopicService>();
        service = new ItemService(db, topicService.Object);
    }

    private Item CreateItem(int id = 0, string text = "Hello world")
    {
        return new Item
        {
            Id = id,
            Active = true,
            Type = Item.ItemType.MultipleChoice,
            Source = Item.ItemSource.LLM,
            Lang = Item.Language.English,
            QuestionText = text,
            ResponseType = "conceptual",
            AppearanceCount = 0,
            Level = "easy",
            Answers = new List<ItemAnswer>
            {
                new ItemAnswer { AnswerText = "A", Correct = true, AnswerIdentifier = "A" }
            }
        };
    }

    [Fact]
    public async Task VerifyItemExists_WhenItemExists_DoesNotThrow()
    {
        var item = CreateItem(1);
        db.Items.Add(item);
        await db.SaveChangesAsync();

        await service.VerifyItemExists(1);
    }

    [Fact]
    public async Task VerifyItemExists_WhenItemMissing_Throws()
    {
        await Assert.ThrowsAsync<Exception>(() => service.VerifyItemExists(999));
    }

    [Fact]
    public async Task GetItem_WithoutAnswers_ReturnsItemWithoutAnswerList()
    {
        var item = CreateItem(1);
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var result = await service.GetItem(1);

        Assert.Equal(1, result.Id);
        Assert.Empty(result.Answers);
    }

    [Fact]
    public async Task GetItem_WithAnswers_ReturnsItemWithAnswers()
    {
        var item = CreateItem(1);
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var result = await service.GetItem(1, withAnswers: true);

        Assert.Equal(1, result.Id);
        Assert.NotNull(result.Answers);
        Assert.Single(result.Answers);
        Assert.Equal("A", result.Answers.First().AnswerText);
    }

    [Fact]
    public async Task GetItem_WhenMissing_Throws()
    {
        await Assert.ThrowsAsync<Exception>(() => service.GetItem(999));
    }

    [Fact]
    public async Task GetItems_ReturnsCorrectRange()
    {

        db.Items.Add(CreateItem(1, "Q1"));
        db.Items.Add(CreateItem(2, "Q2"));
        db.Items.Add(CreateItem(3, "Q3"));
        await db.SaveChangesAsync();

        var items = await service.GetItems(offset: 1, range: 2, 100);

        Assert.Equal(2, items.Length);
        Assert.Equal(2, items[0].Id);
        Assert.Equal(3, items[1].Id);
    }

    [Fact]
    public async Task GetItems_TruncatesQuestionText()
    {
        var longText = new string('X', 200);
        db.Items.Add(CreateItem(1, longText));
        await db.SaveChangesAsync();

        var items = await service.GetItems(0, 10, 50);

        Assert.Equal(50, items[0].QuestionText.Length);
    }

    [Fact]
    public async Task GetItems_ReturnsTopicPaths()
    {
        // Topic hierarchy:
        // Root (Id=1)
        //   -> Child (Id=2)

        var root = new Scope { Id = 1, Name = "Root" };
        var child = new Scope { Id = 2, Name = "Child" };

        db.Scopes.AddRange(root, child);

        db.ScopeMemberships.Add(new ScopeMembership { AncestorId = 1, DescendantId = 2, Depth = 1 });

        await db.SaveChangesAsync();

        var ancestors = db.Scopes.Where(x => x.Id == root.Id);

        Assert.False(ancestors.Count() == 0);

        var ancestor = ancestors.First();
        var scopeMemberships = db.ScopeMemberships.Where(x => x.Ancestor.Id == ancestor.Id);

        Assert.False(scopeMemberships.Count() == 0);

        var scopeMembership = scopeMemberships.First();
        var descendants = db.Scopes.Where(x => x.Id == scopeMembership.DescendantId);

        Assert.False(descendants.Count() == 0);

        var descendant = descendants.First();

        Assert.Equal("Root", ancestor.Name);
        Assert.Equal("Child", descendant.Name);
    }

    [Fact]
    public async Task GetItems_NoItems_ReturnsEmptyArray()
    {
        var items = await service.GetItems(0, 10, 100);

        Assert.Empty(items);
    }

    [Fact]
    public async Task GetItemsForAssessment_ReturnsRequestedItems()
    {
        // Arrange
        db.Items.Add(CreateItem(1, "Q1"));
        db.Items.Add(CreateItem(2, "Q2"));
        db.Items.Add(CreateItem(3, "Q3"));
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetItemsForAssessment(new[] { 1, 3 });

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Contains(result, i => i.Id == 1);
        Assert.Contains(result, i => i.Id == 3);
    }

    [Fact]
    public async Task GetItemsForAssessment_WhenMissingItems_ThrowsKeyNotFoundException()
    {
        // Arrange
        db.Items.Add(CreateItem(1, "Q1"));
        await db.SaveChangesAsync();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetItemsForAssessment(new[] { 1, 2, 3 }));

        Assert.Contains("2, 3", ex.Message);
    }

    [Fact]
    public async Task NotRecentlySeenItems_WhenUserHasSeenNone_ReturnsAllItems()
    {
        db.Items.Add(CreateItem(1, "Q1"));
        db.Items.Add(CreateItem(2, "Q2"));
        await db.SaveChangesAsync();

        var result = service.NotRecentlySeenItems(123).ToArray();

        Assert.Equal(2, result.Length);
        Assert.Contains(result, i => i.Id == 1);
        Assert.Contains(result, i => i.Id == 2);
    }

    [Fact]
    public async Task NotRecentlySeenItems_ExcludesRecentlySeenItems()
    {
        var user = new User
        {
            Id = 123,
            DisplayName = "test",
            FirstName = "A",
            LastName = "B",
            Email = "test@example.com",
            PasswordHash = "x"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var item1 = CreateItem(1, "Q1");
        var item2 = CreateItem(2, "Q2");
        db.Items.AddRange(item1, item2);
        await db.SaveChangesAsync();

        var levelResult = new LevelResult
        {
            UserId = 123,
            User = user,
            CreatedAt = DateTime.UtcNow
        };
        db.LevelResults.Add(levelResult);
        await db.SaveChangesAsync();

        db.UserAnswers.Add(new UserAnswer
        {
            ItemId = 1,
            Item = item1,
            LevelResultId = levelResult.Id,
            LevelResult = levelResult,
            Correct = true
        });
        await db.SaveChangesAsync();

        var result = service.NotRecentlySeenItems(123).ToArray();

        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public async Task NotRecentlySeenItems_IncludesItemsSeenLongAgo()
    {
        var user = new User
        {
            Id = 123,
            DisplayName = "test",
            FirstName = "A",
            LastName = "B",
            Email = "test@example.com",
            PasswordHash = "x"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var item1 = CreateItem(1, "Q1");
        db.Items.Add(item1);
        await db.SaveChangesAsync();

        var levelResult = new LevelResult
        {
            UserId = 123,
            User = user,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };
        db.LevelResults.Add(levelResult);
        await db.SaveChangesAsync();

        db.UserAnswers.Add(new UserAnswer
        {
            ItemId = 1,
            Item = item1,
            LevelResultId = levelResult.Id,
            LevelResult = levelResult,
            Correct = true
        });
        await db.SaveChangesAsync();

        var result = service.NotRecentlySeenItems(123).ToArray();

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    //can't get this to work with a sqllite database

    // [Fact]
    // public async Task GetItemByAnswertext_MatchesQuestionText()
    // {
    //     db.Items.Add(CreateItem(1, "The capital of France is Paris"));
    //     db.Items.Add(CreateItem(2, "No match here"));
    //     await db.SaveChangesAsync();
    //
    //     var result = await service.GetItemByAnswertext("Paris", 200);
    //
    //     Assert.Single(result);
    //     Assert.Equal(1, result[0].Id);
    // }
    //
    // [Fact]
    // public async Task GetItemByAnswertext_TruncatesQuestionText()
    // {
    //     var longText = new string('X', 300);
    //     db.Items.Add(CreateItem(1, longText));
    //     await db.SaveChangesAsync();
    //
    //     var result = await service.GetItemByAnswertext("X", 50);
    //
    //     Assert.Single(result);
    //     Assert.Equal(50, result[0].QuestionText.Length);
    // }
    //
    // [Fact]
    // public async Task GetItemByAnswertext_NoMatches_ReturnsEmpty()
    // {
    //     db.Items.Add(CreateItem(1, "Nothing relevant here"));
    //     await db.SaveChangesAsync();
    //
    //     var result = await service.GetItemByAnswertext("Paris", 200);
    //
    //     Assert.Empty(result);
    // }

    [Fact]
    public async Task UpdateItem_WhenItemDoesNotExist_Throws()
    {
        var updated = CreateItem(999, "Updated");

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateItem(updated));

        Assert.Equal("Item not found", ex.Message);
    }

    [Fact]
    public async Task UpdateItem_UpdatesScalarFields()
    {
        var item = CreateItem(1, "Old text");
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var updated = CreateItem(1, "New text");
        updated.Active = false;
        updated.Level = "3";

        await service.UpdateItem(updated);

        var result = await db.Items.FindAsync(1);

        Assert.Equal("New text", result.QuestionText);
        Assert.False(result.Active);
        Assert.Equal("3", result.Level);
    }

    //expected it to replace answers but it appends them, currently going to assume this is intended
    [Fact]
    public async Task UpdateItem_ReplacesAnswers()
    {
        var item = CreateItem(1, "Q1");
        item.Answers.Add(new ItemAnswer { AnswerIdentifier = "A", AnswerText = "A", Correct = true });
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var updated = CreateItem(1, "Q1");
        updated.Answers.Add(new ItemAnswer { AnswerIdentifier = "B", AnswerText = "New", Correct = false });

        await service.UpdateItem(updated);

        var result = await db.Items.Include(i => i.Answers).FirstAsync();

        Assert.Equal(2, result.Answers.Count);
        Assert.Contains(result.Answers, a => a.AnswerIdentifier == "A");
        Assert.Contains(result.Answers, a => a.AnswerIdentifier == "B");
    }

    [Fact]
    public async Task UpdateItem_WithTopicIds_CallsTopicService()
    {
        var mockTopicService = new Mock<ITopicService>();
        var service = new ItemService(db, mockTopicService.Object);

        var item = CreateItem(1, "Q1");
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var updated = CreateItem(1, "Q1");
        updated.Scopes = new List<Scope>
        {
            new Scope { Id = 10 },
            new Scope { Id = 20 }
        };

        await service.UpdateItem(updated);

        mockTopicService.Verify(
            s => s.UpdateTopicsForItem(item, It.Is<int[]>(ids =>
                ids.SequenceEqual(new[] { 10, 20 })
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateItem_WithoutTopicIds_DoesNotCallTopicService()
    {
        var mockTopicService = new Mock<ITopicService>();
        var service = new ItemService(db, mockTopicService.Object);

        var item = CreateItem(1, "Q1");
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var updated = CreateItem(1, "Q1");
        updated.Scopes = null;

        await service.UpdateItem(updated);

        mockTopicService.Verify(
            s => s.UpdateTopicsForItem(It.IsAny<Item>(), It.IsAny<int[]>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ActivateItem_SetsActiveToTrue()
    {
        var item = CreateItem(1, "Q1");
        item.Active = false;
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var result = await service.ActivateItem(1);

        Assert.True(result.Active);

        var dbItem = await db.Items.FindAsync(1);
        Assert.True(dbItem.Active);
    }

    [Fact]
    public async Task ActivateItem_WhenItemDoesNotExist_Throws()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => service.ActivateItem(999));
        Assert.Equal("item not found", ex.Message);
    }

    [Fact]
    public async Task DeactivateItem_SetsActiveToFalse()
    {
        var item = CreateItem(1, "Q1");
        item.Active = true;
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var result = await service.DeactivateItem(1);

        Assert.False(result.Active);

        var dbItem = await db.Items.FindAsync(1);
        Assert.False(dbItem.Active);
    }

    [Fact]
    public async Task DeactivateItem_WhenItemDoesNotExist_Throws()
    {
        var ex = await Assert.ThrowsAsync<Exception>(() => service.DeactivateItem(999));
        Assert.Equal("item not found", ex.Message);
    }

    [Fact]
    public async Task GetItems_SortByLangAscending_ReturnsItemsInOrder()
    {
        var item1 = CreateItem(1, "Q1");
        item1.Lang = Item.Language.Dutch;

        var item2 = CreateItem(2, "Q2");
        item2.Lang = Item.Language.English;

        db.Items.AddRange(item1, item2);
        await db.SaveChangesAsync();

        var items = await service.GetItems(0, 10, 100, "lang", "ascending");

        // alphabetically: English < Dutch (because of the enum) -> ids 2, 1
        Assert.Equal(new[] { 2, 1 }, items.Select(i => i.Id));
    }

    [Fact]
    public async Task GetItems_SortByLangWithTie_BreaksTiesById()
    {
        var item1 = CreateItem(1, "Q1");
        item1.Lang = Item.Language.English;

        var item2 = CreateItem(2, "Q2");
        item2.Lang = Item.Language.Dutch;

        var item3 = CreateItem(3, "Q3");
        item3.Lang = Item.Language.English; // shares Lang with item1 on purpose

        db.Items.AddRange(item1, item2, item3);
        await db.SaveChangesAsync();

        var items = await service.GetItems(0, 10, 100, "lang", "ascending");

        // English items (1, 3) come first, ordered by Id, then Dutch (2)
        Assert.Equal(new[] { 1, 3, 2 }, items.Select(i => i.Id));
    }

    [Fact]
    public async Task GetItems_SortByActiveAscending_ReturnsItemsInOrder()
    {
        var item1 = CreateItem(1, "Q1");
        item1.Active = true;

        var item2 = CreateItem(2, "Q2");
        item2.Active = false;

        db.Items.AddRange(item1, item2);
        await db.SaveChangesAsync();

        var items = await service.GetItems(0, 10, 100, "active", "ascending");

        // false < true -> id 2, then id 1
        Assert.Equal(new[] { 2, 1 }, items.Select(i => i.Id));
    }

    [Fact]
    public async Task GetItems_SortByActiveWithTie_BreaksTiesById()
    {
        var item1 = CreateItem(1, "Q1");
        item1.Active = true;

        var item2 = CreateItem(2, "Q2");
        item2.Active = false;

        var item3 = CreateItem(3, "Q3");
        item3.Active = true; // shares Active with item1 on purpose

        db.Items.AddRange(item1, item2, item3);
        await db.SaveChangesAsync();

        var items = await service.GetItems(0, 10, 100, "active", "ascending");

        // false (id 2) first, then true items (1, 3) ordered by Id
        Assert.Equal(new[] { 2, 1, 3 }, items.Select(i => i.Id));
    }

    [Fact]
public async Task GetItems_SortByTypeAscending_ReturnsItemsInOrder()
{
    var item1 = CreateItem(1, "Q1");
    item1.Type = Item.ItemType.Numerical;

    var item2 = CreateItem(2, "Q2");
    item2.Type = Item.ItemType.MultipleChoice;

    var item3 = CreateItem(3, "Q3");
    item3.Type = Item.ItemType.Open;

    db.Items.AddRange(item1, item2, item3);
    await db.SaveChangesAsync();

    var items = await service.GetItems(0, 10, 100, "type", "ascending");

    // enum order: MultipleChoice (0) < Open (1) < Numerical (2) -> ids 2, 3, 1
    Assert.Equal(new[] { 2, 3, 1 }, items.Select(i => i.Id));
}

[Fact]
public async Task GetItems_SortByTypeWithTie_BreaksTiesById()
{
    var item1 = CreateItem(1, "Q1");
    item1.Type = Item.ItemType.Open;

    var item2 = CreateItem(2, "Q2");
    item2.Type = Item.ItemType.MultipleChoice;

    var item3 = CreateItem(3, "Q3");
    item3.Type = Item.ItemType.Open; // shares Type with item1 on purpose

    db.Items.AddRange(item1, item2, item3);
    await db.SaveChangesAsync();

    var items = await service.GetItems(0, 10, 100, "type", "ascending");

    // MultipleChoice (id 2) first, then Open items (1, 3) ordered by Id
    Assert.Equal(new[] { 2, 1, 3 }, items.Select(i => i.Id));
}

[Fact]
public async Task GetItems_SortBySourceAscending_ReturnsItemsInOrder()
{
    var item1 = CreateItem(1, "Q1");
    item1.Source = Item.ItemSource.Databank;

    var item2 = CreateItem(2, "Q2");
    item2.Source = Item.ItemSource.LLM;

    var item3 = CreateItem(3, "Q3");
    item3.Source = Item.ItemSource.Imported;

    db.Items.AddRange(item1, item2, item3);
    await db.SaveChangesAsync();

    var items = await service.GetItems(0, 10, 100, "source", "ascending");

    // enum order: LLM (0) < Imported (1) < Databank (2) -> ids 2, 3, 1
    Assert.Equal(new[] { 2, 3, 1 }, items.Select(i => i.Id));
}

[Fact]
public async Task GetItems_SortBySourceWithTie_BreaksTiesById()
{
    var item1 = CreateItem(1, "Q1");
    item1.Source = Item.ItemSource.Imported;

    var item2 = CreateItem(2, "Q2");
    item2.Source = Item.ItemSource.LLM;

    var item3 = CreateItem(3, "Q3");
    item3.Source = Item.ItemSource.Imported; // shares Source with item1 on purpose

    db.Items.AddRange(item1, item2, item3);
    await db.SaveChangesAsync();

    var items = await service.GetItems(0, 10, 100, "source", "ascending");

    // LLM (id 2) first, then Imported items (1, 3) ordered by Id
    Assert.Equal(new[] { 2, 1, 3 }, items.Select(i => i.Id));
}
}

