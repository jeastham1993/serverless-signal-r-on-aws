using System.Diagnostics;

namespace SignalR.Front;

public static class TelemetryConstants
{
    public const string ServiceName = "FrontEnd";
}

public static class Telemetry
{
    public static readonly ActivitySource MyActivitySource = new(TelemetryConstants.ServiceName);
}