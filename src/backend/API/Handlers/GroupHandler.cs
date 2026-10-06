/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace API.Handlers;

public interface IGroupHandler
{
    (bool ok, string? error) ValidateGroupName(string name);
    (bool ok, string? error) ValidateAddMember(bool userExists, bool groupExists, bool alreadyMember);
}

public class GroupHandler : IGroupHandler
{
    /// <summary>
    /// Validates that a group name is non‑empty and well‑formed.
    /// </summary>
    public (bool ok, string? error) ValidateGroupName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Group name cannot be empty");

        return (true, null);
    }

    /// <summary>
    /// Validates whether a user can be added to a group.
    /// </summary>
    public (bool ok, string? error) ValidateAddMember(bool userExists, bool groupExists, bool alreadyMember)
    {
        if (!userExists)
            return (false, "User not found");

        if (!groupExists)
            return (false, "Group not found");

        if (alreadyMember)
            return (false, "User already in group");

        return (true, null);
    }
}
