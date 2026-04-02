namespace HistoricalDialogueRag.Core.Application.Contracts.Dialogue;

public sealed record SourceDto(
    string DocumentTitle,
    string Author,
    string DocumentType,
    int? Year,
    string SourceName,
    string SourceUrl,
    int ChunkIndex,
    double Score);
