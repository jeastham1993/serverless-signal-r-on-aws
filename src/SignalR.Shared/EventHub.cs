using Microsoft.AspNetCore.SignalR;

namespace SignalR.Shared;

public class EventHub : Hub
{
    public async Task SendEvent(string eventSource, string eventType, string eventData)
    {
        await this.Clients.All.SendAsync("SendEvent", eventSource, eventData, eventData);
    }
}