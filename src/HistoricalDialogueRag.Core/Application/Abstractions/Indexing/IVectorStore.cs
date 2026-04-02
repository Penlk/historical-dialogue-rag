using HistoricalDialogueRag.Core.Domain.Corpus;

namespace HistoricalDialogueRag.Core.Application.Abstractions.Indexing;

public sealed record VectorPoint(
    string Id,
    float[] Vector,
    DocumentChunk Chunk);

public sealed record VectorSearchRequest(
    string FigureId,
    float[] QueryVector,
    int TopK,
    double MinScore);

public sealed record VectorSearchResult(
    DocumentChunk Chunk,
    double Score);

public interface IVectorStore
{
    Task UpsertAsync(
        IReadOnlyList<VectorPoint> points,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        VectorSearchRequest request,
        CancellationToken cancellationToken);

    Task DeleteByFigureAsync(
        string figureId,
        CancellationToken cancellationToken);
}
