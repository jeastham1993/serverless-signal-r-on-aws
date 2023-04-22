using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Microsoft.AspNetCore.SignalR;
using SignalR.Shared;
using TranslationProcessor;

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

var hostName = Environment.GetEnvironmentVariable("HOST_NAME") ?? "localhost";
var portNumber = 6379;
var password = Environment.GetEnvironmentVariable("CACHE_PASSWORD") ?? "eYVX7EwVmmxKPCDmwMtyKVge8oLd2t81";

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
        .AddStackExchangeRedis(connectionString, options =>
        {
            options.Configuration.ChannelPrefix = "translation";
        });   
}

builder.Services.AddLogging();

// builder.Services.AddHostedService<TranslationResponseWorker>();

var app = builder.Build();

app.UseAuthorization();

app.MapHub<TranslationHub>("/translationHub");

var translationHub = app.Services.GetRequiredService<IHubContext<TranslationHub>>();

await translationHub.Clients.Group("james").SendCoreAsync("Test", new[] { "hello" });;

app.MapGet(
    "/health",
    () => Results.Ok());

// Allow client responses to be sent over HTTP
app.MapPost("/transation/response", async context =>
{
    var translationResponse = JsonSerializer.Deserialize<TranslateMessageResponse>(context.Request.Body);

    await translationHub.Clients.Group(translationResponse.Username)
        .SendCoreAsync("ReceiveTranslationResponse", new object?[]{translationResponse.Translation});
});

app.MapControllers();

app.Run();
