using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.Translate;
using Amazon.Translate.Model;
using AWS.Lambda.Powertools.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using TranslationProcessor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new AmazonSQSClient());
builder.Services.AddSingleton(new AmazonTranslateClient());

var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_ENDPOINT");

Console.WriteLine($"Connecting to: {redisConnectionString}");

builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnectionString, options =>
    {
    });

var app = builder.Build();

app.MapHub<TranslationHub>("/translationHub");

var handler = async (SQSEvent sqsEvent) =>
{
    Logger.LogInformation("Handler active");
    var sqsClient = app.Services.GetRequiredService<AmazonSQSClient>();
    var translateClient = app.Services.GetRequiredService<AmazonTranslateClient>();
    var hub = app.Services.GetRequiredService<IHubContext<TranslationHub>>();
    
    Logger.LogInformation($"Retrieved Hub: {hub.ToString()}");
    
    await hub.Clients.Group("james").SendCoreAsync("Test", new[] { "hello" });;
    
    Logger.LogInformation("Sending message");
            
    foreach (var message in sqsEvent.Records)
    {
        var translationRequest = JsonSerializer.Deserialize<TranslateMessageCommand>(message.Body);

        var translationResponse = await translateClient.TranslateTextAsync(new TranslateTextRequest
        {
            SourceLanguageCode = "en",
            TargetLanguageCode = translationRequest.TranslateTo.ToLower(),
            Text = translationRequest.Message
        });

        await sqsClient.SendMessageAsync(
            Environment.GetEnvironmentVariable("QUEUE_URL"), JsonSerializer.Serialize(new TranslateMessageResponse()
            {
                Translation = translationResponse.TranslatedText,
                ConnectionId = translationRequest.ConnectionId,
                Username = translationRequest.Username
            }));
    }
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();