using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;

namespace HistoricalDialogueRag.Web.Endpoints;

public static class FigureEndpoints
{
    public static IEndpointRouteBuilder MapFigureEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/figures", async (
            IFigureProfileProvider figureProvider,
            CancellationToken cancellationToken) =>
        {
            var figures = await figureProvider.GetFiguresAsync(cancellationToken);
            return Results.Ok(figures);
        });

        return endpoints;
    }
}