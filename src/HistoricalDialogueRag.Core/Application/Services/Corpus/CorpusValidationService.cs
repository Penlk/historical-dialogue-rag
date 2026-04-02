using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Core.Application.Contracts.Corpus;
using HistoricalDialogueRag.Core.Domain.Corpus;

namespace HistoricalDialogueRag.Core.Application.Services.Corpus;

public sealed class CorpusValidationService : ICorpusValidator
{
    private readonly ICorpusDocumentProvider _documentProvider;

    public CorpusValidationService(ICorpusDocumentProvider documentProvider)
    {
        _documentProvider = documentProvider;
    }

    public async Task<CorpusValidationReport> ValidateAsync(
        string figureId,
        CancellationToken cancellationToken)
    {
        var documents = await _documentProvider.GetDocumentsAsync(
            new CorpusDocumentQuery(figureId, IncludeDraft: true, IncludeNeedsReview: true),
            cancellationToken);

        var errors = new List<string>();
        var warnings = new List<string>();

        var duplicateIds = documents
            .GroupBy(document => document.Metadata.DocumentId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        foreach (var duplicateId in duplicateIds)
            errors.Add($"Duplicate documentId: {duplicateId}");

        foreach (var document in documents)
        {
            var metadata = document.Metadata;

            if (string.IsNullOrWhiteSpace(metadata.DocumentId))
                errors.Add("Document has empty documentId.");

            if (metadata.FigureId != figureId)
                errors.Add($"{metadata.DocumentId}: figureId mismatch.");

            if (string.IsNullOrWhiteSpace(metadata.Title))
                errors.Add($"{metadata.DocumentId}: title is required.");

            if (string.IsNullOrWhiteSpace(metadata.Author))
                errors.Add($"{metadata.DocumentId}: author is required.");

            if (metadata.Year is null && metadata.Date is null)
                errors.Add($"{metadata.DocumentId}: year or date is required.");

            if (string.IsNullOrWhiteSpace(metadata.SourceName))
                errors.Add($"{metadata.DocumentId}: sourceName is required.");

            if (string.IsNullOrWhiteSpace(metadata.SourceUrl))
                errors.Add($"{metadata.DocumentId}: sourceUrl is required.");

            if (string.IsNullOrWhiteSpace(metadata.License))
                errors.Add($"{metadata.DocumentId}: license is required.");

            if (metadata.Quality != DocumentQuality.Checked)
                warnings.Add($"{metadata.DocumentId}: quality is not checked.");

            if (string.IsNullOrWhiteSpace(document.Body))
                errors.Add($"{metadata.DocumentId}: body is empty.");
        }

        return new CorpusValidationReport(
            figureId,
            documents.Count,
            errors,
            warnings);
    }
}
