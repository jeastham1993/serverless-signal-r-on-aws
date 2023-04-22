namespace SignalR.Sample.Chat;

using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await this.Clients.Others.SendAsync("ReceiveMessage", user, message);
    }
}