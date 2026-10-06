/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
using API.Handlers;
using Models;

namespace APITests.Handlers;

public class UserCharacterHandlerTests
{
    private static User DummyUser(int id = 1)
        => new User { Id = id, UserName = $"user{id}@test.com", Email = $"user{id}@test.com" };

    private static Character DummyCharacter(int id = 1)
        => new Character { Id = id, Name = "dog" };

    [Fact]
    public void NewUserCharacter_ReturnsUserCharacter_WithCorrectUserId()
    {
        var handler = new UserCharacterHandler();
        var user = DummyUser(id: 42);
        var character = DummyCharacter(id: 1);

        var result = handler.NewUserCharacter(user, character);

        Assert.Equal(42, result.UserId);
    }

    [Fact]
    public void NewUserCharacter_ReturnsUserCharacter_WithCorrectCharacterId()
    {
        var handler = new UserCharacterHandler();
        var user = DummyUser(id: 1);
        var character = DummyCharacter(id: 7);

        var result = handler.NewUserCharacter(user, character);

        Assert.Equal(7, result.CharacterId);
    }

    [Fact]
    public void NewUserCharacter_ReturnsUserCharacter_WithUserNavigationSet()
    {
        var handler = new UserCharacterHandler();
        var user = DummyUser();
        var character = DummyCharacter();

        var result = handler.NewUserCharacter(user, character);

        Assert.Same(user, result.User);
    }

    [Fact]
    public void NewUserCharacter_ReturnsUserCharacter_WithCharacterNavigationSet()
    {
        var handler = new UserCharacterHandler();
        var user = DummyUser();
        var character = DummyCharacter();

        var result = handler.NewUserCharacter(user, character);

        Assert.Same(character, result.Character);
    }

    [Fact]
    public void NewUserCharacter_ReturnsUserCharacter_WithSelectedFalse()
    {
        var handler = new UserCharacterHandler();

        var result = handler.NewUserCharacter(DummyUser(), DummyCharacter());

        Assert.False(result.Selected);
    }

    [Fact]
    public void NewUserCharacter_ThrowsArgumentNullException_WhenUserIsNull()
    {
        var handler = new UserCharacterHandler();

        Assert.Throws<ArgumentNullException>(() => handler.NewUserCharacter(null!, DummyCharacter()));
    }

    [Fact]
    public void NewUserCharacter_ThrowsArgumentNullException_WhenCharacterIsNull()
    {
        var handler = new UserCharacterHandler();

        Assert.Throws<ArgumentNullException>(() => handler.NewUserCharacter(DummyUser(), null!));
    }

    [Fact]
    public void NewUserCharacter_ThrowsArgumentNullException_WhenBothArgumentsAreNull()
    {
        var handler = new UserCharacterHandler();

        Assert.Throws<ArgumentNullException>(() => handler.NewUserCharacter(null!, null!));
    }
}
