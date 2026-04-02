namespace HistoricalDialogueRag.Core.Application.Contracts.Corpus;

public sealed record CorpusValidationReport(
    string FigureId,
    int DocumentCount,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public bool IsValid => Errors.Count == 0;
}
