using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Microsoft.AspNetCore.SignalR;
using SignalR.Shared;
using SignalR.TranslationWorker.Models;

namespace SignalR.TranslationWorker.Services;

public class BedrockTranslationService : ITranslationService
{
    private readonly AmazonBedrockRuntimeClient bedrockRuntimeClient;
    private readonly IHubContext<TranslationHub> translationHub;

    public BedrockTranslationService(AmazonBedrockRuntimeClient bedrockRuntimeClient, IHubContext<TranslationHub> translationHub)
    {
        this.bedrockRuntimeClient = bedrockRuntimeClient;
        this.translationHub = translationHub;
    }

    public async Task<string> Translate(string requestedByUser, string textToTranslate, string translateTo, CancellationToken stoppingToken)
    {
        var prompt = new AnthropicClaudeV2($"Could you translate '{textToTranslate}' into '{translateTo}'. The target language is denoted by it's 2 digit ISO code.");

        var promptResponse = await this.bedrockRuntimeClient.InvokeModelWithResponseStreamAsync(prompt.AsStreamRequest());

        var fullString = new StringBuilder();
                        
        promptResponse.Body.ChunkReceived += async (sender, e) =>
        {
            var data = await JsonSerializer.DeserializeAsync<AnthropicClaudeResponse>(
                e.EventStreamEvent.Bytes,
                new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                            
            await this.translationHub.Clients.Groups(requestedByUser)
                .SendCoreAsync("ReceiveTranslationResponse", new object?[]{data.Completion}, stoppingToken);

            fullString.Append(data.Completion);
        };
                        
        promptResponse.Body.StartProcessing();

        return fullString.ToString();
    }
}