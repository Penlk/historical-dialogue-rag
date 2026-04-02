namespace HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;

public interface IPromptComposer
{
    string Compose(HistoricalPromptInput input);
}

public sealed record HistoricalPromptInput(
    string FigureName,
    string PersonaStyle,
    string Question,
    IReadOnlyList<string> ContextBlocks);
