using HistoricalDialogueRag.Core.Domain.Corpus;

namespace HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;

public sealed record ContextRetrievalRequest(
    string FigureId,
    string Question,
    int TopK,
    double MinScore);

public sealed record RetrievedContextChunk(
    DocumentChunk Chunk,
    double Score);

public interface IContextRetriever
{
    Task<IReadOnlyList<RetrievedContextChunk>> RetrieveAsync(
        ContextRetrievalRequest request,
        CancellationToken cancellationToken);
}