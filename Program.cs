using Dapr.Client;
using Dapr.Workflow;
using sidekick_example_api;
using sidekick_example_api.Activities;
using sidekick_example_api.Workflows;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDaprSidekick();
const string StoreName = "statestore";
const string DaprWorkflowComponent = "dapr";

builder.Services.AddDaprWorkflow(options =>
{
    // Note that it's also possible to register a lambda function as the workflow
    // or activity implementation instead of a class.
    options.RegisterWorkflow<OrderProcessingWorkflow>();

    // These are the activities that get invoked by the workflow(s).
    options.RegisterActivity<NotifyActivity>();
    options.RegisterActivity<ReserveInventoryActivity>();
    options.RegisterActivity<ProcessPaymentActivity>();
    options.RegisterActivity<UpdateInventoryActivity>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (WorkflowEngineClient workflowClient) =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        using var daprClient = new DaprClientBuilder().Build();

// Generate a unique ID for the workflow
        var orderId = Guid.NewGuid().ToString()[..8];
        var itemToPurchase = "Cars";
        var ammountToPurchase = 10;
        await daprClient.SaveStateAsync<OrderPayload>(StoreName, itemToPurchase,
            new OrderPayload(Name: itemToPurchase, TotalCost: 15000, Quantity: 100));
// Populate the store with items
        // RestockInventory(itemToPurchase);

// Construct the order
        var orderInfo = new OrderPayload(itemToPurchase, 15000, ammountToPurchase);

// Start the workflow
        Console.WriteLine("Starting workflow {0} purchasing {1} {2}", orderId, ammountToPurchase, itemToPurchase);

        await daprClient.StartWorkflowAsync(
            workflowComponent: DaprWorkflowComponent,
            workflowName: nameof(OrderProcessingWorkflow),
            input: orderInfo,
            instanceId: orderId).ConfigureAwait(false);

// Wait for the workflow to start and confirm the input
        var state = await daprClient.WaitForWorkflowStartAsync(
            instanceId: orderId,
            workflowComponent: DaprWorkflowComponent).ConfigureAwait(false);

        Console.WriteLine("Your workflow has started. Here is the status of the workflow: {0}", state.RuntimeStatus);

// Wait for the workflow to complete
        state = await daprClient.WaitForWorkflowCompletionAsync(
            instanceId: orderId,
            workflowComponent: DaprWorkflowComponent).ConfigureAwait(false);

        Console.WriteLine("Workflow Status: {0}", state.RuntimeStatus);

        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}