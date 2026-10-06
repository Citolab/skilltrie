/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Dynamic;

namespace API.Handlers;


/// <summary>
/// A dynamic wrapper over a deserialized JSON payload that provides safe dot-access.
/// This is to make sure missing or changed payloads on posthog can't cause a RunTimeBinderException.
/// </summary>
public class SafePayload : DynamicObject
{
    private readonly IDictionary<string, object?> _data;

    public SafePayload(ExpandoObject? expando)
    {
        _data = expando as IDictionary<string, object?> 
             ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Intercepts dot-access (e.g. payload.someValue).
    /// Returns null if the property does not exist rather than throwing.
    /// Nested ExpandoObjects are wrapped in a new <see cref="SafePayload"/> automatically.
    /// </summary>
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (_data.TryGetValue(binder.Name, out var value))
        {
            // Wrap nested objects so chained access is also safe
            result = value is ExpandoObject nested
                ? new SafePayload(nested)
                : value;
            return true;
        }

        // Return null for missing keys instead of throwing
        result = null;
        return true; // returning true prevents RuntimeBinderException
    }
}

/// <summary>
/// The complete and generic result of a feature flag. 
/// Variant and Payload are both optional, but we at least view if it is enabled. 
/// Created by the GetFeatureFlag method in AbTestingService.cs 
/// </summary>
public record FlagResult(
    bool IsEnabled,
    string? Variant,        // null for boolean flags
    dynamic? Payload);      // null for flags with no payload configured

/// <summary>
/// An AbTest helper function to run some C# action inside of an AbTest wrapper. 
/// This makes it simple to call an AbTest, see also the documentation.
/// </summary>
public static class AbTest
{
    /// <summary>
    /// Standard Run function, simply performs a synchronous Action, where it also uses data from the flag for that action.  
    /// </summary>
    /// <param name="flag">The flag</param>
    /// <param name="action">The action to perform, in a parameterised lambda</param>
    public static void Run(FlagResult? flag, Action<FlagResult> action)
    {
        if (flag is { IsEnabled: true }) action(flag);
    }

    /// <summary>
    /// Standard Run function that can also return a value to be used later. Uses flag data as well. 
    /// This also allows this return value to pass on errors or null in case no flag is found, to easily define
    /// default bevahiour. 
    /// </summary>
    /// <typeparam name="TResult">The type to return</typeparam>
    /// <param name="flag">The flag</param>
    /// <param name="action">The action to perform, in a parameterised lambda</param>
    /// <returns>Whatever result the action does</returns>
    public static TResult? Run<TResult>(FlagResult? flag, Func<FlagResult, TResult> action)
        => flag is { IsEnabled: true } ? action(flag) : default;

    /// <summary>
    /// As the non-returning flag using Run, just async. 
    /// </summary>
    /// <param name="flag">The flag</param>
    /// <param name="action">The action to perform, in a parameterised lambda</param>
    public static async Task RunAsync(FlagResult? flag, Func<FlagResult, Task> action)
    {
        if (flag is { IsEnabled: true }) await action(flag);
    }


    /// <summary>
    /// As the returning flag-using Run, just async. 
    /// </summary>
    /// <typeparam name="TResult">The type to return</typeparam>
    /// <param name="flag">The flag</param>
    /// <param name="action">The action to perform, in a parameterised lambda</param>
    /// <returns>Whatever return the action does</returns>
    public static async Task<TResult?> RunAsync<TResult>(FlagResult? flag, Func<FlagResult, Task<TResult>> action)
        => flag is { IsEnabled: true } ? await action(flag) : default;
}