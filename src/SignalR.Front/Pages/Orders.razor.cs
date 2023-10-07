using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Front.Services;

namespace SignalR.Front.Pages;

public class OrdersBase : ComponentBase
{
    public string OrderNumber { get; set; }
    
    public string OrderAs { get; set;  }

    [Inject]
    public IOrderWorkflowService WorkflowService { get; set; }

    public OrdersBase()
    {
        OrderNumber = "";
        OrderAs = "";
    }

    public async Task Order()
    {
        await this.WorkflowService.StartOrderWorkflow(OrderNumber, OrderAs);

        OrderNumber = "";
    }
}