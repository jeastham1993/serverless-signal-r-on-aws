using System.Text.Json.Serialization;

namespace SignalR.TranslationWorker.Models;

public class CohereCommandResponse
{
    [JsonPropertyName("generations")]
    public Generation[] Generations { get; set; }
}

public class Generation
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}