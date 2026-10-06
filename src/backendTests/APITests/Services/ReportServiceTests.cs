/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Models;
using Moq;
using API.Services;
using API.Handlers;
using API.Handlers.GameEventHandlers;
using API.Tools.EventQueue;
using Microsoft.Extensions.Logging;

namespace APITests.Services;

public class ReportServiceTests
{
    private readonly Mock<IEventQueue> _eventQueue = new ();
    private readonly Mock<IItemService> _itemService;
    private readonly ReportService _repService;
    private readonly AppDbContext _context;

    public ReportServiceTests()
    {
        var (context, _) = CreateSqliteContext();
        _context = context;
        _itemService = new Mock<IItemService>();
        _itemService.Setup(s => s.VerifyItemExists(1)).ReturnsAsync(new Item());
        _repService = new ReportService(_context, _itemService.Object, new ReportHandler(), _eventQueue.Object, Mock.Of<IGameEventManager>(), Mock.Of<ILogger<IReportService>>());
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
        ctx.Users.AddRange(CreateUser(5), CreateUser(6));
        ctx.Items.Add(new Item
        {
            Id = 1,
            QuestionText = "a",
            ResponseType = "a",
        });
        ctx.SaveChanges();

        return (ctx, conn);
    }

    [Fact(DisplayName = "CreateReport: Create valid report on valid inputs")]
    public async Task CreateReport_OnValidInputs()
    {
        await _repService.CreateReport(1, ItemError.QTextEmpty, 5);

        _itemService.Verify(s => s.VerifyItemExists(1), Times.Once);
        VerifyEventQueued(1, 1);
        Assert.Single(_context.Reports);
    }

    [Fact(DisplayName = "CreateReport: Data persisted on report creation")]
    public async Task CreateReport_DataPersisted()
    {
        await _repService.CreateReport(1, ItemError.QTextEmpty, 5);

        var report = Assert.Single(_context.Reports);
        VerifyEventQueued(1, 1);
        Assert.Equal(1, report.ItemId);
        Assert.Equal(ItemError.QTextEmpty, report.ItemError);
    }

    [Fact(DisplayName = "CreateReport: when VerifyItemExists throws, no report is persisted")]
    public async Task CreateReport_ThrownOnNonExistingItem()
    {
        _itemService
            .Setup(s => s.VerifyItemExists(1))
            .ThrowsAsync(new InvalidOperationException("Item not found"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _repService.CreateReport(1, ItemError.QTextEmpty, 5));

        _itemService.Verify(s => s.VerifyItemExists(1), Times.Once);
        VerifyEventQueued(1, 0);
        Assert.Empty(_context.Reports);
    }

    [Fact(DisplayName = "CreateReport: calling twice creates two persisted reports (duplicates allowed when different users report)")]
    public async Task CreateReport_AllowDuplicateReports()
    {
        await _repService.CreateReport(1, ItemError.QTextEmpty, 5);
        await _repService.CreateReport(1, ItemError.QTextEmpty, 6);

        _itemService.Verify(s => s.VerifyItemExists(1), Times.Exactly(2));
        VerifyEventQueued(1, 2);
        Assert.Equal(2, _context.Reports.Count());
    }
    
    [Fact(DisplayName = "CreateReport: calling twice creates two persisted reports (duplicates allowed when same users report different reasons)")]
    public async Task CreateReport_AllowDuplicateReportsWhenReasonsDifferent()
    {
        await _repService.CreateReport(1, ItemError.QTextEmpty, 5);
        await _repService.CreateReport(1, ItemError.GraphicsFaulty, 5);
        await _repService.CreateReport(1, ItemError.GraphicsUnrelated, 5);

        _itemService.Verify(s => s.VerifyItemExists(1), Times.Exactly(3));
        VerifyEventQueued(1, 3);
        Assert.Equal(3, _context.Reports.Count());
    }
    
    [Fact(DisplayName = "CreateReport: calling twice creates one persisted reports (duplicates not allowed when same users report same reason)")]
    public async Task CreateReport_DisallowDuplicateReportsWhenReasonSame()
    {
        await _context.SaveChangesAsync();
        await _repService.CreateReport(1, ItemError.ATextIncorrect, 5);
        await _repService.CreateReport(1, ItemError.ATextIncorrect, 5);
        await _repService.CreateReport(1, ItemError.ATextIncorrect, 5);

        _itemService.Verify(s => s.VerifyItemExists(1), Times.Exactly(3));
        VerifyEventQueued(1, 1);
        Assert.Equal(1, _context.Reports.Count());
    }

    private void VerifyEventQueued(int itemId, int times)
    {
        _eventQueue.Verify(s => s.QueueAsync(It.Is<EventData>(
            e => e.EventType == EventType.ItemReported && 
                 ((Dictionary<string, int>)e.Data).GetValueOrDefault("itemId") == itemId
            ), 
            It.IsAny<CancellationToken>())
        , Times.Exactly(times));
    }
    
    private static User CreateUser(int id)
    {
        return new User
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            DisplayName =  "John Doe",
            UserName = "testuser",
            NormalizedUserName = "TESTUSER" + id,
            Email = "test@test.com",
            NormalizedEmail = "TEST@TEST.COM" + id,
            SecurityStamp = Guid.NewGuid().ToString()
        };
    }
}
