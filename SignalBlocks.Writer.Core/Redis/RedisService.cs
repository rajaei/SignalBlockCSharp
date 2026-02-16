using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using SignalBlocks.Writer.Core.Models;

namespace SignalBlocks.Writer.Core.Redis;

public class RedisService : IDisposable
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService(IOptions<RedisOptions> options)
    {
        var cfg = options.Value;
        var connectionString = cfg.Password switch
        {
            "" => cfg.ConnectionString,
            _ => $"{cfg.ConnectionString},password={cfg.Password}"
        };

        _redis = ConnectionMultiplexer.Connect(connectionString);
        _db = _redis.GetDatabase(cfg.Database);
    }

    // ذخیره index تگ (tag_name → tag_index)
    public async Task SetTagIndexAsync(string tagName, int tagIndex)
    {
        await _db.StringSetAsync($"tag:index:{tagName}", tagIndex, TimeSpan.FromDays(30));
    }

    public async Task<int?> GetTagIndexAsync(string tagName)
    {
        var value = await _db.StringGetAsync($"tag:index:{tagName}");
        return value.HasValue && int.TryParse(value.ToString(), out int idx) ? idx : null;
    }

    public async Task SetLastTagStateAsync(string tagId, double value, string quality, DateTimeOffset unixTimestamp, string? source, string? application)
    {
        var data = new
        {
            value,
            quality,
            timestamp = unixTimestamp,
            source = source ?? "unknown",
            application = application ?? "default"
        };

        var json = JsonSerializer.Serialize(data);
        await _db.StringSetAsync($"tag:last:{tagId}", json, TimeSpan.FromHours(24));
    }

    public async Task<TagValue?> GetLastTagStateAsync(string tagId)
    {
        var json = await _db.StringGetAsync($"tag:last:{tagId}");
        if (json.IsNullOrEmpty) return null;

        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json.ToString());
            if (data == null) return null;

            return new TagValue
            {
                TagId = tagId,
                Value = double.TryParse(data["value"]?.ToString(), out var v) ? v : 0,
                Quality = data["quality"]?.ToString() ?? "unknown",
                Timestamp = DateTimeOffset.TryParse(data["timestamp"]?.ToString(), out var ts)
                    ? ts
                    : DateTimeOffset.MinValue,
                Source = data["source"]?.ToString(),
                Application = data["application"]?.ToString()
            };
        }
        catch (Exception ex)
        {
            // لاگ کن و null برگردان
            Console.WriteLine($"Error deserializing last state for {tagId}: {ex.Message}");
            return null;
        }
    }
    public void Dispose()
    {
        _redis?.Dispose();
    }
}