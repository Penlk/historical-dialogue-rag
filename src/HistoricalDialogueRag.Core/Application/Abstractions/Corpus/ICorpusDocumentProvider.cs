using HistoricalDialogueRag.Core.Domain.Corpus;

namespace HistoricalDialogueRag.Core.Application.Abstractions.Corpus;

public sealed record CorpusDocumentQuery(
    string FigureId,
    bool IncludeDraft = false,
    bool IncludeNeedsReview = false);

public interface ICorpusDocumentProvider
{
    Task<IReadOnlyList<SourceDocument>> GetDocumentsAsync(
        CorpusDocumentQuery query,
        CancellationToken cancellationToken);
}
