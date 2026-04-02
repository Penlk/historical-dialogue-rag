using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Core.Domain.Corpus;

namespace HistoricalDialogueRag.Core.Application.Services.Indexing;

public sealed class MarkdownChunker : IChunker
{
    private const int MaxChunkChars = 1600;
    private const int MinChunkChars = 300;

    public IReadOnlyList<DocumentChunk> Chunk(SourceDocument document)
    {
        var paragraphs = document.Body
            .Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var chunks = new List<DocumentChunk>();
        var current = new List<string>();
        var currentLength = 0;
        var chunkIndex = 0;

        foreach (var paragraph in paragraphs)
        {
            if (currentLength + paragraph.Length > MaxChunkChars && currentLength >= MinChunkChars)
            {
                AddChunk(document, chunks, current, chunkIndex++);
                current.Clear();
                currentLength = 0;
            }

            current.Add(paragraph);
            currentLength += paragraph.Length;
        }

        if (current.Count > 0)
            AddChunk(document, chunks, current, chunkIndex);

        return chunks;
    }

    private static void AddChunk(
        SourceDocument document,
        List<DocumentChunk> chunks,
        List<string> paragraphs,
        int chunkIndex)
    {
        var text = string.Join("\n\n", paragraphs).Trim();

        if (string.IsNullOrWhiteSpace(text))
            return;

        var chunkId = $"{document.Metadata.DocumentId}-chunk-{chunkIndex:D4}";

        chunks.Add(new DocumentChunk(
            chunkId,
            document.Metadata.FigureId,
            document.Metadata.DocumentId,
            chunkIndex,
            text,
            document.Metadata));
    }
}
