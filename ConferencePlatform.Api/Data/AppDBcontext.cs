using Microsoft.EntityFrameworkCore;
using ConferencePlatform.Api.Models;

namespace ConferencePlatform.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Conference> Conferences => Set<Conference>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Speaker> Speakers => Set<Speaker>();
    public DbSet<TalkSubmission> TalkSubmissions => Set<TalkSubmission>();
    public DbSet<ScheduleSlot> ScheduleSlots => Set<ScheduleSlot>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
}