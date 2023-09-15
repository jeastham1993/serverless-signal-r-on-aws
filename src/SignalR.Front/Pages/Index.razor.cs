namespace SignalR.Front.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

public partial class IndexBase : ComponentBase
{
    private HubConnection connection;
    
    public string ConnectionUrl { get; set; }
    public string ConnectAs { get; set; }
    public string Message { get; set; }
    
    public string TranslateText { get; set; }
    
    public string TranslateToLanguage { get; set; }
    
    public List<string> Responses { get; set; }

    public IndexBase()
    {
        Responses = new List<string>();
    }

    public async Task Connect()
    {
        this.Message = "Connecting...";
        
        connection = new HubConnectionBuilder()
            .WithUrl($"{this.ConnectionUrl}/translationHub?username={ConnectAs}")
            .Build();

        connection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0,5) * 1000);
            await connection.StartAsync();
        };

        await this.connection.StartAsync();

        this.connection.On<string>(
            "ReceiveTranslationResponse",
            (received) =>
            {
                Responses.Add(received);
                
                InvokeAsync(this.StateHasChanged);
            });

        this.Message = "Connected";
    }

    public async Task Translate()
    {
        await connection.InvokeAsync("TranslateMessage", this.ConnectAs,this.TranslateToLanguage, this.TranslateText);
    }
}