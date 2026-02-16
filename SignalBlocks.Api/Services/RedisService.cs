// در RedisService.cs
using SignalBlocks.Writer.Core.Models;
using System.Text.Json;

public async Task SetLastTagStateAsync(string tagId, TagValue tag)
{
    var data = new
    {
        value = tag.Value,
        quality = tag.Quality,
        timestamp = tag.Timestamp.ToString("O"),
        source = tag.Source ?? "unknown",
        application = tag.Application ?? "default"
    };

    var json = JsonSerializer.Serialize(data);
    //await _db.StringSetAsync($"tag:last:{tagId}", json, TimeSpan.FromHours(24));
    await _db.HashSetAsync($"tag:last:{tagId}", new HashEntry[]
{
    new("value", tag.Value),
    new("quality", tag.Quality),
    new("timestamp", tag.Timestamp.ToString("O")),
    new("source", tag.Source ?? ""),
    new("application", tag.Application ?? "")
});
}