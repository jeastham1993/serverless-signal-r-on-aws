using Serilog;
using Serilog.Formatting.Compact;
using SignalR.Gateway;
using SignalR.Shared;
using SignalR.TranslationWorker;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services
    .SetupSignalR()
    .SetupAwsSdks();

builder.Services.AddLogging();

builder.Services.AddHostedService<TranslationQueueWorker>();
builder.Services.AddHostedService<EventStreamWorker>();

var app = builder.Build();

app.MapHub<TranslationHub>("/api/translationHub");
app.MapHub<EventHub>("/api/events");

app.MapControllers();

app.Run();
