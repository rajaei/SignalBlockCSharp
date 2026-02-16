namespace SignalBlocks.Writer.Core.Models;

public class KepwareMessage
{
    public string? DeviceId { get; set; }
    public string? GlobalSource { get; set; }
    public string? GlobalApplication { get; set; }
    public List<TagValue> Tags { get; set; } = new();
}