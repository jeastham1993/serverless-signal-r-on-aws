using System.Text.Json.Serialization;

namespace SignalR.Gateway.Translation;

public record TranslateMessageResponse
{
    [JsonPropertyName("translation")]
    public string Translation { get; set; }
    
    [JsonPropertyName("connectionId")]
    public string ConnectionId { get; set; }
    
    [JsonPropertyName("username")]
    public string Username { get; set; }
}