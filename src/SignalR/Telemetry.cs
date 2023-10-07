using System.Diagnostics;

namespace SignalR;

public static class TelemetryConstants
{
    public const string ServiceName = "SignalRGateway";
}

public static class Telemetry
{
    public static readonly ActivitySource MyActivitySource = new(TelemetryConstants.ServiceName);
}