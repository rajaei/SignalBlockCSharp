using Microsoft.AspNetCore.Mvc;
using SignalBlocks.Writer.Core.Interfaces;
using SignalBlocks.Writer.Core.Models;
using SignalBlocks.Writer.Core.Redis;
using System.Text.Json;

namespace SignalBlocks.Writer.Controllers;

[ApiController]
[Route("api/write")]
public class WriteController : ControllerBase
{
    private readonly IWriter _writer;
    private readonly RedisService _redis;

    public WriteController(IWriter writer, RedisService redis)
    {
        _writer = writer;
        _redis = redis;
    }

    [HttpPost("batch")]
    public async Task<IActionResult> WriteBatch([FromBody] KepwareMessage message)
    {
        if (message?.Tags == null || message.Tags.Count == 0)
            return BadRequest("No tags received");

        foreach (var tag in message.Tags)
        {
            // اعمال پیش‌فرض source و application
            tag.Source ??= message.GlobalSource ?? "unknown";
            tag.Application ??= message.GlobalApplication ?? "default";

            // تبدیل Unix timestamp به DateTimeOffset (اگر لازم شد)
            // اگر timestamp عدد Unix باشه، می‌تونی اینجا تبدیل کنی
            var tagTime = tag.Timestamp; // اگر ثانیه است

            // چک آخرین وضعیت با Redis
            var last = await _redis.GetLastTagStateAsync(tag.TagId);
            if (last != null &&
                Math.Abs(last.Value - tag.Value) < 0.001 &&
                last.Quality == tag.Quality &&
                last.Timestamp == tagTime)
            {
                continue; // بدون تغییر معنی‌دار
            }

            // ذخیره وضعیت جدید در Redis
            await _redis.SetLastTagStateAsync(tag.TagId, tag.Value, tag.Quality, tag.Timestamp, tag.Source, tag.Application);
        }

        // ذخیره کل بچ در MinIO از طریق IWriter
        await _writer.WriteBatchAsync(message.Tags);

        return Ok(new
        {
            status = "accepted",
            count = message.Tags.Count,
            message =  "Batch processed"
        });
    }
}