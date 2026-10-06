/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Handlers;

public class UserHandler
{
    /// <summary>
    /// Create a user from a <see cref="UserCreateRecord"/>
    /// </summary>
    /// <param name="ucr">The <see cref="UserCreateRecord"/> with which to create a User </param>
    /// <returns>The <see cref="User"/> to be created.</returns>
    public User CreateUserFromRecord(UserCreateRecord ucr)
    {
        return new User()
        {
            FirstName = ucr.FirstName,
            Infix = ucr.Infix,
            LastName = ucr.LastName,
            DisplayName = ucr.DisplayName,
            Email = ucr.Email,
            UserName = ucr.DisplayName
        };
    }

    /// <summary>
    /// Update a <see cref="User"/> with a <see cref="UserCreateRecord"/>
    /// </summary>
    /// <param name="user">The <see cref="User"/> to be updated</param>
    /// <param name="ucr"> The <see cref="UserCreateRecord"/> to be used to update the <see cref="User"/></param>
    public void UpdateUserFromRecord(User user, UserCreateRecord ucr)
    {
        user.FirstName = ucr.FirstName;
        user.Infix = ucr.Infix;
        user.LastName = ucr.LastName;
        user.DisplayName = ucr.DisplayName;
        user.Email = ucr.Email;
        user.UserName = ucr.DisplayName;
    }

    /// <summary>
    /// Create a default <see cref="UserStreak"/> for a <see cref="User"/>
    /// </summary>
    /// <param name="user">The <see cref="User"/> for whom to create the <see cref="UserStreak"/></param>
    /// <returns>The default <see cref="UserStreak"/> for a new <see cref="User"/></returns>
    public UserStreak CreateUserStreak(User user)
    {
        return new UserStreak()
        {
            UserId = user.Id,
            CurrentStreak = 0,
            HighestStreak = 0,
            LastDayCompleted = DateTime.MinValue
        };
    }
}
