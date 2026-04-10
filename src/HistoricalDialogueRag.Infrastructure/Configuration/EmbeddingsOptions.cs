namespace HistoricalDialogueRag.Infrastructure.Configuration;

public sealed class EmbeddingsOptions
{
    public const string SectionName = "Embeddings";

    public string Provider { get; init; } = "ollama";
    public int BatchSize { get; init; } = 16;
    public OllamaEmbeddingsOptions Ollama { get; init; } = new();
}

public sealed class OllamaEmbeddingsOptions
{
    public string BaseUrl { get; init; } = "http://localhost:11434";
    public string Model { get; init; } = "nomic-embed-text";
    public bool Truncate { get; init; } = true;
}