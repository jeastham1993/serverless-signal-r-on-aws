using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SignalR.Shared;

public class TranslationHub : Hub
{
    private readonly AmazonSQSClient _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TranslationHub> _logger;

    public TranslationHub(AmazonSQSClient sqsClient, IConfiguration configuration, ILogger<TranslationHub> logger)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext().Request.Query["username"];

        this._logger.LogInformation($"User {username[0]} connected");

        this.Groups.AddToGroupAsync(this.Context.ConnectionId, username);

        return base.OnConnectedAsync();
    }

    public async Task SendTranslationResponse(string connectionId, string translation)
    {
        await this.Clients.Client(connectionId).SendAsync("ReceiveTranslationResponse", translation);
    }
    
    public async Task TranslateMessage(string username, string translateTo, string message)
    {
        _logger.LogInformation($"Received request for user {username}");
        
        var messageToSend = new TranslateMessageCommand()
        {
            Username = username,
            TranslateTo = translateTo,
            Message = message,
            ConnectionId = this.Context.ConnectionId
        };

        var queueUrl = Environment.GetEnvironmentVariable("TRANSLATION_QUEUE_URL");
        
        _logger.LogInformation($"Sending request to {queueUrl}");

        await this._sqsClient.SendMessageAsync(new SendMessageRequest(
            queueUrl,
            JsonSerializer.Serialize(messageToSend)));
    }
}