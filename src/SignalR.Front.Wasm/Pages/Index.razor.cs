using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Front.Wasm.Pages;

public partial class IndexBase : ComponentBase
{
    private HubConnection connection;
    private const string THINKING_TEXT = "Thinking.....";
    
    public string ConnectionUrl { get; set; }
    public string ConnectAs { get; set; }
    public string Message { get; set; }
    
    public string TranslateText { get; set; }
    
    public string TranslateToLanguage { get; set; }
    
    public List<string> Responses { get; set; }

    public string ResponseData { get; set; } = "";

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
                if (this.ResponseData == THINKING_TEXT)
                {
                    this.ResponseData = "";
                }
                
                this.ResponseData = $"{this.ResponseData}{received}";
                
                InvokeAsync(this.StateHasChanged);
            });

        this.Message = "Connected";
    }

    public async Task Translate()
    {
        if (!string.IsNullOrEmpty(this.ResponseData))
        {
            this.Responses.Add(this.ResponseData);            
        }

        this.ResponseData = THINKING_TEXT;

        await connection.InvokeAsync("TranslateMessage", this.ConnectAs,this.TranslateToLanguage, this.TranslateText);
    }
}