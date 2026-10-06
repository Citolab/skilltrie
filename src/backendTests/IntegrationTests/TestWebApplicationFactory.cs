/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using AI;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using API.Tools;

namespace IntegrationTests;

/// <summary>
/// Class to create the <see cref="WebApplicationFactory"/> with an in memory sql database and own authentication handler
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public TestWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    /// <summary>
    /// Creates a new <see cref="AppDbContext">
    /// </summary>
    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new AppDbContext(options);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(AppDbContext));
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddAuthentication(TestAuthConstants.Scheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthConstants.Scheme, _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthConstants.Scheme;
                options.DefaultChallengeScheme = TestAuthConstants.Scheme;
            });

            services.RemoveAll<IAIAPI>();
            services.AddScoped<IAIAPI, MockAIAPI>();

            DateTime epochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            services.RemoveAll<TimeTool>();
            services.AddSingleton(new TimeTool(timeProvider: new MockTimeProvider(epochTime)));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _connection?.Dispose();
        base.Dispose(disposing);
    }
}

/// <summary>
/// Class to create a new HttpClient with an authenticated user making the request
/// </summary>
public static class TestClientExtensions
{
    public static HttpClient CreateClientAs(
        this TestWebApplicationFactory factory,
        int userId,
        string email,
        params string[] roles)
    {
        var client = factory.CreateClient();

        var roleString = string.Join(",", roles);

        client.DefaultRequestHeaders.Add(
            TestAuthConstants.Header,
            $"{userId}|{email}|{roleString}");

        return client;
    }
}
