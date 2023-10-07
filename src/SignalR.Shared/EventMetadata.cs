using System.Text.Json.Serialization;

namespace SignalR.Shared;

public class EventMetadata
{
    [JsonPropertyName("userName")]
    public string Username { get; set; }
    
    [JsonPropertyName("parentTrace")]
    public string TraceId { get; set; }
    
    
    [JsonPropertyName("parentSpan")]
    public string SpanId { get; set; }
    
    [JsonPropertyName("eventType")]
    public string EventType { get; set; }
}