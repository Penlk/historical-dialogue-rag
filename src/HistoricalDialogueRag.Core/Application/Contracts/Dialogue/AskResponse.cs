namespace HistoricalDialogueRag.Core.Application.Contracts.Dialogue;

public sealed record AskResponse(
    string? Answer,
    bool UsedContext,
    IReadOnlyList<SourceDto> Sources,
    string? Message);
