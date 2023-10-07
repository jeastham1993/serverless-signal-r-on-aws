using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.Translate;
using Amazon.Translate.Model;
using Microsoft.AspNetCore.SignalR;
using SignalR.Shared;
using SignalR.TranslationWorker.Models;
using SignalR.TranslationWorker.Services;

namespace SignalR.TranslationWorker;

public class TranslationQueueWorker : BackgroundService
{
    private readonly AmazonSQSClient sqsClient;
    private readonly ITranslationService translationService;
    private readonly IConfiguration configuration;
    private readonly ILogger<TranslationQueueWorker> logger;
    private readonly IHubContext<TranslationHub> translationHub;
    private readonly string queueUrl;

    public TranslationQueueWorker(AmazonSQSClient sqsClient, IConfiguration configuration, ILogger<TranslationQueueWorker> logger,
        IHubContext<TranslationHub> translationHub, ITranslationService translationService)
    {
        this.sqsClient = sqsClient;
        this.configuration = configuration;
        this.logger = logger;
        this.translationHub = translationHub;
        this.translationService = translationService;
        this.queueUrl = this.configuration["TRANSLATION_QUEUE_URL"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messages = await this.sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest()
                {
                    QueueUrl = queueUrl,
                    WaitTimeSeconds = 2,
                    MaxNumberOfMessages = 10
                }, stoppingToken);

                var processedMessages = new List<Message>();

                foreach (var message in messages.Messages)
                {
                    try
                    {
                        var translationRequest = JsonSerializer.Deserialize<TranslateMessageCommand>(message.Body);
        
                        this.logger.LogInformation($"Processing translation for {translationRequest.Username} and connection {translationRequest.ConnectionId}");

                        var translationResult = await this.translationService.Translate(translationRequest.Username, translationRequest.Message,
                            translationRequest.TranslateTo, stoppingToken);

                        if (string.IsNullOrEmpty(translationResult))
                        {
                            this.logger.LogInformation("Sending message to connected client");
                            
                            await this.translationHub.Clients.Groups(translationRequest.Username)
                                .SendCoreAsync("ReceiveTranslationResponse", new object?[]{translationResult}, stoppingToken);
                        }

                        processedMessages.Add(message);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, $"Failure processing message");
                        this.logger.LogInformation(message.Body);
                    }
                }

                if (processedMessages.Any())
                {
                    await this.sqsClient.DeleteMessageBatchAsync(
                        this.configuration["TRANSLATION_QUEUE_URL"],
                        processedMessages.Select(
                            message => new DeleteMessageBatchRequestEntry(
                                message.MessageId,
                                message.ReceiptHandle)).ToList(),
                        stoppingToken);
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Failure processing messages");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}