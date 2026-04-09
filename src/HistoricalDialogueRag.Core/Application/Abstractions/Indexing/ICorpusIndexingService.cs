using HistoricalDialogueRag.Core.Application.Contracts.Indexing;

namespace HistoricalDialogueRag.Core.Application.Abstractions.Indexing;

public interface ICorpusIndexingService
{
    Task<IndexCorpusResult> IndexAsync(
        IndexCorpusRequest request,
        CancellationToken cancellationToken);
}
