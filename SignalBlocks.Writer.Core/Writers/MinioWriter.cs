using Minio;
using Minio.DataModel.Args;
using SignalBlocks.Writer.Core.Interfaces;
using SignalBlocks.Writer.Core.Models;

namespace SignalBlocks.Writer.Core.Writers;

public class MinioWriter : IWriter
{
    private readonly IMinioClient _client;
    private readonly string _bucketName;

    public MinioWriter(IMinioClient client, string bucketName)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
    }

    public async Task WriteBatchAsync(IEnumerable<TagValue> items, CancellationToken ct = default)
    {
        // برای تست اولیه ساده، هر آیتم یک فایل جدا می‌شود
        foreach (var item in items)
        {
            var objectName = $"{item.TagId}/{item.Timestamp:yyyy-MM-dd-HH-mm-ss-fff}.json";

            var json = System.Text.Json.JsonSerializer.Serialize(item);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            var args = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(new MemoryStream(bytes))
                .WithObjectSize(bytes.Length)
                .WithContentType("application/json");

            await _client.PutObjectAsync(args, ct);
        }
    }
}