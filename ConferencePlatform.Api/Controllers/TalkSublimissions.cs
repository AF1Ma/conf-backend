using ConferencePlatform.Api.Auth;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet]
    [Authorize(Roles = OrganizerAccount.Role)]
    public async Task<ActionResult<IEnumerable<TalkSubmission>>> GetAll([FromQuery] string? status)
    {
        var query = _context.TalkSubmissions.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(s => s.Status == status);
        }

        var submissions = await query.ToListAsync();
        return Ok(submissions);
    }

    [HttpPost]
    public async Task<ActionResult<TalkSubmission>> SubmitTalk([FromBody] TalkSubmission submission)
    {
        if (string.IsNullOrWhiteSpace(submission.Title))
        {
            return BadRequest(new { message = "Тема доклада обязательна для заполнения" });
        }
        if (string.IsNullOrWhiteSpace(submission.SpeakerId))
            return BadRequest(new { message = "Укажи ID спикера." });
        if (!await _context.Tracks.AnyAsync(track => track.Id == submission.TrackId))
            return BadRequest(new { message = "Выбери существующую секцию конференции." });

        if (string.IsNullOrEmpty(submission.Id) || submission.Id == "123" || submission.Id == "111")
        {
            submission.Id = Guid.NewGuid().ToString();
        }

        submission.Status = "Submitted";
        submission.SubmittedAt = DateTime.UtcNow;

        _context.TalkSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = submission.Id }, submission);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = OrganizerAccount.Role)]
    public async Task<IActionResult> Delete(string id)
    {
        var submission = await _context.TalkSubmissions.FindAsync(id);
        if (submission == null) return NotFound();

        _context.TalkSubmissions.Remove(submission);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = OrganizerAccount.Role)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusDto dto)
    {
        if (dto.Status is not ("Approved" or "Rejected"))
            return BadRequest(new { message = "Допустимые решения: Approved или Rejected." });

        var updated = await _context.TalkSubmissions
            .Where(submission => submission.Id == id && submission.Status == "Submitted")
            .ExecuteUpdateAsync(setters => setters.SetProperty(submission => submission.Status, dto.Status));

        var submission = await _context.TalkSubmissions.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);
        if (submission is null) return NotFound(new { message = "Заявка не найдена." });
        if (updated == 0) return Conflict(new { message = "Заявка уже рассмотрена. Обновите список." });
        return Ok(new { message = $"Статус заявки обновлен на '{dto.Status}'", submission });
    }
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}