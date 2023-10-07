using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.StepFunctions;
using Honeycomb.OpenTelemetry;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SignalR.Front;
using SignalR.Front.Data;
using SignalR.Front.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddHttpClient();

Sdk.CreateTracerProviderBuilder()
    .AddSource(TelemetryConstants.ServiceName)
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: TelemetryConstants.ServiceName).AddTelemetrySdk())
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddHoneycomb(new HoneycombOptions
    {
        ServiceName = TelemetryConstants.ServiceName,
        ApiKey = Environment.GetEnvironmentVariable("HONEYCOMB_API_KEY") ?? "COq84NfvgsAeYIZqaqB1TH"
    })
    .Build();

// Setup AWS Credentials and SDK
var chain = new CredentialProfileStoreChain();
AWSCredentials? awsCredentials;
var regionEndpoint = RegionEndpoint.EUWest1;

var stepFunctionsClient = new AmazonStepFunctionsClient(regionEndpoint);

if (chain.TryGetAWSCredentials("dev", out awsCredentials))
{
    stepFunctionsClient = new AmazonStepFunctionsClient(awsCredentials, regionEndpoint);
}

builder.Services.AddSingleton(stepFunctionsClient);
builder.Services.AddSingleton<IOrderWorkflowService, OrderWorkflowService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
