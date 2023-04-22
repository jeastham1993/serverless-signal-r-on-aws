using System.Threading.Tasks;
using Amazon.SQS;
using Microsoft.AspNetCore.SignalR;

namespace TranslationProcessor;

public class TranslationHub : Hub
{
    private readonly AmazonSQSClient _sqsClient;

    public TranslationHub(AmazonSQSClient sqsClient)
    {
        _sqsClient = sqsClient;
    }

    public override Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext().Request.Query["username"];

        this.Groups.AddToGroupAsync(this.Context.ConnectionId, username);

        return base.OnConnectedAsync();
    }

    public async Task SendTranslationResponse(string connectionId, string translation)
    {
        await this.Clients.Client(connectionId).SendAsync("ReceiveTranslationResponse", translation);
    }
}