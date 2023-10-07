namespace SignalR.Front.Services;

public interface IOrderWorkflowService
{
    Task StartOrderWorkflow(string orderNumber, string orderAs);
}