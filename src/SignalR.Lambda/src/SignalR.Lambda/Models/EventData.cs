using System.Text.Json;
using System.Text.Json.Serialization;

namespace SignalR.Lambda.Models;

public record EventData
{
    [JsonPropertyName("source")]
    public string Source { get; set; }
    
    [JsonPropertyName("detail-type")]
    public string DetailType { get; set; }
    
    [JsonPropertyName("detail")]
    public object Detail { get; set; }

    public string DetailString => JsonSerializer.Serialize(this.Detail);
}