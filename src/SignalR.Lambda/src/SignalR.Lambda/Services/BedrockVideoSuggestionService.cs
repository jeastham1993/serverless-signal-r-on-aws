using System.Text.Json;
using Amazon.BedrockRuntime;
using AWS.Lambda.Powertools.Logging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SignalR.Lambda.Models;
using SignalR.Shared;

namespace SignalR.Lambda.Services;

public class BedrockVideoSuggestionService : IVideoSuggestionService
{
    private readonly AmazonBedrockRuntimeClient bedrockRuntimeClient;
    private readonly IHubContext<EventHub> signalRHub;
    private readonly ILogger<BedrockVideoSuggestionService> logger;

    public BedrockVideoSuggestionService(AmazonBedrockRuntimeClient bedrockRuntimeClient, IHubContext<EventHub> signalRHub, ILogger<BedrockVideoSuggestionService> logger)
    {
        this.bedrockRuntimeClient = bedrockRuntimeClient;
        this.signalRHub = signalRHub;
        this.logger = logger;
    }
    
    public async Task Suggest(string requestedByUser, string topic)
    {
        try
        {
            var prompt = new AnthropicClaudeV2($"I produce content on YouTube focused on building serverless applications on AWS using .NET, Java and Rust. I keep the videos short and focused on a single topic, no longer than 10 minutes. Based on the subject of '{topic}', could you provide an outline of 3 video topics? Including titles.");
        
            Logger.LogInformation("Sending prompt");

            var promptResponse = await this.bedrockRuntimeClient
                .InvokeModelWithResponseStreamAsync(prompt.AsStreamRequest());
            
            Logger.LogInformation($"Prompt Response {promptResponse.HttpStatusCode}");

            var lastUpdated = DateTime.Now;

            promptResponse.Body.ChunkReceived += async (sender, e) =>
            {
                Logger.LogInformation($"Sending chunked response to {requestedByUser}");
            
                var data = await JsonSerializer.DeserializeAsync<AnthropicClaudeResponse>(
                    e.EventStreamEvent.Bytes,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                            
                await this.signalRHub.Clients.Groups(requestedByUser)
                    .SendCoreAsync("VideoSuggestionResponse", new object?[]{data.Completion});

                lastUpdated = DateTime.Now;
            };
        
            Logger.LogInformation($"Starting processing. Last Updated is {lastUpdated}");
                        
            promptResponse.Body.StartProcessing();

            while ((DateTime.Now - lastUpdated).TotalSeconds < 5)
            {
                Logger.LogInformation($"Last Updated is {lastUpdated}");
            
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error sending video suggestion");
        }
    }
}