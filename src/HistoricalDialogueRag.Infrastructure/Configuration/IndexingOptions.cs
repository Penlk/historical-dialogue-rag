namespace HistoricalDialogueRag.Infrastructure.Configuration;

public sealed class IndexingOptions
{
    public const string SectionName = "Indexing";

    public string RegistryPath { get; init; } = "data/registry/index-registry.json";
}