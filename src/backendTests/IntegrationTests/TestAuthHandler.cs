/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace IntegrationTests;

public static class TestAuthConstants
{
    public const string Scheme = "Test";
    public const string Header = "Test-User";
}

/// <summary>
/// Authentication handler to handle the authentication during tests
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(TestAuthConstants.Header, out var header))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing test user header"));
        }

        // format: "userId|email|role1,role2"
        var parts = header.ToString().Split('|');

        var userId = parts.ElementAtOrDefault(0) ?? "test-id";
        var email = parts.ElementAtOrDefault(1) ?? "test@test.com";
        var roles = parts.ElementAtOrDefault(2)?.Split(',') ?? [];

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, email)
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, TestAuthConstants.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestAuthConstants.Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}