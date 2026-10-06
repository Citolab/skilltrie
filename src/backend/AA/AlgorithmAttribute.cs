/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace AA;

/// <summary>
/// An attribute with which to tag algorithms so that the Algorithm Factory automatically detects them using reflection. 
/// </summary>
/// <param name="key">The Name of the algorithm the Algorithm Factory can use to access it.</param>
[AttributeUsage(AttributeTargets.Class)]
public class Algorithm(string key) : Attribute
{
    public string Key { get; } = key;
}