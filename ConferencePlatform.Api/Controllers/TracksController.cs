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
    public async Task<ActionResult<Track>> Create([FromBody] Track track)
    {
        if (string.IsNullOrEmpty(track.Id))
        {
            track.Id = Guid.NewGuid().ToString();
        }

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = track.Id }, track);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var track = await _context.Tracks.FindAsync(id);
        if (track == null) return NotFound(new { message = "Трек не найден" });

        _context.Tracks.Remove(track);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}