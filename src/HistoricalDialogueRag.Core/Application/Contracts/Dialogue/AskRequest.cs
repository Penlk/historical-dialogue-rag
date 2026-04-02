namespace HistoricalDialogueRag.Core.Application.Contracts.Dialogue;

public sealed record AskRequest(
    string FigureId,
    string Question,
    int TopK = 6,
    double MinScore = 0.3);
