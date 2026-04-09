namespace HistoricalDialogueRag.Infrastructure.Configuration;

public sealed class IndexingOptions
{
    public const string SectionName = "Indexing";

    public string RegistryPath { get; init; } = "data/registry/index-registry.json";
    public string LocalVectorStorePath { get; init; } = "data/registry/local-vector-store.json";
    public int DevVectorSize { get; init; } = 128;
}
