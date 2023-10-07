using System;
using Amazon.BedrockRuntime;
using Honeycomb.OpenTelemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;
using SignalR.Shared;
using SignalR.TranslationWorker;
using SignalR.TranslationWorker.Services;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

Sdk.CreateTracerProviderBuilder()
    .AddSource(TelemetryConstants.ServiceName)
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: TelemetryConstants.ServiceName).AddTelemetrySdk())
    .AddAspNetCoreInstrumentation()
    .AddHoneycomb(new HoneycombOptions
    {
        ServiceName = TelemetryConstants.ServiceName,
        ApiKey = Environment.GetEnvironmentVariable("HONEYCOMB_API_KEY")
    })
    .Build();

builder.Configuration.AddEnvironmentVariables();

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services
    .SetupSignalR()
    .SetupAwsSdks();

builder.Services.AddSingleton<ITranslationService, BedrockTranslationService>();

builder.Services.AddLogging();

builder.Services.AddHostedService<TranslationQueueWorker>();
builder.Services.AddHostedService<EventStreamWorker>();

var app = builder.Build();

app.MapHub<TranslationHub>("/api/translationHub");
app.MapHub<EventHub>("/api/events");

app.MapControllers();

app.Run();
