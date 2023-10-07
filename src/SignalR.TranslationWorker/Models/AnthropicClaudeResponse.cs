namespace SignalR.TranslationWorker.Models;

public class AnthropicClaudeResponse
{
    public string? Completion { get; set; }

    public string? Stop_Reason { get; set; }

    public string? GetResponse()
    {
        return Completion;
    }

    public string? GetStopReason()
    {
        return Stop_Reason;
    }
}