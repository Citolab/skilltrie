/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public static class SqliteInMemoryContextFactory
{
    public static AppDbContext Create()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open(); // critical: keeps DB alive

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated(); // critical: builds schema

        return context;
    }
}
