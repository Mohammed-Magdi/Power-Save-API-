using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerSave.Application.Common.Interfaces;
using PowerSave.Domain.Entities;
using PowerSave.Infrastructure.Persistence;

namespace PowerSave.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically cleans up accounts that remained unverified for more than 48 hours.
/// All associated records (OTPs, Consents, RateLimits, Personalization data) are automatically deleted via EF Core cascade delete.
/// </summary>
public class UnverifiedUserCleanupService : BackgroundService
{
    public static readonly TimeSpan UnverifiedAccountLifespan = TimeSpan.FromHours(48);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UnverifiedUserCleanupService> _logger;
    private readonly TimeSpan _checkInterval;

    public UnverifiedUserCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<UnverifiedUserCleanupService> logger)
        : this(scopeFactory, logger, TimeSpan.FromHours(1))
    {
    }

    public UnverifiedUserCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<UnverifiedUserCleanupService> logger,
        TimeSpan checkInterval)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _checkInterval = checkInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UnverifiedUserCleanupService started. Interval: {Interval}, Lifespan: {Lifespan}",
            _checkInterval, UnverifiedAccountLifespan);

        // Run an initial sweep after a short initial delay to allow startup to complete
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            await CleanupExpiredUsersAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during initial cleanup of unverified users.");
        }

        using var timer = new PeriodicTimer(_checkInterval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CleanupExpiredUsersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired unverified users.");
            }
        }

        _logger.LogInformation("UnverifiedUserCleanupService stopped.");
    }

    public async Task<int> CleanupExpiredUsersAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PowerSaveDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var timeProvider = scope.ServiceProvider.GetService<IDateTimeProvider>();

        var now = timeProvider?.UtcNow ?? DateTime.UtcNow;
        var threshold = now.Subtract(UnverifiedAccountLifespan);

        var expiredUsers = await dbContext.Users
            .Where(u => !u.PhoneNumberConfirmed && !u.IsActive && u.CreatedAtUtc < threshold)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (expiredUsers.Count == 0)
        {
            return 0;
        }

        _logger.LogInformation("Found {Count} expired unverified user(s) created before {Threshold}.",
            expiredUsers.Count, threshold);

        var deletedCount = 0;
        foreach (var user in expiredUsers)
        {
            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                deletedCount++;
            }
            else
            {
                _logger.LogWarning("Failed to delete expired user {UserId}: {Errors}",
                    user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        _logger.LogInformation("Successfully cleaned up {DeletedCount} of {TotalCount} expired unverified user(s).",
            deletedCount, expiredUsers.Count);

        return deletedCount;
    }
}
