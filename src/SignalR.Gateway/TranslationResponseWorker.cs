using System.Text.Json;
using Amazon.SQS;
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
            var messages = await this._sqsClient.ReceiveMessageAsync(this._configuration["TranslationResponseQueueUrl"], stoppingToken);

            foreach (var message in messages.Messages)
            {
                this._logger.LogInformation("Processing message");
                
                try
                {
                    var translationResponse = JsonSerializer.Deserialize<TranslateMessageResponse>(message.Body);

                    await this._translationHub.Clients.Groups(translationResponse.Username)
                        .SendCoreAsync("ReceiveTranslationResponse", new object?[]{translationResponse.Translation}, stoppingToken);

                    await this._sqsClient.DeleteMessageAsync(this._configuration["TranslationResponseQueueUrl"],
                        message.ReceiptHandle, stoppingToken);
                }
                catch (Exception e)
                {
                    this._logger.LogError(e, "Failure processing message");
                }
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}