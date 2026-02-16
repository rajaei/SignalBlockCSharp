namespace SignalBlocks.Writer.Core.Redis;

public class RedisOptions
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public int Database { get; set; } = 0;
    public string Password { get; set; } = string.Empty;
}