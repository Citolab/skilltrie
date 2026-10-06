/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Net;
using Models;

namespace IntegrationTests;

public class AIQuestionTests : AbstractAsyncLifetime
{
    [Fact]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
    {
        //Arrange
        int userId = 1;
        string role = Roles.Admin;
        var client = _factory.CreateClientAs(
            userId,
            $"{userId}@test.com",
            role);

        var db = _factory.CreateDbContext();
        await db.Database.EnsureCreatedAsync();

        db.Items.Add(new Item { Id = 1, QuestionText = "Test Question", Type = Item.ItemType.MultipleChoice, Source = Item.ItemSource.LLM, Lang = Item.Language.Dutch, ResponseType = "conceptual", Level = "easy", AppearanceCount = 0 });
        db.Items.Add(new Item { Id = 2, QuestionText = "Test Question", Type = Item.ItemType.MultipleChoice, Source = Item.ItemSource.LLM, Lang = Item.Language.Dutch, ResponseType = "conceptual", Level = "easy", AppearanceCount = 0 });
        db.Items.Add(new Item { Id = 3, QuestionText = "Test Question", Type = Item.ItemType.MultipleChoice, Source = Item.ItemSource.LLM, Lang = Item.Language.Dutch, ResponseType = "conceptual", Level = "easy", AppearanceCount = 0 });
        db.Scopes.Add(new Scope { Id = 1, Name = "Test Scope" });
        db.ScopeItems.Add(new ScopeItem { ItemId = 1, ScopeId = 1 });
        db.ScopeItems.Add(new ScopeItem { ItemId = 2, ScopeId = 1 });
        db.ScopeItems.Add(new ScopeItem { ItemId = 3, ScopeId = 1 });
        db.SaveChanges();

        //Act
        var response = await client.GetAsync("/api/ai/createSampleQuestions/1/1");

        //Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public async Task QuestionSavedInDatabase()
    {
        //Arrange
        int userId = 1;
        string role = Roles.Admin;
        var client = _factory.CreateClientAs(
            userId,
            $"{userId}@test.com",
            role);

        var db = _factory.CreateDbContext();
        await db.Database.EnsureCreatedAsync();

        db.Items.Add(new Item { Id = 1, QuestionText = "Test Question", Type = Item.ItemType.MultipleChoice, Source = Item.ItemSource.LLM, Lang = Item.Language.Dutch, ResponseType = "conceptual", Level = "easy", AppearanceCount = 0 });
        db.Items.Add(new Item { Id = 2, QuestionText = "Test Question", Type = Item.ItemType.MultipleChoice, Source = Item.ItemSource.LLM, Lang = Item.Language.Dutch, ResponseType = "conceptual", Level = "easy", AppearanceCount = 0 });
        db.Items.Add(new Item { Id = 3, QuestionText = "Test Question", Type = Item.ItemType.MultipleChoice, Source = Item.ItemSource.LLM, Lang = Item.Language.Dutch, ResponseType = "conceptual", Level = "easy", AppearanceCount = 0 });
        db.Scopes.Add(new Scope { Id = 1, Name = "Test Scope" });
        db.ScopeItems.Add(new ScopeItem { ItemId = 1, ScopeId = 1 });
        db.ScopeItems.Add(new ScopeItem { ItemId = 2, ScopeId = 1 });
        db.ScopeItems.Add(new ScopeItem { ItemId = 3, ScopeId = 1 });
        db.SaveChanges();

        //Act
        var response = await client.GetAsync("/api/ai/createSampleQuestions/1/1");

        //Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(4, db.Items.Count()); //3 initial items + 1 generated item
        Assert.NotNull(db.Items.Where(i => i.Id == 4).Select(i => i.QuestionText).FirstOrDefault());
        Assert.Equal(Item.ItemSource.LLM, db.Items.Where(i => i.Id == 4).Select(i => i.Source).FirstOrDefault());
        Assert.Equal(1, db.ScopeItems.Where(ti => ti.ItemId == 4).Select(ti => ti.ScopeId).FirstOrDefault()); //Check if the generated question is linked to the correct topic
    }
}