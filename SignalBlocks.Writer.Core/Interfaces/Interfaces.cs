using SignalBlocks.Writer.Core.Models;

namespace SignalBlocks.Writer.Core.Interfaces;

public interface IWriter
{
    Task WriteBatchAsync(IEnumerable<TagValue> items, CancellationToken ct = default);
}