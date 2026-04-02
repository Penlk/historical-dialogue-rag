namespace HistoricalDialogueRag.Core.Domain.Figures;

public sealed record HistoricalFigure(
    string FigureId,
    string DisplayName,
    string FullName,
    string LifeYears,
    string DefaultLanguage,
    FigurePersona Persona);
