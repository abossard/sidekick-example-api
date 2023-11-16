using Dapr.Actors;

namespace sidekick_example_api;
public interface IController : IActor
{
    Task RegisterDeviceIdsAsync(string[] deviceIds);
    Task<string[]> ListRegisteredDeviceIdsAsync();
    Task TriggerAlarmForAllDetectors();
}

public class ControllerData
{
    public string[] DeviceIds { get; set; } = default!;
}