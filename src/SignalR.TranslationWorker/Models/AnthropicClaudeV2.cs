using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime.Model;

namespace SignalR.TranslationWorker.Models;

public class AnthropicClaudeV2
{
    private string prompt { get; set; }
    public AnthropicClaudeV2(string prompt)
    {
        this.prompt = prompt;
    }

    public InvokeModelWithResponseStreamRequest AsStreamRequest()
    {
        InvokeModelWithResponseStreamRequest request = new()
        {
            ModelId = "anthropic.claude-v2",
            ContentType = "application/json"
        };

        StringBuilder promtValueBuilder = new();
        string label = "Human";
        promtValueBuilder.Append(label);
        promtValueBuilder.Append(": ");
        promtValueBuilder.Append(this.prompt);
        promtValueBuilder.AppendLine();

        if (!promtValueBuilder.ToString().EndsWith('.'))
            promtValueBuilder.Append('.');
        promtValueBuilder.AppendLine();
        promtValueBuilder.AppendLine("Assistant: ");

        request.Accept = "application/json";
        request.Body = new MemoryStream(
            Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(new
                {
                    prompt = promtValueBuilder.ToString(),
                    max_tokens_to_sample = 300
                })
            )
        );

        return request;
    }
}