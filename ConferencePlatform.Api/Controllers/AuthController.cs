using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using ConferencePlatform.Api.Auth;
using ConferencePlatform.Api.Data;
using ConferencePlatform.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ConferencePlatform.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AppDbContext db, IPasswordHasher<User> hasher, AuthTokens tokens) : ControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (request.Role is not ("Listener" or OrganizerAccount.Role))
            return BadRequest(new { message = "Выбери роль участника или организатора." });

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
            return Conflict(new { message = "Пользователь с таким email уже зарегистрирован." });

        var fullName = request.FullName?.Trim();
        var user = new User
        {
            Email = email,
            FullName = string.IsNullOrWhiteSpace(fullName) ? email.Split('@')[0] : fullName,
            Role = request.Role
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { message = "Пользователь с таким email уже зарегистрирован." });
        }

        return CreatedAtAction(nameof(Me), null, Session(user));
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(account => account.Email == email, cancellationToken);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Неверный email или пароль." });

        return Ok(Session(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(account => account.Id == id, cancellationToken);
        if (user is null) return Unauthorized();
        return Ok(Profile(user));
    }

    [Authorize]
    [HttpPost("become-organizer/{userId}")]
    public async Task<IActionResult> BecomeOrganizer(string userId, CancellationToken cancellationToken)
    {
        if (User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value != userId)
            return Forbid();

        var user = await db.Users.SingleOrDefaultAsync(account => account.Id == userId, cancellationToken);
        if (user is null) return NotFound(new { message = "Пользователь не найден." });

        if (user.Role != OrganizerAccount.Role)
        {
            user.Role = OrganizerAccount.Role;
            await db.SaveChangesAsync(cancellationToken);
        }

        return Ok(Session(user));
    }

    private static object Profile(User user) => new { userId = user.Id, user.Email, user.FullName, user.Role };

    private object Session(User user) => new
    {
        userId = user.Id,
        user.Email,
        user.FullName,
        user.Role,
        access_token = tokens.Issue(user)
    };
}

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128)] string Password);

public sealed record RegisterRequest(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128, MinimumLength = 8)] string Password,
    [StringLength(120)] string? FullName,
    string Role = "Listener");
