namespace HistoricalDialogueRag.Core.Domain.Corpus;

public sealed record SourceDocument(
    DocumentMetadata Metadata,
    string Body);
