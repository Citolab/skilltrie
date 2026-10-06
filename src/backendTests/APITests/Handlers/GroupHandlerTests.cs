/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers;
using Xunit;

namespace APITests.Handlers;

public class GroupHandlerTests
{
    private readonly GroupHandler handler = new();

    [Fact]
    public void ValidateGroupName_Fails_WhenEmpty()
    {
        var (ok, error) = handler.ValidateGroupName("");
        Assert.False(ok);
        Assert.Equal("Group name cannot be empty", error);
    }

    [Fact]
    public void ValidateGroupName_Fails_WhenWhitespace()
    {
        var (ok, error) = handler.ValidateGroupName("   ");
        Assert.False(ok);
        Assert.Equal("Group name cannot be empty", error);
    }

    [Fact]
    public void ValidateGroupName_Succeeds_WhenValid()
    {
        var (ok, error) = handler.ValidateGroupName("Valid Name");
        Assert.True(ok);
        Assert.Null(error);
    }

    [Fact]
    public void ValidateAddMember_Fails_WhenUserMissing()
    {
        var (ok, error) = handler.ValidateAddMember(
            userExists: false,
            groupExists: true,
            alreadyMember: false
        );

        Assert.False(ok);
        Assert.Equal("User not found", error);
    }

    [Fact]
    public void ValidateAddMember_Fails_WhenGroupMissing()
    {
        var (ok, error) = handler.ValidateAddMember(
            userExists: true,
            groupExists: false,
            alreadyMember: false
        );

        Assert.False(ok);
        Assert.Equal("Group not found", error);
    }

    [Fact]
    public void ValidateAddMember_Fails_WhenAlreadyMember()
    {
        var (ok, error) = handler.ValidateAddMember(
            userExists: true,
            groupExists: true,
            alreadyMember: true
        );

        Assert.False(ok);
        Assert.Equal("User already in group", error);
    }

    [Fact]
    public void ValidateAddMember_Succeeds_WhenAllValid()
    {
        var (ok, error) = handler.ValidateAddMember(
            userExists: true,
            groupExists: true,
            alreadyMember: false
        );

        Assert.True(ok);
        Assert.Null(error);
    }
}
