using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;
using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;

namespace HistoricalDialogueRag.Core.Application.Services.Dialogue;

public sealed class ContextRetriever : IContextRetriever
{
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;

    public ContextRetriever(
        IEmbeddingProvider embeddingProvider,
        IVectorStore vectorStore)
    {
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
    }

    public async Task<IReadOnlyList<RetrievedContextChunk>> RetrieveAsync(
        ContextRetrievalRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FigureId))
            throw new ArgumentException("FigureId is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Question))
            throw new ArgumentException("Question is required.", nameof(request));

        if (request.TopK <= 0)
            throw new ArgumentOutOfRangeException(nameof(request), "TopK must be greater than zero.");

        if (request.MinScore < 0 || request.MinScore > 1)
            throw new ArgumentOutOfRangeException(nameof(request), "MinScore must be in range 0..1.");

        var queryVector = await _embeddingProvider.EmbedQueryAsync(
            request.Question,
            cancellationToken);

        var searchResults = await _vectorStore.SearchAsync(
            new VectorSearchRequest(
                request.FigureId,
                queryVector,
                request.TopK,
                request.MinScore),
            cancellationToken);

        return searchResults
            .Select(result => new RetrievedContextChunk(result.Chunk, result.Score))
            .ToList();
    }
}