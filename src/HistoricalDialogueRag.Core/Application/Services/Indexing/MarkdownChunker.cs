using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Core.Domain.Corpus;

namespace HistoricalDialogueRag.Core.Application.Services.Indexing;

public sealed class MarkdownChunker : IChunker
{
    private const int MaxChunkChars = 1600;

    public IReadOnlyList<DocumentChunk> Chunk(SourceDocument document)
    {
        var body = NormalizeBody(document.Body);

        if (string.IsNullOrWhiteSpace(body))
            return [];

        var paragraphs = body
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
            .ToList();

        if (paragraphs.Count == 0)
            return [];

        var chunks = new List<DocumentChunk>();
        var current = new List<string>();
        var currentLength = 0;
        var chunkIndex = 0;

        foreach (var paragraph in paragraphs)
        {
            if (current.Count > 0 && currentLength + paragraph.Length > MaxChunkChars)
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

    private static string NormalizeBody(string body)
    {
        return body
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Trim();
    }

    private static void AddChunk(
        SourceDocument document,
        List<DocumentChunk> chunks,
        IReadOnlyList<string> paragraphs,
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