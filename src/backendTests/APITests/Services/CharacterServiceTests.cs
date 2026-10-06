/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers.GameEventHandlers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models;
using Moq;

namespace APITests.Services;

public class CharacterServiceTests
{
    private readonly CharacterService _svc;
    private readonly AppDbContext _context;

    public CharacterServiceTests()
    {
        var (ctx, _) = CreateSqliteContext();
        _context = ctx;
        _svc = new CharacterService(ctx, Mock.Of<IGameEventManager>());
    }
    
    private static (AppDbContext Ctx, SqliteConnection Conn) CreateSqliteContext()
    {
        var conn = new SqliteConnection("Filename=:memory:");
        conn.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        ctx.Characters.AddRange(
            new Character { Id = 1, Name = "dog" },
            new Character { Id = 2, Name = "cat" },
            new Character { Id = 3, Name = "owl" }
        );
        ctx.Users.AddRange(
            new User { Id = 1, FirstName = "John", LastName = "Pork", Email = "john@test.com", UserName = "john@test.com", DisplayName = "JohnPork" },
            new User { Id = 2, FirstName = "Sara", LastName = "Pork", Email = "sara@test.com", UserName = "sara@test.com", DisplayName = "SaraPork" }
        );
        ctx.SaveChanges();
        return (ctx, conn);
    }

