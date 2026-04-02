namespace HistoricalDialogueRag.Core.Domain.Corpus;

public sealed record DocumentMetadata(
    string DocumentId,
    string FigureId,
    string Title,
    string Author,
    HistoricalDocumentType DocumentType,
    DateOnly? Date,
    int? Year,
    string TextLanguage,
    string? OriginalLanguage,
    string SourceName,
    string SourceUrl,
    string License,
    DocumentQuality Quality);
