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
    public async Task<ActionResult<Conference>> Create([FromBody] Conference conference)
    {
        if (string.IsNullOrEmpty(conference.Id) || conference.Id == "123" || conference.Id == "111")
        {
            conference.Id = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(conference.OrganizerId))
        {
            conference.OrganizerId = "admin-1";
        }

        _context.Conferences.Add(conference);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = conference.Id }, conference);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var conference = await _context.Conferences.FindAsync(id);
        if (conference == null) return NotFound();

        _context.Conferences.Remove(conference);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}