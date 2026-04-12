using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;
using HistoricalDialogueRag.Core.Application.Contracts.Dialogue;

namespace HistoricalDialogueRag.Web.Endpoints;

public static class DialogueEndpoints
{
    public static IEndpointRouteBuilder MapDialogueEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/dialogue/ask", async (
            AskRequest request,
            IDialogueService dialogueService,
            CancellationToken cancellationToken) =>
        {
            var response = await dialogueService.AskAsync(request, cancellationToken);
            return Results.Ok(response);
        });

        return endpoints;
    }
}