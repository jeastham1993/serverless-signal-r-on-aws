using Microsoft.AspNetCore.SignalR;

using SignalR.Gateway;
using SignalR.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services
    .SetupSignalR()
    .SetupAwsSdks();

builder.Services.AddLogging();

var app = builder.Build();

app.MapHub<TranslationHub>("/api/translationHub");
app.MapHub<EventHub>("/api/events");

app.MapGet("/api/health", () => Results.Ok());

app.MapControllers();

app.Run();
