/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.EntityFrameworkCore;
using Models;

namespace AA.StampAlgorithm;

/// <summary>
/// This stamping algorithm assumes the passport is a list of pages and each stamp has the same width and height.
/// each being a grid of <see cref="_passportWidth">width</see> X <see cref="_passportHeight">height</see> cells.
/// The algorithm assumes a stamp has a width of <see cref="_stampWidth"/> and a height of <see cref="_stampHeight"/>
/// It also constraints each page on the passport to have less or equal than <see cref="_maxStamps"/> stamps
/// When adding a stamp, it finds the first available page in which still has space for stamps.
/// Then it tries to put it in a random position on the page, if not possible, it goes to the next page
/// until it reaches a new page. 
/// </summary>
/// <remarks>
/// This algorithm is designed for simplicity and simple grids. It will get out of hand when using pixel-amount-like sizes
/// </remarks>
[Algorithm("Simple stamp")]
public class SimpleStampAlgorithm(AppDbContext context) : IStampAlgorithm
{
    private static int _passportWidth = 4;
    private static int _passportHeight = 8;
    private static int _stampWidth = 2;
    private static int _stampHeight = 2;
    private static int _maxStamps = 8;

    /// <summary>
    /// Returns all the cells in the grid as tuples of (x, y), ordered randomly.
    /// </summary>
    /// <returns></returns>
    private List<(int, int)> GenerateRandomPositions()
    {
        var widthRange = Enumerable.Range(0, _passportWidth / _stampWidth);  
        var heightRange = Enumerable.Range(0, _passportHeight / _stampHeight); 

        return widthRange
            .SelectMany(x => heightRange.Select(y => (x * _stampWidth, y * _stampHeight)))
            .OrderBy(_ => Random.Shared.Next())
            .ToList();
    }
    
    /// <summary>
    /// This method can be divided in multiple steps.
    /// <ul>
    /// <li>Fetch all the <see cref="BadgeProgress"/> entries from the user.</li>
    /// <li>Filter out the page numbers which have still room for a stamp, put them in a queue.</li>
    /// <li>For each available page, for each available cell ordered randomly
    /// <ul>
    ///     <li>Check whether the cell is still available <see cref="IsFreeForStamp"/>,
    ///         based on the stamps on this page</li>
    ///     <li>If so, create the stamp object and return.</li>
    ///     <li>Otherwise, go to next cell</li>
    /// </ul>
    /// </li>
    /// <li>If there are no cells available for any page, do the same for a fresh page.</li>
    /// </ul>
    /// </summary>
    /// <param name="userId">The user for which to add a stamp</param>
    /// <param name="stampPath">Unused for this implementation</param>
    /// <returns>The <see cref="BadgeStamp"/> entity which just got created</returns>
    /// <remarks>Does not directly add to the database, only returns the object</remarks>
    /// <remarks>DateAccomplished will not be set!</remarks>
    public async Task<BadgeStamp> FindNewStampPosition(int userId, string? stampPath = null)
    {
        List<(int, int)> positions = GenerateRandomPositions();
        ICollection<BadgeStamp> allStamps = await GetStamps(userId);
        Queue<int> pages = new Queue<int>(AvailablePages(allStamps));

        while (true)
        {
            int page = pages.Count >= 1 ? pages.Dequeue() : NewPageAsync(allStamps);
            List<BadgeStamp> pageStamps = PageStamps(allStamps, page);

            foreach (var (x, y) in positions)
            {
                if (!IsFreeForStamp(x, y, pageStamps)) continue;

                return new BadgeStamp
                {
                    Page = page, X = x, Y = y,
                };
            }
        }
    }

    /// <summary>
    /// Get all the <see cref="BadgeProgress"/> entries of a user which already have a stamp
    /// </summary>
    /// <param name="userId">The user for which to fetch the entries</param>
    /// <returns></returns>
    private async Task<ICollection<BadgeStamp>> GetStamps(int userId)
    {
        return await context.BadgeProgresses
            .Include(p => p.Stamp)
            .Where(p => p.UserId == userId && p.Stamp != null)
            .Select(p => p.Stamp!)
            .ToListAsync();
    }

    /// <summary>
    /// Get all the stamp entries on a specific page.
    /// </summary>
    /// <param name="stamps">The stamp entries from which it will start filtering</param>
    /// <param name="page">The desired page to get the stamps from</param>
    private List<BadgeStamp> PageStamps(ICollection<BadgeStamp> stamps, int page)
    {
        return stamps
            .Where(s => s.Page == page)
            .ToList();
    }

    /// <summary>
    /// Returns a list of page numbers which do not have more than <see cref="_maxStamps"/> amount of stamps
    /// </summary>
    private IEnumerable<int> AvailablePages(IEnumerable<BadgeStamp> stamps)
    {
        return stamps
            .GroupBy(s => s.Page)
            .Where(g => g.Count() < _maxStamps)
            .OrderBy(g => g.Key)
            .Select(g => g.Key);
    }
    
    /// <param name="x">The x position of the new stamp</param>
    /// <param name="y">The y position of the new stamp</param>
    /// <param name="stamps">The already existing stamps</param>
    /// <returns>Whether the new stamp fits on the page without colliding with other existing stamps</returns>
    /// <remarks>Assumes all stamps have with <see cref="_stampWidth"/> and height <see cref="_stampHeight"/></remarks>
    private bool IsFreeForStamp(int x, int y, List<BadgeStamp> stamps)
    {
        return !stamps.Any(s =>
            s.X < x + _stampWidth && s.X + _stampWidth > x &&
            s.Y < y + _stampHeight && s.Y + _stampHeight > y
        );
    }

    /// <summary>
    /// Looks at what the highest page number is and returns its value +1
    /// </summary>
    /// <param name="progresses">The progress entries to start filtering from</param>
    /// <remarks>Pages are 0-indexed, if there are no pages at all, this method returns 0</remarks>
    private int NewPageAsync(ICollection<BadgeStamp> progresses)
    {
        int maxPage;
        try
        {
            maxPage = progresses.Max(s => s.Page);
        }
        // sequence contains no elements
        catch (InvalidOperationException) { maxPage = -1; }

        return maxPage + 1;
    }
}