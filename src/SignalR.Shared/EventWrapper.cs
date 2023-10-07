using System.Text.Json.Serialization;

namespace SignalR.Shared;

public class EventWrapper
{
    [JsonPropertyName("metadata")]
    public EventMetadata Metadata { get; set; }
    
    [JsonPropertyName("data")]
    
    public object Data { get; set; }
}