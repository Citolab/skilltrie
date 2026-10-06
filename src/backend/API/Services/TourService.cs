/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Services;

public interface ITourService
{
    bool GetTourSeen(int userId, string tourKey);
    void MarkTourSeen(int userId, string tourKey);
}

public class TourService(AppDbContext db) : ITourService
{
    /// <summary>
    /// Returns whether the user has seen the passed Tour
    /// </summary>
    /// <param name="userId">The currently logged in User</param>
    /// <param name="tourKey">The key identifying the tour</param>
    /// <returns>A boolean representing if the tour has been seen</returns>
    public bool GetTourSeen(int userId, string tourKey)
    {
        return db.UserTours
            .Any(t => t.UserId == userId && t.TourKey == tourKey);
    }

    /// <summary>
    /// A function to add a UserId TourKey pair into the database, representing if the tour is already seen. 
    /// </summary>
    /// <param name="userId">The currently logged in User</param>
    /// <param name="tourKey">The key identifying the tour</param>
    public void MarkTourSeen(int userId, string tourKey)
    {
        bool already = db.UserTours
            .Any(t => t.UserId == userId && t.TourKey == tourKey);

        if (already) return;

        db.UserTours.Add(new UserTour
        {
            UserId = userId,
            TourKey = tourKey,
        });
        db.SaveChanges();
    }
}