/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers;
using Models;

namespace APITests.Handlers;

public class UserHandlerTest
{
    private readonly UserHandler _userHandler = new UserHandler();

    [Fact(DisplayName = "CreateUserFromRecord correctly returns a User on valid inputs")]
    public void CreateUserFromRecordCorrectly()
    {
        UserCreateRecord ucr = new UserCreateRecord
        {
            FirstName = "hans",
            Infix = " ",
            LastName = "philippi",
            DisplayName = "DeHans1",
            Email = "hans@derhanzi.nl"
        };

        var result = _userHandler.CreateUserFromRecord(ucr);

        Assert.NotNull(result);
        Assert.Equal("hans", result.FirstName);
        Assert.Equal(" ", result.Infix);
        Assert.Equal("philippi", result.LastName);
        Assert.Equal("DeHans1", result.DisplayName);
        Assert.Equal(ucr.Email, result.Email);
        Assert.Equal(ucr.DisplayName, result.UserName);
    }

    [Fact(DisplayName = "UpdateUserFromRecord correctly returns a User on valid inputs")]
    public void UpdateUserFromRecordCorrectly()
    {
        User user = new User
        {
            FirstName = " ",
            Infix = "",
            LastName = " ",
            DisplayName = " ",
            Email = " ",
        };

        UserCreateRecord ucr = new UserCreateRecord
        {
            FirstName = "hans",
            Infix = " ",
            LastName = "philippi",
            DisplayName = "DeHans1",
            Email = "hans@derhanzi.nl"
        };

        _userHandler.UpdateUserFromRecord(user, ucr);


        Assert.Equal(user.FirstName, ucr.FirstName);
        Assert.Equal(user.Infix, ucr.Infix);
        Assert.Equal(user.LastName, ucr.LastName);
        Assert.Equal(user.DisplayName, ucr.DisplayName);
        Assert.Equal(user.Email, ucr.Email);
        Assert.Equal(user.UserName, ucr.DisplayName);
    }

}
