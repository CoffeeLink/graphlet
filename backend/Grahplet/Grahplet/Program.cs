using Grahplet.Repositories;
using Grahplet.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<IAuthRepository, InMemoryAuthRepository>();
builder.Services.AddScoped<IWorkspaceRepository, InMemoryWorkspaceRepository>();
builder.Services.AddScoped<ITagRepository, InMemoryTagRepository>();
builder.Services.AddScoped<INoteRepository, InMemoryNoteRepository>();
builder.Services.AddScoped<INoteRelationRepository, InMemoryNoteRelationRepository>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Custom authentication middleware
app.UseCustomAuthentication();

app.MapControllers();

app.Run();

