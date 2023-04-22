using System.Text.Json;
using Amazon.SQS;
using Microsoft.AspNetCore.SignalR;
using SignalR.Gateway.Translation;

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
            this._logger.LogInformation("Checking for message");
            
            var messages = await this._sqsClient.ReceiveMessageAsync(this._configuration["TranslationResponseQueueUrl"], stoppingToken);
            
            this._logger.LogInformation($"{messages.Messages.Count} message(s) found");

            foreach (var message in messages.Messages)
            {
                try
                {
                    var translationResponse = JsonSerializer.Deserialize<TranslateMessageResponse>(message.Body);

                    await this._translationHub.Clients.Client(translationResponse.ConnectionId)
                        .SendCoreAsync("ReceiveTranslationResponse", new object?[]{translationResponse.Translation}, stoppingToken);

                    await this._sqsClient.DeleteMessageAsync(this._configuration["TranslationResponseQueueUrl"],
                        message.ReceiptHandle, stoppingToken);
                }
                catch (Exception e)
                {
                    this._logger.LogError(e, "Failure processing message");
                }
            }
            
            this._logger.LogInformation("Waiting 5 seconds....");
            
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}