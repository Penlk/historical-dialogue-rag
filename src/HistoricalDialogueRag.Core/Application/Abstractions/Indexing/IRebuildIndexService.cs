using HistoricalDialogueRag.Core.Application.Contracts.Indexing;

namespace HistoricalDialogueRag.Core.Application.Abstractions.Indexing;

public interface IRebuildIndexService
{
    Task<IndexCorpusResult> RebuildAsync(
        string figureId,
        CancellationToken cancellationToken);
}
