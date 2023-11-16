using Dapr.Actors;

namespace sidekick_example_api;
public interface ISmartDevice : IActor
{
    Task<string> SetDataAsync(SmartDeviceData device);
    Task<SmartDeviceData> GetDataAsync();
    Task DetectSmokeAsync();
    Task SoundAlarm();
    Task ClearAlarm();
}

public class SmartDeviceData
{   
    public string Status { get; set; } = default!;
    public string Location { get; set; } = default!;

    public override string ToString()
    {
        return $"Location: {this.Location}, Status: {this.Status}";
    }
}
