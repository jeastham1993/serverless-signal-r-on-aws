using System.Diagnostics;
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SignalR.Shared;
using SignalR.TranslationWorker.Services;

namespace SignalR.TranslationWorker;

public class VideoSuggestionWorker : BackgroundService
{
    private readonly AmazonSQSClient sqsClient;
    private readonly string suggestVideoQueueUrl;
    private readonly IVideoSuggestionService videoSuggestionService;
    private readonly ILogger<VideoSuggestionWorker> logger;

    public VideoSuggestionWorker(IConfiguration configuration, AmazonSQSClient sqsClient, IVideoSuggestionService videoSuggestionService, ILogger<VideoSuggestionWorker> logger)
    {
        this.videoSuggestionService = videoSuggestionService;
        this.logger = logger;
        this.sqsClient = sqsClient;
        this.suggestVideoQueueUrl = configuration["SUGGESTION_QUEUE_URL"];
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await this.sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest()
            {
                QueueUrl = suggestVideoQueueUrl,
                WaitTimeSeconds = 2,
                MaxNumberOfMessages = 10
            }, stoppingToken);

            var processedMessages = new List<Message>();

            foreach (var message in messages.Messages)
            {
                var suggestVideo = JsonSerializer.Deserialize<SuggestVideoTopicCommand>(message.Body);
                
                this.logger.LogInformation($"Processing video translation for {suggestVideo.Username} with topic {suggestVideo.Topic}");

                await this.videoSuggestionService.Suggest(suggestVideo.Username, suggestVideo.Topic, stoppingToken);

                processedMessages.Add(message);
            }

            if (processedMessages.Any())
            {
                await this.sqsClient.DeleteMessageBatchAsync(
                    suggestVideoQueueUrl,
                    processedMessages.Select(
                        message => new DeleteMessageBatchRequestEntry(
                            message.MessageId,
                            message.ReceiptHandle)).ToList(),
                    stoppingToken);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}