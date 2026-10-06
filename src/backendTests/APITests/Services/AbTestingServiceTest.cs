/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Runtime.CompilerServices;
using System.Text.Json;
using API.Handlers;
using API.Services;
using Moq;
using PostHog;
using PostHog.Features;

namespace APITests.Services;

public class PostHogAbTestingServiceTest
{
    private readonly Mock<IPostHogClientWrapper> _postHogMock;
    private readonly PostHogAbTestingService _testingService;

    public PostHogAbTestingServiceTest()
    {
        _postHogMock = new Mock<IPostHogClientWrapper>();
        _testingService = new PostHogAbTestingService(_postHogMock.Object);
    }

    /// <summary>
    /// Builds a FeatureFlag via the Posthog SDK's JSON deserialization,
    /// </summary>
    private static FeatureFlag MakeFlag(bool enabled, string? variantKey = null, string? payload = null)
    {
        var flag = (FeatureFlag)RuntimeHelpers.GetUninitializedObject(typeof(FeatureFlag));

        typeof(FeatureFlag).GetProperty("IsEnabled")!
            .SetValue(flag, enabled);
        typeof(FeatureFlag).GetProperty("VariantKey")!
            .SetValue(flag, variantKey);
        typeof(FeatureFlag).GetProperty("Payload")!
            .SetValue(flag, payload is not null ? JsonDocument.Parse(payload) : null);

        return flag;
    }
    
    [Fact]
    public async Task GetFlagAsync_PostHogReturnsNull_ReturnsNull()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync("flag", "1"))
            .ReturnsAsync((FeatureFlag?)null);

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "flag");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetFlagAsync_EnabledBooleanFlag_ReturnsEnabledTrue()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync("flag", "1"))
            .ReturnsAsync(MakeFlag(enabled: true));

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "flag");

        Assert.NotNull(result);
        Assert.True(result.IsEnabled);
    }

    [Fact]
    public async Task GetFlagAsync_DisabledBooleanFlag_ReturnsEnabledFalse()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync("flag", "1"))
            .ReturnsAsync(MakeFlag(enabled: false));

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "flag");

        Assert.NotNull(result);
        Assert.False(result.IsEnabled);
    }
    
    [Fact]
    public async Task GetFlagAsync_NoVariant_VariantIsNull()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync("flag", "1"))
            .ReturnsAsync(MakeFlag(enabled: true));

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "flag");

        Assert.Null(result!.Variant);
    }

    [Fact]
    public async Task GetFlagAsync_NoPayload_PayloadIsNull()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync("flag", "1"))
            .ReturnsAsync(MakeFlag(enabled: true));

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "flag");

        Assert.Null(result!.Payload);
    }

    [Fact]
    public async Task GetFlagAsync_VariantFlag_VariantKeyMappedCorrectly()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync("flag", "1"))
            .ReturnsAsync(MakeFlag(enabled: true, variantKey: "control"));

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "flag");

        Assert.Equal("control", result!.Variant);
    }

    [Fact]
    public async Task GetFlagAsync_VariantFlag_IsEnabledTrue()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync("flag", "1"))
            .ReturnsAsync(MakeFlag(enabled: true, variantKey: "control"));

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "flag");

        Assert.True(result!.IsEnabled);
    }

    [Fact]
    public async Task GetFlagAsync_FlagWithPayload_PayloadIsNotNull()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync("payload-flag", "1"))
            .ReturnsAsync(MakeFlag(enabled: true, payload: "{\"proficiency\": 0.1}"));

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "payload-flag");

        Assert.NotNull(result!.Payload);
    }

    [Fact]
    public async Task GetFlagAsync_FlagWithPayload_PayloadPropertyAccessible()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync("flag", "1"))
            .ReturnsAsync(MakeFlag(enabled: true, payload: "{\"proficiency\": 0.1}"));

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "flag");

        dynamic payload = result!.Payload!;
        Assert.Equal(0.1, payload.proficiency);
    }

    [Fact]
    public async Task GetFlagAsync_FlagWithNestedPayload_NestedPropertyAccessible()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync("flag", "1"))
            .ReturnsAsync(MakeFlag(enabled: true, payload: "{\"config\": {\"totalItems\": 10}}"));

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "flag");
        
        Assert.Equal(10, result?.Payload!.config.totalItems);
    }

    [Fact]
    public async Task GetFlagAsync_FlagWithPayload_MissingPropertyReturnsNull()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync("flag", "1"))
            .ReturnsAsync(MakeFlag(enabled: true, payload: "{\"proficiency\": 0.1}"));

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "flag");

        dynamic payload = result!.Payload!;
        Assert.Null(payload.nonExistentKey);
    }
    
    [Fact]
    public async Task GetFlagAsync_PostHogThrows_ReturnsNull()
    {
        _postHogMock
            .Setup(p => p.GetFeatureFlagAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("PostHog unavailable"));

        var result = await _testingService.GetFlagAsync(userId: 1, flagKey: "flag");

        Assert.Null(result);
    }
}