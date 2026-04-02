namespace HistoricalDialogueRag.Infrastructure.Configuration;

public sealed class CorpusOptions
{
    public const string SectionName = "Corpus";

    public string RootPath { get; init; } = "data/corpus";
}
