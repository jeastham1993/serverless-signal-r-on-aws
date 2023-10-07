using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalR.Shared;

public class EventHub : Hub
{
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
}