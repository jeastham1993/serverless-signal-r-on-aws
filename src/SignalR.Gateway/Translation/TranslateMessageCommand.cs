using System.Text.Json.Serialization;

namespace SignalR.Gateway.Translation;

public record TranslateMessageCommand
{
    [JsonPropertyName("translateTo")]
    public string TranslateTo { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; }
    
    [JsonPropertyName("connectionId")]
    
    public string ConnectionId { get; set; }
}