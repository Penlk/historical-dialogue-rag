namespace HistoricalDialogueRag.Core.Domain.Corpus;

public sealed record DocumentChunk(
    string ChunkId,
    string FigureId,
    string DocumentId,
    int ChunkIndex,
    string Text,
    DocumentMetadata Metadata);
