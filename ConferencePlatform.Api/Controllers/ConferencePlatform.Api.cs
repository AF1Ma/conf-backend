using System.IdentityModel.Tokens.Jwt;
using ConferencePlatform.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConferencePlatform.Api.Data;
using ConferencePlatform.Api.Models;

namespace ConferencePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConferencesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ConferencesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Conference>>> GetAll([FromQuery] string? organizerId)
    {
        var query = _context.Conferences.AsQueryable();

        if (!string.IsNullOrEmpty(organizerId))
        {
            query = query.Where(c => c.OrganizerId == organizerId);
        }

        var conferences = await query.ToListAsync();
        return Ok(conferences);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Conference>> GetById(string id)
    {
        var conference = await _context.Conferences.FirstOrDefaultAsync(c => c.Id == id);
        if (conference == null)
        {
            return NotFound(new { message = $"Конференция с ID '{id}' не найдена" });
        }
        return Ok(conference);
    }

    [HttpPost]
    [Authorize(Roles = OrganizerAccount.Role)]
    public async Task<ActionResult<Conference>> Create([FromBody] Conference conference)
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userId is null || userId != conference.OrganizerId) return Forbid();

        conference.Title = conference.Title.Trim();
        conference.Description = conference.Description.Trim();
        conference.Location = conference.Location.Trim();
        if (conference.Title.Length is < 1 or > 160 || conference.Location.Length is < 1 or > 180
            || conference.Description.Length > 6000)
            return BadRequest(new { message = "Укажи название и место проведения; описание — до 6000 символов." });

        if (conference.StartDate == default || conference.EndDate <= conference.StartDate)
            return BadRequest(new { message = "Укажи время окончания позже начала конференции." });

        conference.Id = Guid.NewGuid().ToString();
        conference.StartDate = conference.StartDate.ToUniversalTime();
        conference.EndDate = conference.EndDate.ToUniversalTime();
        _context.Conferences.Add(conference);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = conference.Id }, conference);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = OrganizerAccount.Role)]
    public async Task<IActionResult> Delete(string id)
    {
        var conference = await _context.Conferences.FindAsync(id);
        if (conference == null) return NotFound();
        if (conference.OrganizerId != User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value) return Forbid();

        _context.Conferences.Remove(conference);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}