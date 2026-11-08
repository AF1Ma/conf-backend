using System.ComponentModel.DataAnnotations;
using ConferencePlatform.Api.Data;
using ConferencePlatform.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConferencePlatform.Api.Auth;

public static class OrganizerAccount
{
    public const string Role = "Organizer";

    public static async Task ProvisionAsync(WebApplication app)
    {
        var email = app.Configuration["OrganizerAccount:Email"]?.Trim().ToLowerInvariant();
        var password = app.Configuration["OrganizerAccount:Password"];
        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password)) return;
        if (string.IsNullOrEmpty(email) || email.Length > 254 || !new EmailAddressAttribute().IsValid(email)
            || password is null || password.Length is < 8 or > 128)
            throw new InvalidOperationException("Set a valid OrganizerAccount:Email and a password of 8–128 characters.");

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var existing = await db.Users.SingleOrDefaultAsync(user => user.Email == email);
        if (existing is not null)
        {
            if (existing.Role != Role)
                app.Logger.LogWarning("The configured organizer email belongs to an existing attendee; use a separate email.");
            return;
        }

        var user = new User { Email = email, FullName = "Организатор", Role = Role };
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        user.PasswordHash = hasher.HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        app.Logger.LogInformation("Organizer account created.");
    }
}
