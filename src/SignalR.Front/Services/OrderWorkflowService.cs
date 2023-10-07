using System.Diagnostics;
using System.Text.Json;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;

namespace SignalR.Front.Services;

public class OrderWorkflowService : IOrderWorkflowService
{
    private readonly AmazonStepFunctionsClient _stepFunctionsClient;

    public OrderWorkflowService(AmazonStepFunctionsClient stepFunctionsClient)
    {
        _stepFunctionsClient = stepFunctionsClient;
    }
    
    public async Task StartOrderWorkflow(string orderNumber, string orderAs)
    {
        using var activity = Telemetry.MyActivitySource.StartActivity(ActivityKind.Client, name: "Start Order Workflow");
        activity.AddTag("order-number", orderNumber);
        activity.AddTag("username", orderAs);

        await this._stepFunctionsClient.StartExecutionAsync(new StartExecutionRequest
        {
            Input = JsonSerializer.Serialize(new OrderWorkflowInput(orderNumber, orderAs, activity.TraceId.ToString(), activity.SpanId.ToString())),
            Name = $"{orderNumber}{Guid.NewGuid()}",
            StateMachineArn = Environment.GetEnvironmentVariable("ORDER_WORKFLOW_ARN"),
        });
    }
}

public record OrderWorkflowInput(string orderNumber, string orderAs, string parentTraceId, string parentSpanId);