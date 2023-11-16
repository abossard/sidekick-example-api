using Dapr.Client;
using Dapr.Workflow;

namespace sidekick_example_api.Activities;

internal class UpdateInventoryActivity : WorkflowActivity<PaymentRequest, object>
{
    private static readonly string storeName = "statestore";
    private readonly DaprClient client;
    private readonly ILogger logger;

    public UpdateInventoryActivity(ILoggerFactory loggerFactory, DaprClient client)
    {
        logger = loggerFactory.CreateLogger<UpdateInventoryActivity>();
        this.client = client;
    }

    public override async Task<object> RunAsync(WorkflowActivityContext context, PaymentRequest req)
    {
        logger.LogInformation(
            "Checking Inventory for: Order# {requestId} for {amount} {item}",
            req.RequestId,
            req.Amount,
            req.ItemBeingPruchased);

        // Simulate slow processing
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Determine if there are enough Items for purchase
        var (original, originalETag) =
            await client.GetStateAndETagAsync<OrderPayload>(storeName, req.ItemBeingPruchased);
        int newQuantity = original.Quantity - req.Amount;

        if (newQuantity < 0)
        {
            logger.LogInformation(
                "Payment for request ID '{requestId}' could not be processed. Insufficient inventory.",
                req.RequestId);
            throw new InvalidOperationException();
        }

        // Update the statestore with the new amount of paper clips
        await client.SaveStateAsync(storeName, req.ItemBeingPruchased,
            new OrderPayload(Name: req.ItemBeingPruchased, TotalCost: req.Currency, Quantity: newQuantity));
        logger.LogInformation($"There are now: {newQuantity} {original.Name} left in stock");

        return null;
    }
}