using Dapr.Client;
using Dapr.Workflow;

namespace sidekick_example_api.Activities;

internal class ProcessPaymentActivity : WorkflowActivity<PaymentRequest, object>
{
    private readonly DaprClient client;
    private readonly ILogger logger;

    public ProcessPaymentActivity(ILoggerFactory loggerFactory, DaprClient client)
    {
        logger = loggerFactory.CreateLogger<ProcessPaymentActivity>();
        this.client = client;
    }

    public override async Task<object> RunAsync(WorkflowActivityContext context, PaymentRequest req)
    {
        logger.LogInformation(
            "Processing payment: {requestId} for {amount} {item} at ${currency}",
            req.RequestId,
            req.Amount,
            req.ItemBeingPruchased,
            req.Currency);

        // Simulate slow processing
        await Task.Delay(TimeSpan.FromSeconds(7));

        logger.LogInformation(
            "Payment for request ID '{requestId}' processed successfully",
            req.RequestId);

        return null;
    }
}