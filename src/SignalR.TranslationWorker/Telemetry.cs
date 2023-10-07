using System.Diagnostics;

namespace SignalR.TranslationWorker;

public static class TelemetryConstants
{
    public const string ServiceName = "TranslationWorker";
}

public static class Telemetry
{
    public static readonly ActivitySource MyActivitySource = new(TelemetryConstants.ServiceName);
}