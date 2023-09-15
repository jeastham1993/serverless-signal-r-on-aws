using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;

using SignalR.Shared;
using TranslationProcessor;

using Amazon.Translate;
using Amazon.Translate.Model;

namespace SignalR.Gateway;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TranslationQueueWorker : BackgroundService
{
    private readonly AmazonSQSClient sqsClient;
    private readonly AmazonTranslateClient translateClient;
    private readonly IConfiguration configuration;
    private readonly ILogger<TranslationQueueWorker> logger;
    private readonly IHubContext<TranslationHub> translationHub;
    private readonly string queueUrl;

    public TranslationQueueWorker(AmazonSQSClient sqsClient, AmazonTranslateClient translateClient, IConfiguration configuration, ILogger<TranslationQueueWorker> logger,
        IHubContext<TranslationHub> translationHub)
    {
        this.sqsClient = sqsClient;
        this.translateClient = translateClient;
        this.configuration = configuration;
        this.logger = logger;
        this.translationHub = translationHub;
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

                        var translationResponse = await translateClient.TranslateTextAsync(new TranslateTextRequest
                        {
                            SourceLanguageCode = "en",
                            TargetLanguageCode = translationRequest.TranslateTo.ToLower(),
                            Text = translationRequest.Message
                        }, stoppingToken);
            
                        this.logger.LogInformation("Sending message to connected client");
                        
                        await this.translationHub.Clients.Groups(translationRequest.Username)
                            .SendCoreAsync("ReceiveTranslationResponse", new object?[]{translationResponse.TranslatedText}, stoppingToken);

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