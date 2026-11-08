using System.Threading.RateLimiting;
using ConferencePlatform.Api.Auth;
using ConferencePlatform.Api.Data;
using ConferencePlatform.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => options.AddPolicy("AllowAll", policy =>
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var tokens = new AuthTokens(builder.Configuration["Auth:SigningKey"]);
builder.Services.AddSingleton(tokens);
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = tokens.ValidationParameters;
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", async (AppDbContext db, CancellationToken cancellationToken) =>
    await db.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
app.MapControllers();

await OrganizerAccount.ProvisionAsync(app);
app.Run();
