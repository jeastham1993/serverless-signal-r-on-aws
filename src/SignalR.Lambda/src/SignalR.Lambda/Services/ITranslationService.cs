namespace SignalR.Lambda.Services;

public interface ITranslationService
{
    Task<string> Translate(string requestedByUser, string textToTranslate, string translateTo);
}