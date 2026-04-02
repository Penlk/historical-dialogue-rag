using HistoricalDialogueRag.Core.Application.Contracts.Corpus;

namespace HistoricalDialogueRag.Core.Application.Abstractions.Corpus;

public interface ICorpusValidator
{
    Task<CorpusValidationReport> ValidateAsync(
        string figureId,
        CancellationToken cancellationToken);
}
