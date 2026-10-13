namespace ConferencePlatform.Api.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Listener";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Hall
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConferenceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

public class Track
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConferenceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class Speaker
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public bool IsDiplomaVerified { get; set; } = false;
}

public class TalkSubmission
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SpeakerId { get; set; } = string.Empty;
    public string TrackId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string Status { get; set; } = "Submitted";
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public class ScheduleSlot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string HallId { get; set; } = string.Empty;
    public string TalkSubmissionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class Registration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConferenceId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Confirmed";
}

public class Partner
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConferenceId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Tier { get; set; } = "General";
    public string LogoUrl { get; set; } = string.Empty;
}

public class Feedback
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ScheduleSlotId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}