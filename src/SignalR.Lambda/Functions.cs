using System.Text.Json;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.SQSEvents;
using Amazon.Translate;
using AWS.Lambda.Powertools.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SignalR.Lambda.Models;
using SignalR.Lambda.Services;
using SignalR.Shared;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SignalR.Lambda;

public class Functions
{
    private readonly IVideoSuggestionService videoSuggestionService;
    private readonly IHubContext<TranslationHub> translationHub;
    private readonly IHubContext<EventHub> eventHub;
    private readonly ITranslationService translationService;

    public Functions()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddSingleton<EventHub>();
        builder.Services.AddSingleton(new AmazonBedrockRuntimeClient(RegionEndpoint.USEast1));
        builder.Services.AddSingleton(new AmazonTranslateClient());
        
        builder.Services.AddSingleton<IVideoSuggestionService, BedrockVideoSuggestionService>();
        builder.Services.AddSingleton<ITranslationService, AmazonTranslateTranslationService>();
        
        builder.Services
            .SetupSignalR();

        var app = builder.Build();

        app.MapHub<EventHub>("/events");
        app.MapHub<TranslationHub>("/translations");

        videoSuggestionService = app.Services.GetRequiredService<IVideoSuggestionService>();
        translationService = app.Services.GetRequiredService<ITranslationService>();
        translationHub = app.Services.GetRequiredService<IHubContext<TranslationHub>>();
        eventHub = app.Services.GetRequiredService<IHubContext<EventHub>>();
    }
    
    [LambdaFunction()]
    public async Task VideoSuggestionWorker(SQSEvent sqsEvent)
    {
        foreach (var message in sqsEvent.Records)
        {
            var suggestVideo = JsonSerializer.Deserialize<SuggestVideoTopicCommand>(message.Body);
            
            Logger.LogInformation($"Processing message {message.MessageId}. User {suggestVideo.Username} and topic {suggestVideo.Topic}");

            await this.videoSuggestionService.Suggest(suggestVideo.Username, suggestVideo.Topic);
        }
    }
    
    [LambdaFunction()]
    public async Task TranslationWorker(SQSEvent sqsEvent)
    {
        foreach (var message in sqsEvent.Records)
        {
            var translationMessage = JsonSerializer.Deserialize<TranslateMessageCommand>(message.Body);
            
            Logger.LogInformation($"Processing message {message.MessageId}. User {translationMessage.Username} and translation of {translationMessage.Message} tp '{translationMessage.TranslateTo}");

            var translationResult = await this.translationService.Translate(translationMessage.Username, translationMessage.Message, translationMessage.TranslateTo);
            
            await this.translationHub.Clients.Groups(translationMessage.Username)
                .SendCoreAsync("ReceiveTranslationResponse", new object?[]{translationResult});
        }
    }
    
    [LambdaFunction()]
    public async Task EventStreamWorker(SQSEvent sqsEvent)
    {
        foreach (var message in sqsEvent.Records)
        {
            var eventData = JsonSerializer.Deserialize<EventData>(message.Body);

            var eventObject = JsonSerializer.Deserialize<EventWrapper>(eventData.DetailString);

            await this.eventHub.Clients.Client(eventObject.Metadata.Username).SendAsync("SendEvent", eventData.Source, eventData.DetailType, eventData.DetailString);
        }
    }
}