namespace HistoricalDialogueRag.Core.Application.Abstractions.Indexing;

public interface IEmbeddingProvider
{
    Task<IReadOnlyList<float[]>> EmbedDocumentsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken);

    Task<float[]> EmbedQueryAsync(
        string query,
        CancellationToken cancellationToken);
}
