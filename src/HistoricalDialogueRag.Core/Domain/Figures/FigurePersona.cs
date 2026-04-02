namespace HistoricalDialogueRag.Core.Domain.Figures;

public sealed record FigurePersona(
    string ShortDescription,
    string AnswerStyle,
    string FallbackMessage);
