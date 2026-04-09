namespace HistoricalDialogueRag.Core.Application.Abstractions.Indexing;

public sealed record IndexRegistryEntry(
    string FigureId,
    DateTimeOffset IndexedAtUtc,
    int DocumentCount,
    int ChunkCount);

public interface IIndexRegistry
{
    Task SaveAsync(
        IndexRegistryEntry entry,
        CancellationToken cancellationToken);

    Task<IndexRegistryEntry?> GetAsync(
        string figureId,
        CancellationToken cancellationToken);
}
