using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Data.Entities;

namespace Project.Web;

public class SeedDataService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<SeedDataService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Wait for Postgres to be fully ready — the Aspire health check passes before
        // the database is ready to accept schema queries on first container creation
        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (await db.Database.CanConnectAsync(cancellationToken))
                    break;
            }
            catch
            {
                // Connection not ready yet
            }

            if (attempt == maxAttempts)
            {
                logger.LogError("Could not connect to database after {Max} attempts.", maxAttempts);
                throw new InvalidOperationException("Database is not reachable.");
            }

            logger.LogInformation("Waiting for database (attempt {Attempt}/{Max})...", attempt, maxAttempts);
            await Task.Delay(2000, cancellationToken);
        }

        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Database migrations applied.");

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await userManager.Users.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Users already exist, skipping seed.");
            return;
        }

        var email = configuration["SeedUser:Email"];
        var password = configuration["SeedUser:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No SeedUser configured in appsettings.json. Skipping user seed.");
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create seed user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        logger.LogInformation("Seed user '{Email}' created.", email);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
