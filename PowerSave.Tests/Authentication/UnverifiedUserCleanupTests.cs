using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PowerSave.Application.Common.Interfaces;
using PowerSave.Application.Common.Localization;
using PowerSave.Application.DTOs;
using PowerSave.Application.Services;
using PowerSave.Application.Validation;
using PowerSave.Domain.Entities;
using PowerSave.Infrastructure.BackgroundServices;
using PowerSave.Infrastructure.Persistence;
using PowerSave.Infrastructure.Security;
using PowerSave.Tests.Helpers;
using Xunit;

namespace PowerSave.Tests.Authentication;

public class UnverifiedUserCleanupTests
{
    private readonly TestTimeProvider _timeProvider = new()
    {
        UtcNow = new DateTime(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc)
    };
    private readonly TestSmsSender _smsSender = new();
    private readonly PhoneNumberValidator _phoneValidator = new();
    private readonly UsernameValidator _usernameValidator = new();
    private readonly PasswordPolicyValidator _passwordValidator = new();
    private readonly ConsentPolicyValidator _consentValidator = new();
    private readonly OtpService _otpService = new();

    private AuthService CreateAuthService(string dbName)
    {
        var (dbContext, userManager) = TestAuthDbContextFactory.Create(dbName);
        var jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = "Test_Secret_Key_For_Unit_Testing_PowerSave_Platform_2026!",
            Issuer = "PowerSave.API",
            Audience = "PowerSave.App",
            ExpirationMinutes = 60
        });
        var tokenService = new JwtTokenService(jwtOptions);

        return new AuthService(
            userManager,
            dbContext,
            _phoneValidator,
            _usernameValidator,
            _passwordValidator,
            _consentValidator,
            _otpService,
            _smsSender,
            tokenService,
            _timeProvider);
    }

    [Fact]
    public async Task Register_WhenStaleUnverifiedUserExistsOlderThan48Hours_ShouldDeleteOldUserAndSucceed()
    {
        var dbName = Guid.NewGuid().ToString();
        var (dbContext, userManager) = TestAuthDbContextFactory.Create(dbName);
        var authService = CreateAuthService(dbName);

        // Seed a stale unverified user created 50 hours ago (> 48h)
        var staleUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "stale_user",
            PhoneNumber = "01012345678",
            PhoneNumberConfirmed = false,
            IsActive = false,
            CreatedAtUtc = _timeProvider.UtcNow.AddHours(-50),
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await userManager.CreateAsync(staleUser, "Password123!");

        // New registration using the exact same phone number and username
        var request = new RegisterRequestDto
        {
            PhoneNumber = "01012345678",
            Username = "stale_user",
            Password = "NewPassword123!",
            ConsentToAccountCreationAndDataCollection = true,
            ConsentToProductImprovementDataUsage = true,
            ConsentDocumentVersion = "v1.0"
        };

        var result = await authService.RegisterAsync(request, "127.0.0.1");

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.StatusCode);

        // Verify the old user was deleted and the new user exists
        var users = await dbContext.Users.ToListAsync();
        Assert.Single(users);
        Assert.NotEqual(staleUser.Id, users[0].Id);
        Assert.Equal("stale_user", users[0].UserName);
        Assert.Equal("01012345678", users[0].PhoneNumber);
    }

    [Fact]
    public async Task Register_WhenUnverifiedUserExistsLessThan48Hours_ShouldReturnDuplicatePhoneConflict()
    {
        var dbName = Guid.NewGuid().ToString();
        var (dbContext, userManager) = TestAuthDbContextFactory.Create(dbName);
        var authService = CreateAuthService(dbName);

        // Seed a recent unverified user created 10 hours ago (< 48h)
        var recentUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "recent_user",
            PhoneNumber = "01012345678",
            PhoneNumberConfirmed = false,
            IsActive = false,
            CreatedAtUtc = _timeProvider.UtcNow.AddHours(-10),
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await userManager.CreateAsync(recentUser, "Password123!");

        var request = new RegisterRequestDto
        {
            PhoneNumber = "01012345678",
            Username = "different_username",
            Password = "Password123!",
            ConsentToAccountCreationAndDataCollection = true,
            ConsentToProductImprovementDataUsage = true,
            ConsentDocumentVersion = "v1.0"
        };

        var result = await authService.RegisterAsync(request, "127.0.0.1");

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal(AuthErrorCode.DuplicatePhoneNumber, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyPhone_WhenUserIsExpiredUnverified_ShouldDeleteUserAndReturnNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var (dbContext, userManager) = TestAuthDbContextFactory.Create(dbName);
        var authService = CreateAuthService(dbName);

        var staleUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "expired_user",
            PhoneNumber = "01012345678",
            PhoneNumberConfirmed = false,
            IsActive = false,
            CreatedAtUtc = _timeProvider.UtcNow.AddHours(-50),
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await userManager.CreateAsync(staleUser, "Password123!");

        var result = await authService.VerifyPhoneAsync(new VerifyPhoneRequestDto
        {
            Identifier = "01012345678",
            OtpCode = "123456"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal(AuthErrorCode.UserNotFound, result.ErrorCode);

        // Verify user was deleted
        var exists = await dbContext.Users.AnyAsync(u => u.Id == staleUser.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task ResendOtp_WhenUserIsExpiredUnverified_ShouldDeleteUserAndReturnNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var (dbContext, userManager) = TestAuthDbContextFactory.Create(dbName);
        var authService = CreateAuthService(dbName);

        var staleUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "expired_user",
            PhoneNumber = "01012345678",
            PhoneNumberConfirmed = false,
            IsActive = false,
            CreatedAtUtc = _timeProvider.UtcNow.AddHours(-50),
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await userManager.CreateAsync(staleUser, "Password123!");

        var result = await authService.ResendOtpAsync(new ResendOtpRequestDto
        {
            Identifier = "01012345678"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal(AuthErrorCode.UserNotFound, result.ErrorCode);

        // Verify user was deleted
        var exists = await dbContext.Users.AnyAsync(u => u.Id == staleUser.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task Login_WhenUserIsExpiredUnverified_ShouldDeleteUserAndReturnNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var (dbContext, userManager) = TestAuthDbContextFactory.Create(dbName);
        var authService = CreateAuthService(dbName);

        var staleUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "expired_user",
            PhoneNumber = "01012345678",
            PhoneNumberConfirmed = false,
            IsActive = false,
            CreatedAtUtc = _timeProvider.UtcNow.AddHours(-50),
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await userManager.CreateAsync(staleUser, "Password123!");

        var result = await authService.LoginAsync(new LoginRequestDto
        {
            Identifier = "01012345678",
            Password = "Password123!"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal(AuthErrorCode.UserNotFound, result.ErrorCode);

        // Verify user was deleted
        var exists = await dbContext.Users.AnyAsync(u => u.Id == staleUser.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task BackgroundCleanupService_CleanupExpiredUsers_DeletesOnlyStaleUnverifiedUsers()
    {
        var dbName = Guid.NewGuid().ToString();
        var (dbContext, userManager) = TestAuthDbContextFactory.Create(dbName);

        var staleUnverifiedUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "stale_unverified",
            PhoneNumber = "+201011111111",
            PhoneNumberConfirmed = false,
            IsActive = false,
            CreatedAtUtc = _timeProvider.UtcNow.AddHours(-49),
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await userManager.CreateAsync(staleUnverifiedUser, "Password123!");

        var recentUnverifiedUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "recent_unverified",
            PhoneNumber = "+201022222222",
            PhoneNumberConfirmed = false,
            IsActive = false,
            CreatedAtUtc = _timeProvider.UtcNow.AddHours(-10),
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await userManager.CreateAsync(recentUnverifiedUser, "Password123!");

        var verifiedActiveUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "verified_active",
            PhoneNumber = "+201033333333",
            PhoneNumberConfirmed = true,
            IsActive = true,
            CreatedAtUtc = _timeProvider.UtcNow.AddHours(-100),
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await userManager.CreateAsync(verifiedActiveUser, "Password123!");

        // Build a ServiceProvider with the required services
        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        services.AddSingleton(userManager);
        services.AddSingleton<IDateTimeProvider>(_timeProvider);

        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var cleanupService = new UnverifiedUserCleanupService(
            scopeFactory,
            NullLogger<UnverifiedUserCleanupService>.Instance,
            TimeSpan.FromHours(1));

        var deletedCount = await cleanupService.CleanupExpiredUsersAsync();

        Assert.Equal(1, deletedCount);

        var remainingUsers = await dbContext.Users.ToListAsync();
        Assert.Equal(2, remainingUsers.Count);
        Assert.DoesNotContain(remainingUsers, u => u.Id == staleUnverifiedUser.Id);
        Assert.Contains(remainingUsers, u => u.Id == recentUnverifiedUser.Id);
        Assert.Contains(remainingUsers, u => u.Id == verifiedActiveUser.Id);
    }
}
