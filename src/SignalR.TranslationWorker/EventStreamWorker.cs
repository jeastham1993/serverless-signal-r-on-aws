using System.Text.Json;
using Amazon.Lambda.CloudWatchEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.SignalR;
using SignalR.Shared;

namespace SignalR.TranslationWorker;

public class EventStreamWorker : BackgroundService
{
    private readonly AmazonSQSClient sqsClient;
    private readonly ILogger<EventStreamWorker> logger;
    private readonly string eventStreamQueueUrl;
    private readonly IHubContext<EventHub> eventHub;

    public EventStreamWorker(IConfiguration configuration, AmazonSQSClient sqsClient, IHubContext<EventHub> eventHub, ILogger<EventStreamWorker> logger)
    {
        this.eventHub = eventHub;
        this.logger = logger;
        this.sqsClient = sqsClient;
        this.eventStreamQueueUrl = configuration["EVENT_STREAM_QUEUE_URL"];
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await this.sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest()
            {
                QueueUrl = eventStreamQueueUrl,
                WaitTimeSeconds = 2,
                MaxNumberOfMessages = 10
            }, stoppingToken);

            var processedMessages = new List<Message>();

            foreach (var message in messages.Messages)
            {
                var eventData = JsonSerializer.Deserialize<EventData>(message.Body);
                
                this.logger.LogInformation($"Source: {eventData.Source}");
                this.logger.LogInformation($"DetailType: {eventData.DetailType}");
                this.logger.LogInformation($"Detail: {eventData.DetailString}");

                await this.eventHub.Clients.All.SendAsync("SendEvent", eventData.Source, eventData.DetailType, eventData.DetailString);

                processedMessages.Add(message);
            }

            if (processedMessages.Any())
            {
                await this.sqsClient.DeleteMessageBatchAsync(
                    eventStreamQueueUrl,
                    processedMessages.Select(
                        message => new DeleteMessageBatchRequestEntry(
                            message.MessageId,
                            message.ReceiptHandle)).ToList(),
                    stoppingToken);
            }
        }
    }
}