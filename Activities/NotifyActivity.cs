using Dapr.Workflow;

namespace sidekick_example_api.Activities;

internal record Notification(string Message);

internal class NotifyActivity : WorkflowActivity<Notification, object>
{
    private readonly ILogger logger;

    public NotifyActivity(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<NotifyActivity>();
    }

    public override Task<object> RunAsync(WorkflowActivityContext context, Notification notification)
    {
        logger.LogInformation(notification.Message);

        return Task.FromResult<object>(null);
    }
}