using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SignalR.Shared;

public class EventHub : Hub
{
    private readonly IConfiguration _configuration;
    private AmazonSQSClient _sqsClient;
    private readonly ILogger<EventHub> _logger;

    public EventHub(IConfiguration configuration, AmazonSQSClient sqsClient, ILogger<EventHub> logger)
    {
        _configuration = configuration;
        _sqsClient = sqsClient;
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext().Request.Query["username"];

        this.Groups.AddToGroupAsync(this.Context.ConnectionId, username);

        return base.OnConnectedAsync();
    }
    
    public async Task SendEvent(string eventSource, string eventType, string eventData)
    {
        await this.Clients.All.SendAsync("SendEvent", eventSource, eventData, eventData);
    }
    
    public async Task GenerateVideoSuggestions(string username, string topic)
    {
        this._logger.LogInformation($"Received video suggestion generation request for {username} and topic {topic}");
        
        var messageToSend = new SuggestVideoTopicCommand()
        {
            Username = username,
            Topic = topic
        };

        var queueUrl = this._configuration["SUGGESTION_QUEUE_URL"];

        await this._sqsClient.SendMessageAsync(new SendMessageRequest(
            queueUrl,
            JsonSerializer.Serialize(messageToSend)));
    }
}