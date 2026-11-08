using System.IdentityModel.Tokens.Jwt;
using ConferencePlatform.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConferencePlatform.Api.Data;
using ConferencePlatform.Api.Models;

namespace ConferencePlatform.Api.Controllers;

[ApiController]
[Route("api/tracks")]
public class TracksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TracksController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Track>>> GetAll([FromQuery] string? conferenceId)
    {
        var query = _context.Tracks.AsQueryable();
        if (!string.IsNullOrEmpty(conferenceId))
        {
            query = query.Where(t => t.ConferenceId == conferenceId);
        }
        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    [Authorize(Roles = OrganizerAccount.Role)]
    public async Task<ActionResult<Track>> Create([FromBody] Track track)
    {
        var conference = await _context.Conferences.AsNoTracking().FirstOrDefaultAsync(c => c.Id == track.ConferenceId);
        if (conference is null) return NotFound(new { message = "Конференция не найдена." });
        if (conference.OrganizerId != User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value) return Forbid();

        track.Name = track.Name.Trim();
        if (track.Name.Length is < 1 or > 120)
            return BadRequest(new { message = "Введи название секции длиной до 120 символов." });

        track.Id = Guid.NewGuid().ToString();
        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = track.Id }, track);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = OrganizerAccount.Role)]
    public async Task<IActionResult> Delete(string id)
    {
        var track = await _context.Tracks.FindAsync(id);
        if (track == null) return NotFound(new { message = "Трек не найден" });

        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!await _context.Conferences.AnyAsync(c => c.Id == track.ConferenceId && c.OrganizerId == userId)) return Forbid();

        _context.Tracks.Remove(track);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}