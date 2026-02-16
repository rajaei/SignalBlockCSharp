using Microsoft.AspNetCore.Mvc;
using SignalBlocks.Writer.Core.Interfaces;
using SignalBlocks.Writer.Core.Models;
using System.Text.Json;

namespace SignalBlocks.Api.Controllers;

[ApiController]
[Route("api/write")]
public class WriteController : ControllerBase
{
    private readonly IWriter _writer;

    public WriteController(IWriter writer)
    {
        _writer = writer;
    }

    [HttpPost("batch")]
    public async Task<IActionResult> WriteBatch([FromBody] KepwareMessage message)
    {
        if (message?.Tags == null || message.Tags.Count == 0)
            return BadRequest("No tags received");

        // اعمال اولویت source و application
        foreach (var tag in message.Tags)
        {
            tag.Source ??= message.GlobalSource ?? "unknown";
            tag.Application ??= message.GlobalApplication ?? "default";
        }

        // نوشتن به صورت asynchronous و کنترل‌شده
        await _writer.WriteBatchAsync(message.Tags);

        return Ok(new { status = "accepted", count = message.Tags.Count });
    }

    [HttpPost("TestBatch")]
    public async Task<IActionResult> WriteBatch()
    {
        // ۱. JSON خام رو از بدنه درخواست بخون (به صورت string)
        using var reader = new StreamReader(Request.Body);
        var rawJson = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return BadRequest(new { error = "Empty request body" });
        }

        // ۲. دستی parse کن (با JsonSerializerOptions برای انعطاف بیشتر)
        KepwareMessage? message;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, // برای case-insensitive بودن
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            message = JsonSerializer.Deserialize<KepwareMessage>(rawJson, options);
        }
        catch (JsonException ex)
        {
            return BadRequest(new
            {
                error = "Invalid JSON format",
                details = ex.Message,
                position = ex.Path
            });
        }

        if (message == null || message.Tags == null || message.Tags.Count == 0)
        {
            return BadRequest(new { error = "No tags found in message" });
        }

        // ۳. اعمال منطق پیش‌فرض source و application (اختیاری)
        foreach (var tag in message.Tags)
        {
            tag.Source ??= message.GlobalSource ?? "unknown";
            tag.Application ??= message.GlobalApplication ?? "default";
        }

        // ۴. نوشتن به MinIO (یا هر writer دیگری)
        await _writer.WriteBatchAsync(message.Tags);

        return Ok(new 
        {
            status = "accepted",
            count = message.Tags.Count,
            message =  "Batch processed"
        });
    }
}
