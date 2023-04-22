using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace SignalR.Sample.Chat;

using Microsoft.AspNetCore.SignalR;

public class TranslationHub : Hub
{
    private readonly AmazonSQSClient _sqsClient;
    private readonly IConfiguration _configuration;

    public TranslationHub(AmazonSQSClient sqsClient, IConfiguration configuration)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
    }
    
    public async Task TranslateMessage(string translateTo, string message)
    {
        var messageToSend = new
        {
            translateTo = translateTo,
            message = message,
            connectionIdentifier = this.Context.ConnectionId
        };

        await this._sqsClient.SendMessageAsync(new SendMessageRequest(
            this._configuration["TranslationQueueUrl"],
            JsonSerializer.Serialize(message)));
    }
}