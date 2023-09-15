using SignalR.Gateway;
using SignalR.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services
    .SetupSignalR()
    .SetupAwsSdks();

builder.Services.AddLogging();

builder.Services.AddHostedService<TranslationQueueWorker>();


var app = builder.Build();

app.MapHub<TranslationHub>("/api/translationHub");

app.MapControllers();

app.Run();
