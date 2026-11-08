using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConferencePlatform.Api.Data;
using ConferencePlatform.Api.Models;

namespace ConferencePlatform.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return BadRequest(new { message = "Пользователь с таким email уже существует" });
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = dto.Email,
            FullName = dto.FullName,
            Role = string.IsNullOrEmpty(dto.Role) ? "Listener" : dto.Role,
            PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(dto.Password)),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(201, new { message = "Регистрация успешна", userId = user.Id, role = user.Role });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginDto dto)
    {
        var targetHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(dto.Password));

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.PasswordHash == targetHash);
        if (user == null)
        {
            return Unauthorized(new { message = "Неверный email или пароль" });
        }

        return Ok(new { message = "Успешный вход", userId = user.Id, fullName = user.FullName, role = user.Role });
    }

    [HttpPost("become-organizer/{userId}")]
    public async Task<ActionResult> BecomeOrganizer(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = "Пользователь не найден" });

        user.Role = "Organizer";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Роль успешно изменена на Организатор", role = user.Role });
    }
}

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Listener";
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}