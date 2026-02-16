namespace SignalBlocks.Writer.Core.Models;

public class TagValue
{
    public string TagId { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Quality { get; set; } = "good";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }          // منبع (اختیاری)
    public string? Application { get; set; }     // کاربرد/کانتکست (اختیاری)
}