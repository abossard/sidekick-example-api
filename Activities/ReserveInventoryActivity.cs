using Dapr.Client;
using Dapr.Workflow;

namespace sidekick_example_api.Activities;

internal class ReserveInventoryActivity : WorkflowActivity<InventoryRequest, InventoryResult>
{
    private static readonly string storeName = "statestore";
    private readonly DaprClient client;
    private readonly ILogger logger;

    public ReserveInventoryActivity(ILoggerFactory loggerFactory, DaprClient client)
    {
        logger = loggerFactory.CreateLogger<ReserveInventoryActivity>();
        this.client = client;
    }

    public override async Task<InventoryResult> RunAsync(WorkflowActivityContext context, InventoryRequest req)
    {
        logger.LogInformation(
            "Reserving inventory for order {requestId} of {quantity} {name}",
            req.RequestId,
            req.Quantity,
            req.ItemName);

        OrderPayload orderResponse;
        string key;

        // Ensure that the store has items
        (orderResponse, key) = await client.GetStateAndETagAsync<OrderPayload>(storeName, req.ItemName);

        // Catch for the case where the statestore isn't setup
        if (orderResponse == null)
            // Not enough items.
            return new InventoryResult(false, orderResponse);

        logger.LogInformation(
            "There are: {requestId}, {name} available for purchase",
            orderResponse.Quantity,
            orderResponse.Name);

        // See if there're enough items to purchase
        if (orderResponse.Quantity >= req.Quantity)
        {
            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(2));

            return new InventoryResult(true, orderResponse);
        }

        // Not enough items.
        return new InventoryResult(false, orderResponse);
    }
}