using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Front.Wasm.Pages;

public class EventStreamBase : ComponentBase
{
    private HubConnection connection;
    
    public string ConnectionUrl { get; set; }
    public string ConnectAs { get; set; }

    public List<string> Responses { get; set; }

    public string Message { get; set; }

    public EventStreamBase()
    {
        Responses = new List<string>();
    }

    public async Task Connect()
    {
        this.Message = "Connecting...";
        
        connection = new HubConnectionBuilder()
            .WithUrl($"{this.ConnectionUrl}/events?username={ConnectAs}")
            .Build();

        connection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0,5) * 1000);
            await connection.StartAsync();
        };

        await this.connection.StartAsync();

        this.connection.On<string, string, string>(
            "SendEvent",
            (source, eventType, eventData) =>
            {
                Responses.Add($"{source} - {eventType} = {eventData}");
                
                InvokeAsync(this.StateHasChanged);
            });

        this.Message = "Connected";
    }
}