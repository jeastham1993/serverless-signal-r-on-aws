using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.SignalR;
using SignalR.Shared;
using TranslationProcessor;

namespace SignalR.Gateway;

public class TranslationResponseWorker : BackgroundService
{
    private readonly AmazonSQSClient _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<TranslationHub> _translationHub;
    private readonly ILogger<TranslationResponseWorker> _logger;

    public TranslationResponseWorker(AmazonSQSClient sqsClient, IConfiguration configuration, ILogger<TranslationResponseWorker> logger, IHubContext<TranslationHub> translationHub)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _logger = logger;
        _translationHub = translationHub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var queueUrl = this._configuration["TRANSLATION_RESPONSE_QUEUE_URL"];
            
                this._logger.LogInformation($"Attempting to connect to queue URL {queueUrl}");

                var messages = await this._sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest()
                {
                    QueueUrl = queueUrl,
                    WaitTimeSeconds = 2,
                    MaxNumberOfMessages = 10
                }, stoppingToken);

                foreach (var message in messages.Messages)
                {
                    this._logger.LogInformation("Processing message");
                
                    try
                    {
                        var translationResponse = JsonSerializer.Deserialize<TranslateMessageResponse>(message.Body);

                        await this._translationHub.Clients.Groups(translationResponse.Username)
                            .SendCoreAsync("ReceiveTranslationResponse", new object?[]{translationResponse.Translation}, stoppingToken);

                        await this._sqsClient.DeleteMessageAsync(queueUrl,
                            message.ReceiptHandle, stoppingToken);
                    }
                    catch (Exception e)
                    {
                        this._logger.LogError(e, "Failure processing message");
                    }
                }
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "Failure processing messages");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}