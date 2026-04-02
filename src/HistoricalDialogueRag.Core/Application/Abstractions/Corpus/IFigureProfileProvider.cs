using HistoricalDialogueRag.Core.Domain.Figures;

namespace HistoricalDialogueRag.Core.Application.Abstractions.Corpus;

public interface IFigureProfileProvider
{
    Task<HistoricalFigure> GetFigureAsync(
        string figureId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<HistoricalFigure>> GetFiguresAsync(
        CancellationToken cancellationToken);
}
