/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Diagnostics;

namespace DatabasePopulator;
#pragma warning disable EF1002

class Program
{
    static async Task Main(string[] args)
    {
        Env.Load(GetAbsolutePath("../.env"));

        string dbHost = Environment.GetEnvironmentVariable("BACKEND_DB_HOST")
            ?? throw new Exception("BACKEND_DB_HOST not set");
        string dbPort = Environment.GetEnvironmentVariable("BACKEND_DB_PORT")
            ?? throw new Exception("BACKEND_DB_PORT not set");
        string dbDatabaseName = Environment.GetEnvironmentVariable("BACKEND_DB_DATABASENAME")
            ?? throw new Exception("BACKEND_DB_DATABASENAME not set");
        string dbUserName = Environment.GetEnvironmentVariable("BACKEND_DB_USERNAME")
            ?? throw new Exception("BACKEND_DB_USERNAME not set");
        string dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
            ?? throw new Exception("POSTGRES_PASSWORD not set");

        string dbConnectionString = $"Host={dbHost};Port={dbPort};Database={dbDatabaseName};Username={dbUserName};Password={dbPassword}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dbConnectionString)
            .Options;

        using (AppDbContext context = new(options))
        {
            // truncate existing tables if user desires
            await AskToExecute("Delete any existing items from items, answers, scope tables, cosmetics before importing? (recommended if importing)", async () => { await Truncate(context); });

            // read and store all imported data
            await AskToExecute("Import the input files?", async () => { await new InputParser().Parse(context); });

            // verify the current database
            await AskToExecute("Verify the current database?", async () => { await new InputVerifier().Verify(context); });

            // update the remainder of the database (such as user proficiencies)
            await AskToExecute("Update the remainder of the database?", async () => { await new InputUpdater().Update(context); });
        }
    }

    private static async Task AskToExecute(string request, Func<Task> task)
    {
        while (true)
        {
            Write($"{request} (Y/N): ");

            string? input = Console.ReadLine();

            if (input?.Trim().ToLower() == "y")
            {
                await task();

                break;
            }
            else if (input?.Trim().ToLower() == "n")
                break;
            else
                Write("Invalid input, please try again...");
        }
    }
    private static async Task Truncate(DbContext context)
    {
        Status("\nTruncating tables...");

        // GetTableName() for model returns first letter lowercase -> will fail -> reverted this to hardcorded
        await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"Item\" RESTART IDENTITY CASCADE;");
        await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"Scope\" RESTART IDENTITY CASCADE;");
        await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"Character\" RESTART IDENTITY CASCADE;");
        await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"Cosmetic\" RESTART IDENTITY CASCADE;");

        Success("Tables successfully truncated\n");
    }

    // quick hack to account for different working directories when debugging / not debugging
    // without explicitly configuring default working dir (nightmare to config)
    internal static string GetAbsolutePath(string path, params string[] paths)
    {
        string absolutePath;

        if (Debugger.IsAttached)
            absolutePath = Path.GetFullPath("../../../" + path);
        else
            absolutePath = Path.GetFullPath(path);

        if (path.Length > 0)
            absolutePath = Path.Combine([absolutePath, .. paths]);

        return absolutePath;
    }

    // Functions to simplify writing to the console.
    // You can comment out the content of Program.Error to disable all the errors while parsing
    private static void Write(string message, ConsoleColor consoleColor = ConsoleColor.White)
    {
        Console.ForegroundColor = consoleColor;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    internal static void Status(string message)
    {
        Write(message, ConsoleColor.Yellow);
    }
    internal static void Success(string message)
    {
        Write(message, ConsoleColor.Green);
    }
    internal static void Error(Exception exception)
    {
        Write($"Error: {exception.Message}", ConsoleColor.Red);
    }

    // Helper function to easily navigate through ScopeEdge to get scopes to the left in the tree
    internal static IQueryable<Scope> GetFromScopes(AppDbContext context, Scope toScope)
    {
        return (
            from se in context.ScopeEdges
            where se.ToScopeId == toScope.Id
            select se.FromScope
            );
    }

    // Helper function to easily navigate through ScopeEdge to get scopes to the right in the tree
    internal static IQueryable<Scope> GetToScopes(AppDbContext context, Scope fromScope)
    {
        return (
            from se in context.ScopeEdges
            where se.FromScopeId == fromScope.Id
            select se.ToScope
            );
    }

    // Helper function to easily navigate through ScopeMembership to get ancestors
    internal static IQueryable<Scope> GetAncestors(AppDbContext context, Scope descendant)
    {
        return (
            from se in context.ScopeMemberships
            where se.DescendantId == descendant.Id
            select se.Ancestor
            );
    }

    // Helper function to easily navigate through ScopeMembership to get descendants
    internal static IQueryable<Scope> GetDescendants(AppDbContext context, Scope ancestor)
    {
        return (
            from se in context.ScopeMemberships
            where se.AncestorId == ancestor.Id
            select se.Descendant
            );
    }
}
