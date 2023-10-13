namespace SignalR.Lambda.Services;

public interface IVideoSuggestionService
{
    Task Suggest(string requestedByUser, string topic);
}