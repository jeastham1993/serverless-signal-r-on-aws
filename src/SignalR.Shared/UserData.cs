using System.Text.Json.Serialization;

namespace SignalR.Shared;

public class UserData
{
    [JsonPropertyName("userName")]
    public string Username { get; set; }
}