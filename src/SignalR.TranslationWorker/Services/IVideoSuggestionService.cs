namespace SignalR.TranslationWorker.Services;

public interface IVideoSuggestionService
{
    Task Suggest(string requestedByUser, string topic, CancellationToken stoppingToken);
}