namespace SignalR.Shared;

public record SuggestVideoTopicCommand
{
    public string Username { get; set; }
    
    public string Topic { get; set; }
}