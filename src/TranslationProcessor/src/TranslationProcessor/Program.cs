using System;
using System.Text.Json;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.Translate;
using Amazon.Translate.Model;
using AWS.Lambda.Powertools.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalR.Shared;
using TranslationProcessor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new AmazonSQSClient());
builder.Services.AddSingleton(new AmazonTranslateClient());

var configuration = new ConfigurationBuilder()
    .Build();

builder.Services.AddSingleton<IConfiguration>(configuration);
builder.Services.AddLogging();

var app = builder.Build();

var translateClient = app.Services.GetRequiredService<AmazonTranslateClient>();
var sqsClient = app.Services.GetRequiredService<AmazonSQSClient>();

var handler = async (SQSEvent sqsEvent) =>
{
    Logger.LogInformation("Handler active");

    foreach (var message in sqsEvent.Records)
    {
        var translationRequest = JsonSerializer.Deserialize<TranslateMessageCommand>(message.Body);
        
        Logger.LogInformation($"Processing translation for {translationRequest.Username} and connection {translationRequest.ConnectionId}");

        var translationResponse = await translateClient.TranslateTextAsync(new TranslateTextRequest
        {
            SourceLanguageCode = "en",
            TargetLanguageCode = translationRequest.TranslateTo.ToLower(),
            Text = translationRequest.Message
        });
        
        Logger.LogInformation("Sending message to connected client");

        await sqsClient.SendMessageAsync(Environment.GetEnvironmentVariable("QUEUE_URL"), JsonSerializer.Serialize(
            new TranslateMessageResponse()
            {
                Translation = translationResponse.TranslatedText,
                Username = translationRequest.Username,
                ConnectionId = translationRequest.ConnectionId
            }));
    }
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();