using Microsoft.AspNetCore.Mvc;

namespace ConferencePlatform.Api;

[ApiController]
[Route("api/[controller]")]
public class ConferencesController : ControllerBase
{
    private static readonly List<Conference> MockConferences = new()
    {
        new Conference
        {
            Id = 1,
            Title = "Science & IT Conf 2026",
            Description = "Ежегодная научная конференция по современным IT-технологиям и архитектуре систем.",
            StartDate = new DateTime(2026, 10, 15, 10, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 10, 17, 18, 0, 0, DateTimeKind.Utc),
            Location = "Главный корпус / Онлайн"
        },
        new Conference
        {
            Id = 2,
            Title = "DevOps & Cloud Days",
            Description = "Митап практиков по контейнеризации, CI/CD и наблюдаемости распределенных систем.",
            StartDate = new DateTime(2026, 10, 22, 11, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 10, 23, 17, 0, 0, DateTimeKind.Utc),
            Location = "Аудитория 204"
        }
    };

    [HttpGet]
    public ActionResult<IEnumerable<Conference>> GetAll()
    {
        return Ok(MockConferences);
    }

    [HttpGet("{id}")]
    public ActionResult<Conference> GetById(int id)
    {
        var conference = MockConferences.FirstOrDefault(c => c.Id == id);
        if (conference == null)
        {
            return NotFound(new { message = $"Конференция с ID {id} не найдена" });
        }
        return Ok(conference);
    }
}