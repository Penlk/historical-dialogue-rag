using HistoricalDialogueRag.Core.Domain.Corpus;

namespace HistoricalDialogueRag.Core.Application.Abstractions.Indexing;

public interface IChunker
{
    IReadOnlyList<DocumentChunk> Chunk(SourceDocument document);
}
