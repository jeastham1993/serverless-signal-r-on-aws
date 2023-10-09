using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SignalR.Shared;
using SignalR.TranslationWorker.Models;

namespace SignalR.TranslationWorker.Services;

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
    
    public async Task Suggest(string requestedByUser, string topic, CancellationToken stoppingToken)
    {
        var prompt = new AnthropicClaudeV2($"I produce content on YouTube focused on building serverless applications on AWS using .NET, Java and Rust. I keep the videos short and focused on a single topic, no longer than 10 minutes. Based on the subject of '{topic}', could you provide an outline of 3 video topics? Including titles.");

        var promptResponse = await this.bedrockRuntimeClient.InvokeModelWithResponseStreamAsync(prompt.AsStreamRequest(), stoppingToken);

        promptResponse.Body.ChunkReceived += async (sender, e) =>
        {
            this.logger.LogInformation($"Sending chunked response to {requestedByUser}");
            
            var data = await JsonSerializer.DeserializeAsync<AnthropicClaudeResponse>(
                e.EventStreamEvent.Bytes,
                new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                            
            await this.signalRHub.Clients.Groups(requestedByUser)
                .SendCoreAsync("VideoSuggestionResponse", new object?[]{data.Completion}, stoppingToken);
        };
                        
        promptResponse.Body.StartProcessing();
    }
}