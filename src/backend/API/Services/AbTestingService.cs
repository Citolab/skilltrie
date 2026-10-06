/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
using System.Dynamic;
using System.Text.Json;
using API.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PostHog;
using PostHog.Features;

namespace API.Services;

/// <summary>
/// Abstraction over an A/B testing provider.
/// Resolves feature flags for a given user, returning a <see cref="FlagResult"/> that contains
/// the enabled state, variant key, and optional dynamic payload in a single call.
/// </summary>
public interface IAbTestingService
{
    /// <summary>
    /// Fetches the flag from some abstract implementation. 
    /// </summary>
    /// <param name="userId">Our userId as stored in db</param>
    /// <param name="flagKey">A string of feature flag we are using</param>
    /// <returns>A <see cref="FlagResult"/> of the resolved flag</returns>
    Task<FlagResult?> GetFlagAsync(int userId, string flagKey);
}

/// <summary>
/// Cheeky wrapper to make mocking possible.
/// </summary>
public interface IPostHogClientWrapper
{
    Task<FeatureFlag?> GetFeatureFlagAsync(string flagKey, string distinctId);
}

/// <summary>
/// Implementation of the cheeky wrapper to make mocking possible
/// </summary>
/// <param name="postHog"></param>
public class PostHogClientWrapper(IPostHogClient postHog) : IPostHogClientWrapper
{
    public Task<FeatureFlag?> GetFeatureFlagAsync(string flagKey, string distinctId)
        => postHog.GetFeatureFlagAsync(flagKey, distinctId);
}

/// <summary>
/// PostHog-backed implementation of <see cref="IAbTestingService"/>.
/// Wraps the PostHog .NET SDK, mapping its <c>FeatureFlag</c> response into a <see cref="FlagResult"/>.
/// </summary>
public class PostHogAbTestingService(IPostHogClientWrapper postHog) : IAbTestingService
{
    /// <summary>
    /// Fetches the flag from the Posthog .NET SDK 
    /// </summary>
    /// <param name="userId">Our userId as stored in db</param>
    /// <param name="flagKey">A string of feature flag we are using</param>
    /// <returns>A <see cref="FlagResult"/> of the resolved flag</returns>
    public async Task<FlagResult?> GetFlagAsync(int userId, string flagKey)
    {
        try
        {
            var flag = await postHog.GetFeatureFlagAsync(flagKey, userId.ToString());
            if (flag is null) return null;

            return new FlagResult(
                IsEnabled: flag.IsEnabled,
                Variant:   flag.VariantKey,
                Payload:   DeserializePayload(flag.Payload));
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync($"{e.Message}, PostHog failed to evaluate flag '{flagKey}' for user {userId}");
            return null;
        }
    }
    
    /// <summary>
    /// Deserializes a raw <see cref="JsonDocument"/> payload into a <see cref="ExpandoObject"/>  />,
    /// enabling dot-access on payload properties at the call site (e.g. <c>f.Payload.someParameter</c>).
    /// Returns <c>null</c> if no payload is present.
    /// </summary>
    private static SafePayload? DeserializePayload(JsonDocument? payload)
    {
        if (payload is null) return null;
        var expando = JsonConvert.DeserializeObject<ExpandoObject>(payload.RootElement.GetRawText(), new ExpandoObjectConverter());
        return new SafePayload(expando);
    }
}