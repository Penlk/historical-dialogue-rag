namespace HistoricalDialogueRag.Core.Application.Contracts.Indexing;

public sealed record IndexCorpusResult(
    string FigureId,
    int DocumentCount,
    int ChunkCount);
