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
    public async Task<ActionResult<IEnumerable<Conference>>> GetAll()
    {
        if (!await _context.Conferences.AnyAsync())
        {
            _context.Conferences.AddRange(
                new Conference
                {
                    Id = "d3b07384-d113-4c57-bc9b-a39f64724216",
                    Title = "Science & IT Conf 2026",
                    Description = "Ежегодная научная конференция по современным IT-технологиям и архитектуре систем.",
                    StartDate = new DateTime(2026, 10, 15, 10, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 10, 17, 18, 0, 0, DateTimeKind.Utc),
                    Location = "Главный корпус / Онлайн"
                },
                new Conference
                {
                    Id = "e7c10592-8a44-42b1-9b63-c78216147289",
                    Title = "DevOps & Cloud Days",
                    Description = "Митап практиков по контейнеризации, CI/CD и наблюдаемости распределенных систем.",
                    StartDate = new DateTime(2026, 10, 22, 11, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 10, 23, 17, 0, 0, DateTimeKind.Utc),
                    Location = "Аудитория 204"
                }
            );
            await _context.SaveChangesAsync();
        }

        var conferences = await _context.Conferences.ToListAsync();
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