using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConferencePlatform.Api.Data;
using ConferencePlatform.Api.Models;

namespace ConferencePlatform.Api.Controllers;

[ApiController]
[Route("api/speakers")]
public class SpeakersController : ControllerBase
{
    private readonly AppDbContext _context;

    public SpeakersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Speaker>>> GetAll()
    {
        return Ok(await _context.Speakers.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<Speaker>> Create([FromBody] Speaker speaker)
    {
        if (string.IsNullOrEmpty(speaker.Id))
        {
            speaker.Id = Guid.NewGuid().ToString();
        }

        _context.Speakers.Add(speaker);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = speaker.Id }, speaker);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var speaker = await _context.Speakers.FindAsync(id);
        if (speaker == null) return NotFound(new { message = "Спикер не найден" });

        _context.Speakers.Remove(speaker);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}