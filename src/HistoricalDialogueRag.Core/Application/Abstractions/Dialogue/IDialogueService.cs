using HistoricalDialogueRag.Core.Application.Contracts.Dialogue;

namespace HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;

public interface IDialogueService
{
    Task<AskResponse> AskAsync(
        AskRequest request,
        CancellationToken cancellationToken);
}
