using Honeycomb.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SignalR;
using SignalR.Gateway;
using SignalR.Shared;

var builder = WebApplication.CreateBuilder(args);

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HONEYCOMB_API_KEY")))
{
    Sdk.CreateTracerProviderBuilder()
        .AddSource(TelemetryConstants.ServiceName)
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: TelemetryConstants.ServiceName)
            .AddTelemetrySdk())
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddHoneycomb(new HoneycombOptions
        {
            ServiceName = TelemetryConstants.ServiceName,
            ApiKey = Environment.GetEnvironmentVariable("HONEYCOMB_API_KEY")
        })
        .Build();
}

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services
    .SetupSignalR()
    .SetupAwsSdks();

builder.Services.AddLogging();

var app = builder.Build();

app.UseCors(opt => opt
    .AllowAnyHeader()
    .AllowAnyOrigin()
    .AllowAnyMethod());

app.MapHub<TranslationHub>("/api/translationHub");
app.MapHub<EventHub>("/api/events");

app.MapGet("/api/health", () => Results.Ok());

app.MapControllers();

app.Run();
