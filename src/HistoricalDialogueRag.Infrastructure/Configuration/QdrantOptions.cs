namespace HistoricalDialogueRag.Infrastructure.Configuration;

public sealed class QdrantOptions
{
    public const string SectionName = "Qdrant";

    public string BaseUrl { get; init; } = "http://localhost:6333";
    public string CollectionName { get; init; } = "historical-dialogue-chunks";
    public int VectorSize { get; init; } = 768;
    public string Distance { get; init; } = "Cosine";
}
