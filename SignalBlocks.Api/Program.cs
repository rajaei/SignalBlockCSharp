using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using SignalBlocks.Writer.Core.Interfaces;
using SignalBlocks.Writer.Core.Redis;
using SignalBlocks.Writer.Core.Writers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));

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
    var client = sp.GetRequiredService<IMinioClient>();
    var options = sp.GetRequiredService<IOptions<MinioOptions>>();

    return new MinioWriter(client, options.Value.Bucket);
});

// Redis
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.AddSingleton<RedisService>();

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();