    // -----------------------------------------------------------------------
    // GetCharacters
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetCharacters_ReturnsAllCharacters()
    {
        var result = await _svc.GetCharacters();

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetCharacters_ReturnsCorrectNames()
    {
        var result = await _svc.GetCharacters();
        var names = result.Select(c => c.Name).ToArray();

        Assert.Contains("dog", names);
        Assert.Contains("cat", names);
        Assert.Contains("owl", names);
    }

    [Fact]
    public async Task GetCharacters_ThrowsException_WhenNoCharactersExist()
    {

        _context.Characters.RemoveRange(_context.Characters);
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<Exception>(() => _svc.GetCharacters());
    }

    [Fact]
    public async Task GetDefaultCharacters_ReturnsAllCharacters()
    {
        var result = await _svc.GetDefaultCharacters();
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetDefaultCharacters_ThrowsException_WhenNoCharactersExist()
    {
        _context.Characters.RemoveRange(_context.Characters);
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<Exception>(() => _svc.GetDefaultCharacters());
    }

    [Fact]
    public async Task GetUserCharacters_ReturnsCharacters_ForValidUser()
    {
        _context.UserCharacters.AddRange(
            new UserCharacter { UserId = 1, CharacterId = 1, Selected = false },
            new UserCharacter { UserId = 1, CharacterId = 2, Selected = true }
        );
        await _context.SaveChangesAsync();

        var result = await _svc.GetUserCharacters(1);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUserCharacters_ReturnsCharactersOrderedByCharacterId()
    {
        _context.UserCharacters.AddRange(
            new UserCharacter { UserId = 1, CharacterId = 3, Selected = false },
            new UserCharacter { UserId = 1, CharacterId = 1, Selected = true },
            new UserCharacter { UserId = 1, CharacterId = 2, Selected = false }
        );
        await _context.SaveChangesAsync();

        var result = await _svc.GetUserCharacters(1);
        var ids = result.Select(uc => uc.CharacterId).ToArray();

        Assert.Equal(new[] { 1, 2, 3 }, ids);
    }

    [Fact]
    public async Task GetUserCharacters_IncludesCharacterNavigation()
    {
        _context.UserCharacters.Add(new UserCharacter { UserId = 1, CharacterId = 1, Selected = true });
        await _context.SaveChangesAsync();

        var result = await _svc.GetUserCharacters(1);

        Assert.All(result, uc => Assert.NotNull(uc.Character));
    }

    [Fact]
    public async Task GetUserCharacters_OnlyReturnsCharacters_ForRequestedUser()
    {
        _context.UserCharacters.AddRange(
            new UserCharacter { UserId = 1, CharacterId = 1, Selected = true },
            new UserCharacter { UserId = 2, CharacterId = 2, Selected = true }
        );
        await _context.SaveChangesAsync();

        var result = await _svc.GetUserCharacters(1);

        Assert.All(result, uc => Assert.Equal(1, uc.UserId));
    }

    [Fact]
    public async Task GetUserCharacters_ThrowsKeyNotFoundException_WhenUserHasNoCharacters()
    { 
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _svc.GetUserCharacters(999));
    }

    [Fact]
    public async Task GetSelectedUserCharacter_ReturnsSelectedCharacter()
    {
        _context.UserCharacters.AddRange(
            new UserCharacter { UserId = 1, CharacterId = 1, Selected = false },
            new UserCharacter { UserId = 1, CharacterId = 2, Selected = true }
        );
        await _context.SaveChangesAsync();

        var result = await _svc.GetSelectedUserCharacter(1);

        Assert.Equal(2, result.CharacterId);
        Assert.True(result.Selected);
    }

    [Fact]
    public async Task GetSelectedUserCharacter_IncludesCharacterNavigation()
    {
        _context.UserCharacters.Add(new UserCharacter { UserId = 1, CharacterId = 1, Selected = true });
        await _context.SaveChangesAsync();

        var result = await _svc.GetSelectedUserCharacter(1);

        Assert.NotNull(result.Character);
        Assert.Equal("dog", result.Character.Name);
    }

    [Fact]
    public async Task GetSelectedUserCharacter_ThrowsKeyNotFoundException_WhenNoSelectedCharacter()
    {
        _context.UserCharacters.Add(new UserCharacter { UserId = 1, CharacterId = 1, Selected = false });
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _svc.GetSelectedUserCharacter(1));
    }

    [Fact]
    public async Task GetSelectedUserCharacter_ThrowsKeyNotFoundException_WhenUserDoesNotExist()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _svc.GetSelectedUserCharacter(999));
    }

    [Fact]
    public async Task SetSelectedCharacter_SetsCorrectCharacterToSelected()
    {
        _context.UserCharacters.AddRange(
            new UserCharacter { UserId = 1, CharacterId = 1, Selected = true },
            new UserCharacter { UserId = 1, CharacterId = 2, Selected = false }
        );
        await _context.SaveChangesAsync();

        await _svc.SetSelectedCharacter(userId: 1, characterId: 2, palette: "fire");

        var selected = await _context.UserCharacters
            .Where(uc => uc.UserId == 1 && uc.Selected)
            .ToListAsync();

        Assert.Single(selected);
        Assert.Equal(2, selected[0].CharacterId);
    }

    [Fact]
    public async Task SetSelectedCharacter_DeselectsPreviousCharacter()
    {
        _context.UserCharacters.AddRange(
            new UserCharacter { UserId = 1, CharacterId = 1, Selected = true },
            new UserCharacter { UserId = 1, CharacterId = 2, Selected = false }
        );
        await _context.SaveChangesAsync();

        await _svc.SetSelectedCharacter(userId: 1, characterId: 2, palette: "ice");

        var previouslySelected = await _context.UserCharacters
            .FirstAsync(uc => uc.UserId == 1 && uc.CharacterId == 1);

        Assert.False(previouslySelected.Selected);
    }

    [Fact]
    public async Task SetSelectedCharacter_SetsPaletteOnSelectedCharacter()
    {
        _context.UserCharacters.Add(new UserCharacter { UserId = 1, CharacterId = 1, Selected = true });
        await _context.SaveChangesAsync();

        await _svc.SetSelectedCharacter(userId: 1, characterId: 1, palette: "shadow");

        var character = await _context.UserCharacters
            .FirstAsync(uc => uc.UserId == 1 && uc.CharacterId == 1);

        Assert.Equal("shadow", character.Palette);
    }

    [Fact]
    public async Task SetSelectedCharacter_DoesNotAffectOtherUsers()
    {
        _context.UserCharacters.AddRange(
            new UserCharacter { UserId = 1, CharacterId = 1, Selected = true },
            new UserCharacter { UserId = 2, CharacterId = 2, Selected = true }
        );
        await _context.SaveChangesAsync();

        await _svc.SetSelectedCharacter(userId: 1, characterId: 1, palette: "wind");

        var otherUserCharacter = await _context.UserCharacters
            .FirstAsync(uc => uc.UserId == 2 && uc.CharacterId == 2);

        Assert.True(otherUserCharacter.Selected);
    }

    [Fact]
    public async Task SetSelectedCharacter_OnlyOneCharacterSelectedAfterUpdate()
    {
        _context.UserCharacters.AddRange(
            new UserCharacter { UserId = 1, CharacterId = 1, Selected = true },
            new UserCharacter { UserId = 1, CharacterId = 2, Selected = false },
            new UserCharacter { UserId = 1, CharacterId = 3, Selected = false }
        );
        await _context.SaveChangesAsync();

        await _svc.SetSelectedCharacter(userId: 1, characterId: 3, palette: "default");

        var selectedCount = await _context.UserCharacters
            .CountAsync(uc => uc.UserId == 1 && uc.Selected);

        Assert.Equal(1, selectedCount);
    }
}
