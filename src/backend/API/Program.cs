/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA;
using AI;
using AI.FeedbackGen;
using AI.QuestionGen;
using API.Handlers;
using API.Handlers.EventHandlers;
using API.Handlers.ExceptionHandlers;
using API.Handlers.GameEventHandlers;
using API.Services;
using API.Tools;
using API.Tools.Badges.BadgeImageCollecting;
using API.Tools.Badges.Compilation;
using API.Tools.EventQueue;
using API.Tools.Streak;
using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Models;
using PostHog;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using API.Tools.MathMLConverter;

Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");

// Add services to the container.
builder.AddPostHog();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AI-rate-limiter", o =>
    {
        o.Window = TimeSpan.FromSeconds(7);
        o.PermitLimit = 1;
    });
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
    };
});

builder
    .Services
    .AddControllers(
        options =>
        {
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        }
    )
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddResponseCompression();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (!isTesting)
{
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

    string dbConnection =
        $"Host={dbHost};Port={dbPort};Database={dbDatabaseName};Username={dbUserName};Password={dbPassword}";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(dbConnection, b => b.MigrationsAssembly("API")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("DataSource=:memory:"));
}

builder.Services.AddIdentity<User, ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<UserHandler>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ILevelService, LevelService>();
builder.Services.AddScoped<ItemPoolService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ReportHandler>();
builder.Services.AddScoped<LevelHandler>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IGroupHandler, GroupHandler>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<TopicHandler>();
builder.Services.AddScoped<IAbTestingService, PostHogAbTestingService>();
builder.Services.AddScoped<IPostHogClientWrapper, PostHogClientWrapper>();

builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IUserCurrencyService, UserCurrencyService>();
builder.Services.AddScoped<UserCurrencyHandler>();

builder.Services.AddScoped<ICosmeticService, CosmeticService>();
builder.Services.AddScoped<IUserCosmeticService, UserCosmeticService>();
builder.Services.AddScoped<UserCosmeticHandler>();

builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<UserCharacterHandler>();

builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<IBadgeAdminService, BadgeAdminService>();

builder.Services.AddScoped<IItemPoolService, ItemPoolService>();
builder.Services.AddScoped<IAIAPI, AIAPI>();
builder.Services.AddScoped<IFeedbackGen, OriginalFeedbackGen>();
builder.Services.AddScoped<IQuestionGen, OriginalQuestionGen>();
builder.Services.AddScoped<AIUtils>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<IResearcherDashboardService, ResearcherDashboardService>();

builder.Services.AddScoped(typeof(IAlgorithmRegistry<>), typeof(AlgorithmRegistry<>));
var aaAssembly = typeof(AlgorithmServiceAdder).Assembly;
builder.Services.AddAlgorithms(aaAssembly);

builder.Services.AddSingleton(_ => Channel.CreateBounded<EventData>(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = true,
}));
builder.Services.AddSingleton<IEventQueue, EventQueue>();
builder.Services.AddEventHandlers(typeof(EventHandlerAdder).Assembly);
builder.Services.AddHostedService<EventQueueService>();
builder.Services.AddScoped<IStreakResetService, StreakResetService>();
builder.Services.AddHostedService<StreakService>();

builder.Services.AddScoped<IGameEventManager, GameEventManager>();
builder.Services.AddScoped<IGameEventListener, BadgeGameEventHandler>();
builder.Services.AddScoped<IBadgeImageCollectorListener, LiveBadgeCompiler>();
builder.Services.AddSingleton<IBadgeImageCollector, BadgeImageCollector>();

builder.Services.AddSingleton<ITimeProvider, SystemTimeProvider>();
builder.Services.AddSingleton<TimeTool>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_ඞ";
    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedAccount = false;
});

builder.Services.AddAuthorization();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "test-session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<IMathMLConverter, MathMLConverter>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.Services.GetRequiredService<IBadgeImageCollector>(); // forces instantiation of the badge image collector

// Correctly redirect to HTTPS instead of HTTP
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    var postHog = app.Services.GetRequiredService<IPostHogClient>();
    postHog.Dispose();
});

if (Environment.GetEnvironmentVariable("BACKEND_ENVIRONMENT") == "Development")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseResponseCompression();

app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
    context.Response.Headers.Pragma = "no-cache";
    context.Response.Headers.Expires = "0";
    await next();
});

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();

//Updates "LastLoggedIn" for the user whenever logging in
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {

        var userManager = context.RequestServices.GetRequiredService<UserManager<User>>();
        var freshUser = await userManager.GetUserAsync(context.User);
        if (freshUser != null && (freshUser.LastLoggedIn == null ||
            (DateTimeOffset.UtcNow - freshUser.LastLoggedIn.Value >= TimeSpan.FromHours(1)))) //prevents updating every refresh
        {
            freshUser.LastLoggedIn = DateTimeOffset.UtcNow;
            await userManager.UpdateAsync(freshUser);
        }
    }
    await next();
});

app.MapControllers();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

if (!isTesting)
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // renamed: was conflicting with 'context' variable name from outer scope
    if ((Environment.GetEnvironmentVariable("BACKEND_DB_AUTOMIGRATE") ?? "false").Equals("true", StringComparison.CurrentCultureIgnoreCase))
    {
        try
        {
            if (dbContext.Database.GetPendingMigrations().Any())
                dbContext.Database.Migrate();
        }
        catch
        {
            dbContext.Database.Migrate();
        }
    }

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    if (!await roleManager.RoleExistsAsync(Roles.Admin))
        await roleManager.CreateAsync(new ApplicationRole { Name = Roles.Admin });

    if (!await roleManager.RoleExistsAsync(Roles.User))
        await roleManager.CreateAsync(new ApplicationRole { Name = Roles.User });

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    // Moved to .env
    string adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")
        ?? throw new Exception("ADMIN_EMAIL not set");
    string adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
        ?? throw new Exception("ADMIN_PASSWORD not set");

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = "RootAdmin",
            Email = adminEmail,
            DisplayName = "RootAdmin",
            FirstName = "Root",
            LastName = "Admin"
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);

        if (!createResult.Succeeded)
        {
            throw new Exception(
                "Failed to create admin user: " +
                string.Join(", ", createResult.Errors.Select(e => e.Description))
            );
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
        await userManager.AddToRoleAsync(adminUser, Roles.Admin);
}

app.Run();

public partial class Program { }