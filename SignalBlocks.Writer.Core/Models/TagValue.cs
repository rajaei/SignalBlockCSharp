namespace SignalBlocks.Writer.Core.Models;

public class TagValue
{
    public string TagId { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Quality { get; set; } = "good";
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string? Source { get; set; }          // منبع (اختیاری)
    public string? Application { get; set; }     // کاربرد/کانتکست (اختیاری)

    // متد کمکی برای تبدیل به DateTimeOffset (اگر لازم شد)
    public DateTimeOffset GetTimestampAsDateTimeOffset()
    {
        return Timestamp; // اگر ثانیه است
        // یا DateTimeOffset.FromUnixTimeMilliseconds(Timestamp); اگر میلی‌ثانیه است
    }
}