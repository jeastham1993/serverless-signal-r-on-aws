using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Microsoft.AspNetCore.SignalR;
using SignalR.Gateway;
using SignalR.Gateway.Translation;

var builder = WebApplication.CreateBuilder(args);

var chain = new CredentialProfileStoreChain();
AWSCredentials? awsCredentials;
var regionEndpoint = RegionEndpoint.USEast1;

AmazonSQSClient sqsClient = new AmazonSQSClient(regionEndpoint);

if (chain.TryGetAWSCredentials("dev", out awsCredentials))
{
    sqsClient = new AmazonSQSClient(awsCredentials, regionEndpoint);
}

builder.Services.AddSingleton(sqsClient);

builder.Services.AddControllers();

var hostName = Environment.GetEnvironmentVariable("HOST_NAME") ?? "";
var portNumber = 6379;
var password = Environment.GetEnvironmentVariable("CACHE_PASSWORD") ?? "";

var connectionString = $"{hostName}:{portNumber}";

if (!string.IsNullOrEmpty(password))
{
    connectionString = $"{connectionString},password={password}";
}

if (string.IsNullOrEmpty(hostName))
{
    builder.Services.AddSignalR();
}
else
{
    builder.Services.AddSignalR()
        .AddStackExchangeRedis(connectionString);   
}

builder.Services.AddLogging();

builder.Services.AddHostedService<TranslationResponseWorker>();

var app = builder.Build();

app.UseAuthorization();

app.MapHub<TranslationHub>("/translationHub");

var translationHub = app.Services.GetRequiredService<IHubContext<TranslationHub>>();

app.MapGet(
    "/health",
    () => Results.Ok());

// Allow client responses to be sent over HTTP
app.MapPost("/transation/response", async context =>
{
    var translationResponse = JsonSerializer.Deserialize<TranslateMessageResponse>(context.Request.Body);

    await translationHub.Clients.Client(translationResponse.ConnectionId)
        .SendCoreAsync("ReceiveTranslationResponse", new object?[]{translationResponse.Translation});
});

// Allow client responses to be sent over HTTP
app.MapGet("/usertest", async context =>
{
    await translationHub.Clients.Groups("james")
        .SendCoreAsync("ReceiveTranslationResponse", new object?[]{"The translation is James"});
});

app.MapControllers();

app.Run();
