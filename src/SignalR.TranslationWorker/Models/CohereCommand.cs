namespace SignalR.TranslationWorker.Models;

public class CohereCommand
{
    public CohereCommand(string prompt)
    {
        this.prompt = prompt;
    }
    
    public string prompt { get; set; }
    public int max_tokens { get; set; } = 400;
    public double temperature { get; set; } = 0.7;
    public double p { get; set; } = 0.01;
    public int k { get; set; } = 0;
    public object[] stop_sequences { get; set; } = new object[0];
    public string return_likelihoods { get; set; } = "NONE";
}

