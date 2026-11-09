using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDay.Api.Data;
using RaceDay.Api.Middleware;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// API and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "REST API used by the RaceDay MVC application."
    });

    // Avoid schema-name collisions if two CLR types share the same short name.
    options.CustomSchemaIds(type =>
        type.FullName?.Replace("+", ".", StringComparison.Ordinal) ?? type.Name);
});

// Database
var connectionString = builder.Configuration.GetConnectionString("RaceDayConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'RaceDayConnection' is missing from appsettings.json.");
}

builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(connectionString));

// Session-based authentication used by this assignment.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddAuthentication(RaceDay.Api.Middleware.SessionAuthenticationHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
        RaceDay.Api.Middleware.SessionAuthenticationHandler>(
        RaceDay.Api.Middleware.SessionAuthenticationHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<BlobStorageService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay.Api v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/error", () => Results.Problem(
    title: "An unexpected API error occurred.",
    statusCode: StatusCodes.Status500InternalServerError));

// Seed the two required roles if the existing database is reachable.
// This is idempotent and does not recreate or delete the user's database.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("RaceDayStartup");
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
        if (await db.Database.CanConnectAsync())
        {
            foreach (var roleName in new[] { "Participant", "Organiser" })
            {
                if (!await db.Roles.AnyAsync(role => role.RoleName == roleName))
                {
                    db.Roles.Add(new Role { RoleName = roleName });
                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation("RaceDay role seed check completed.");
        }
        else
        {
            logger.LogWarning(
                "RaceDay database is not reachable. Check ConnectionStrings:RaceDayConnection.");
        }
    }
    catch (Exception ex)
    {
        // Keep the API host available so Swagger can display diagnostics.
        // Registration will still require a working database and role table.
        logger.LogError(ex,
            "Could not verify RaceDay database/roles. Check the connection and run the SQL scripts.");
    }
}

app.Run();

public partial class Program { }
