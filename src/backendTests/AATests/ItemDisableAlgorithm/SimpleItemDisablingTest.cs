/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Reflection;
using AA.ItemDisableAlgorithm;
using Models;
using Xunit;

namespace AATests.ItemDisableAlgorithm;

public class SimpleItemDisablingTest
{
    private readonly AppDbContext _db;
    private readonly SimpleItemValidity _algorithm;

    public SimpleItemDisablingTest()
    {
        _db = SqliteInMemoryContextFactory.Create();
        _algorithm = new SimpleItemValidity(_db);
    }
    
    private void SetReportCountThreshold(int threshold)
    {
        FieldInfo? reportCountThresholdField = typeof(SimpleItemValidity).GetField(
            "_reportCountThreshold", 
            BindingFlags.NonPublic | BindingFlags.Instance
        );
        if (reportCountThresholdField == null) 
            throw new Exception("SimpleItemDisabling was expected to have a field called '_reportCountThreshold'");
        reportCountThresholdField.SetValue(_algorithm, threshold);
    }

    [Fact]
    public async Task AssesItemValidity_ShouldDeactivateWhenAboveThreshold()
    {
        int itemId = 10;
        SetReportCountThreshold(5);
        AddItem(itemId);
        await AddReportsForItem(itemId, count: 5);
        Assert.False(await _algorithm.AssesItemValidity(itemId));
    }
    
    [Fact]
    public async Task AssesItemValidity_ShouldNotDeactivateWhenAboveThreshold()
    {
        int itemId = 10;
        SetReportCountThreshold(5);
        AddItem(itemId);
        await AddReportsForItem(itemId, count: 4);
        Assert.True(await _algorithm.AssesItemValidity(itemId));
    }

    private void AddItem(int itemId)
    {
        _db.Items.Add(new Item
        {
            Id = itemId,
            QuestionText = "q",
            ResponseType = "r",
        });

        _db.SaveChangesAsync();
    }

    private async Task AddReportsForItem(int itemId, int count = 1)
    {
        int itemErrorCount = Enum.GetNames(typeof(ItemError)).Length;
        for (int i = 0; i < count; i++)
        {
            await _db.Users.AddAsync(CreateUser(i + 1)); // Apparently User with id 0 is already tracked from the beginning.
            await _db.Reports.AddAsync(new Report
            {
                ItemId = itemId,
                ItemError = (ItemError)Random.Shared.Next(1, itemErrorCount),
                UserId = i + 1
            });
        }

        await _db.SaveChangesAsync();
    }
    
    private User CreateUser(int id)
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