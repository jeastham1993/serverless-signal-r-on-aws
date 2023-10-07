using System.Diagnostics;
using SignalR.Shared;

namespace SignalR.TranslationWorker;

public static class TelemetryExtensions
{
    public static Activity AddEventData(this Activity source, EventData data)
    {
        source.AddTag("event-source", data.Source);
        source.AddTag("event-type", data.DetailType);
        
        return source;
    }
    
    public static Activity AddMetadata(this Activity source, EventMetadata data)
    {
        source.AddTag("user-name", data.Username);
        source.AddTag("linked-trace", data.TraceId);
        source.AddTag("event-type", data.EventType);

        return source;
    }
}