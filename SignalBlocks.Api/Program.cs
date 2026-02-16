using Minio;
using SignalBlocks.Writer.Core.Interfaces;
using SignalBlocks.Writer.Core.Writers;

var builder = WebApplication.CreateBuilder(args);

// === Dependency Injection ===
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MinioClient()
        .WithEndpoint(config["Minio:Endpoint"] ?? "localhost:9000")
        .WithCredentials(
            config["Minio:AccessKey"] ?? "minioadmin",
            config["Minio:SecretKey"] ?? "minioadmin123")
        .WithSSL(false)
        .Build();
});

builder.Services.AddSingleton<IWriter>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var client = sp.GetRequiredService<IMinioClient>();

    var bucket = config["Minio:Bucket"] ?? "signalblocks";

    return new MinioWriter(client, bucket);
});
// Controllers
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();