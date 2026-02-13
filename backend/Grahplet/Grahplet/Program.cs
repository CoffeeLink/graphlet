using Grahplet.Repositories;
using Grahplet.Middleware;
using Grahplet;
using Grahplet.WebSockets;
using Grahplet.Data;
using Microsoft.EntityFrameworkCore;
using Grahplet.Security;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
Configuration.LoadFromConfiguration(builder.Configuration);

// Database (SQLite by default)
var connStr = builder.Configuration["Database:ConnectionString"];
if (string.IsNullOrWhiteSpace(connStr))
{
    // fallback to local file
    connStr = "Data Source=grahplet.db";
}

builder.Services.AddDbContext<GrahpletDbContext>(options => options.UseSqlite(connStr));

// Repositories (EF-backed)
builder.Services.AddScoped<IAuthRepository, EfAuthRepository>();
builder.Services.AddScoped<IWorkspaceRepository, EfWorkspaceRepository>();
builder.Services.AddScoped<ITagRepository, EfTagRepository>();
builder.Services.AddScoped<INoteRepository, EfNoteRepository>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register SessionDictionary as a singleton hosted service
builder.Services.AddSingleton<SessionDictionary>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<SessionDictionary>());

// SessionClient should be transient (one per WebSocket connection)
builder.Services.AddTransient<SessionClient>();
// CORS: Permissive policy to allow cross-origin requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("Permissive", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GrahpletDbContext>();
    db.Database.EnsureCreated();
    if (!db.Users.Any())
    {
        db.Users.Add(new DbUser
        {
            Id = Guid.NewGuid(),
            Username = "demo",
            Email = "demo@example.com",
            PasswordHash = PasswordHasher.Hash("demo"),
            ProfilePicUrl = "https://example.com/pic.jpg",
            LastSeen = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseCustomAuthentication();

// Enable CORS before mapping endpoints
app.UseCors("Permissive");

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

app.Map("/ws/live", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var sessionClient = context.RequestServices.GetRequiredService<SessionClient>();
        await sessionClient.HandleAsync(context, socket);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});
app.MapControllers();

app.Run();

