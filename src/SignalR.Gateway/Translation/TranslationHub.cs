using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.SignalR;

namespace SignalR.Gateway.Translation;

public class TranslationHub : Hub
{
    private readonly AmazonSQSClient _sqsClient;
    private readonly IConfiguration _configuration;

    public TranslationHub(AmazonSQSClient sqsClient, IConfiguration configuration)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
    }
    
    public async Task SendTranslationResponse(string connectionId, string translation)
    {
        await this.Clients.Client(connectionId).SendAsync("ReceiveTranslationResponse", translation);
    }
    
    public async Task TranslateMessage(string translateTo, string message)
    {
        var messageToSend = new TranslateMessageCommand()
        {
            TranslateTo = translateTo,
            Message = message,
            ConnectionId = this.Context.ConnectionId
        };

        await this._sqsClient.SendMessageAsync(new SendMessageRequest(
            this._configuration["TranslationQueueUrl"],
            JsonSerializer.Serialize(messageToSend)));
    }
}