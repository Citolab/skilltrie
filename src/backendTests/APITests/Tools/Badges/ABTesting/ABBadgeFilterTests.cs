/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers;
using API.Services;
using API.Tools.Badges.ABTesting;
using Models;
using Moq;

namespace APITests.Tools.Badges.ABTesting;

public class ABBadgeFilterTests
{
    private readonly Mock<IAbTestingService> _service = new();
    private readonly int _userId = 5;

    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    [InlineData("C")]
    public async Task FilterABTesting_ShouldOnlyShowCorrectVariant(string variant)
    {
        List<Badge> badges =
        [
            CreateBadge("BadgeNone", null, null),
            CreateBadge("BadgeA", "TestFlag", "A"), 
            CreateBadge("BadgeB", "TestFlag", "B"),
            CreateBadge("BadgeC", "TestFlag", "C")
        ];
        _service
            .Setup(s => s.GetFlagAsync(_userId, "TestFlag"))
            .ReturnsAsync(new FlagResult(true, variant, null));

        badges = await badges.FilterABTesting(b => b.BadgeState, _userId, _service.Object);
        
        Assert.Equal(
            ["BadgeNone", "Badge" + variant], 
            badges.Select(b => b.Identifier)
        );
    }
    
    [Fact]
    public async Task FilterABTesting_ShouldAlwaysIncludeIfFlagDoesNotExist()
    {
        List<Badge> badges =
        [
            CreateBadge("BadgeNone", null, null),
            CreateBadge("BadgeA", "TestFlag", "A"), 
            CreateBadge("BadgeB", "TestFlag", "B"),
            CreateBadge("BadgeC", "TestFlag", "C")
        ];
        _service
            .Setup(s => s.GetFlagAsync(_userId, "TestFlag"))
            .ReturnsAsync((FlagResult?)null);

        badges = await badges.FilterABTesting(b => b.BadgeState, _userId, _service.Object);
        
        Assert.Equal(
            ["BadgeNone", "BadgeA", "BadgeB", "BadgeC"], 
            badges.Select(b => b.Identifier)
        );
    }
    
    [Fact]
    public async Task FilterABTesting_ShouldAlwaysIncludeIfFlagKeyOrVariantIsNull()
    {
        List<Badge> badges =
        [
            CreateBadge("BadgeNone", null, null),
            CreateBadge("BadgeA", "TestFlag", null), 
            CreateBadge("BadgeB", null, "B"),
        ];
        _service
            .Setup(s => s.GetFlagAsync(_userId, "TestFlag"))
            .ReturnsAsync(new FlagResult(true, "A", null));

        badges = await badges.FilterABTesting(b => b.BadgeState, _userId, _service.Object);
        
        Assert.Equal(
            ["BadgeNone", "BadgeA", "BadgeB"], 
            badges.Select(b => b.Identifier)
        );
    }
    
    /// <summary>
    /// This means that the flag is invalid for badges. A possible reason could be that it is not a multivariate badge.
    /// </summary>
    [Fact]
    public async Task FilterABTesting_ShouldAlwaysIncludeIfServiceReturnsEmptyVariant()
    {
        List<Badge> badges =
        [
            CreateBadge("BadgeNone", null, null),
            CreateBadge("BadgeA", "TestFlag", "A"), 
            CreateBadge("BadgeB", "TestFlag", "B"),
        ];
        _service
            .Setup(s => s.GetFlagAsync(_userId, "TestFlag"))
            .ReturnsAsync(new FlagResult(true, null, null));

        badges = await badges.FilterABTesting(b => b.BadgeState, _userId, _service.Object);
        
        Assert.Equal(
            ["BadgeNone", "BadgeA", "BadgeB"], 
            badges.Select(b => b.Identifier)
        );
    }
    
    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    public async Task FilterABTesting_ShouldPerformLambdaOnAllKindsOfObjects(string variant)
    {
        List<int> badges = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        _service
            .Setup(s => s.GetFlagAsync(_userId, "TestFlag"))
            .ReturnsAsync(new FlagResult(true, variant, null));

        Dictionary<string, List<int>> expected = new Dictionary<string, List<int>>
        {
            { "A", [2, 4, 6, 8, 10] },
            { "B", [1, 3, 5, 7, 9] },
        };

        badges = await badges.FilterABTesting(
            b => b % 2 == 0 
                ? new BadgeState {FlagKey = "TestFlag", FlagVariant = "A"}
                : new BadgeState {FlagKey = "TestFlag", FlagVariant = "B"}, 
            _userId, _service.Object);
        
        Assert.Equal(
            expected[variant],
            badges
        );
    }

    private Badge CreateBadge(string identifier, string? flagKey, string? flagVariant)
    {
        return new Badge
        {
            Identifier = identifier,
            BadgeState = new BadgeState
            {
                FlagKey = flagKey,
                FlagVariant = flagVariant
            }
        };
    }
}