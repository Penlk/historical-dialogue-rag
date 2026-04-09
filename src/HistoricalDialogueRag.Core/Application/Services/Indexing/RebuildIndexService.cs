using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Core.Application.Contracts.Indexing;

namespace HistoricalDialogueRag.Core.Application.Services.Indexing;

public sealed class RebuildIndexService : IRebuildIndexService
{
    private readonly IVectorStore _vectorStore;
    private readonly ICorpusIndexingService _indexingService;

    public RebuildIndexService(
        IVectorStore vectorStore,
        ICorpusIndexingService indexingService)
    {
        _vectorStore = vectorStore;
        _indexingService = indexingService;
    }

    public async Task<IndexCorpusResult> RebuildAsync(
        string figureId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(figureId))
            throw new ArgumentException("FigureId is required.", nameof(figureId));

        await _vectorStore.DeleteByFigureAsync(figureId, cancellationToken);

        return await _indexingService.IndexAsync(
            new IndexCorpusRequest(figureId),
            cancellationToken);
    }
}
