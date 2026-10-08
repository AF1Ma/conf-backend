using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConferencePlatform.Api.Data;
using ConferencePlatform.Api.Models;

namespace ConferencePlatform.Api.Controllers;

[ApiController]
[Route("api/talk-submissions")]
public class TalkSubmissionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TalkSubmissionsController(AppDbContext context)
    {
        _context = context;
    }

    // Получение списка всех поданных заявок (для панели организатора)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TalkSubmission>>> GetAll()
    {
        var submissions = await _context.TalkSubmissions.ToListAsync();
        return Ok(submissions);
    }

    // Подача заявки спикером (Call for Papers)
    [HttpPost]
    public async Task<ActionResult<TalkSubmission>> SubmitTalk([FromBody] TalkSubmission submission)
    {
        if (string.IsNullOrWhiteSpace(submission.Title))
        {
            return BadRequest(new { message = "Тема доклада обязательна для заполнения" });
        }

        submission.Id = Guid.NewGuid().ToString();
        submission.Status = "Submitted";
        submission.SubmittedAt = DateTime.UtcNow;

        _context.TalkSubmissions.Add(submission);
        await _context.SaveChangesAsync(); // Запись в реальную таблицу TalkSubmissions

        return CreatedAtAction(nameof(GetAll), new { id = submission.Id }, submission);
    }

    // Модерация заявки организатором (смена статуса на Approved или Rejected)
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusDto dto)
    {
        var submission = await _context.TalkSubmissions.FirstOrDefaultAsync(s => s.Id == id);
        if (submission == null)
        {
            return NotFound(new { message = "Заявка не найдена" });
        }

        submission.Status = dto.Status;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Статус заявки обновлен на '{dto.Status}'", submission });
    }
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}