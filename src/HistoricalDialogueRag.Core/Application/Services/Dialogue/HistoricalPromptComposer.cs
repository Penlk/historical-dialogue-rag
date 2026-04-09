using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;

namespace HistoricalDialogueRag.Core.Application.Services.Dialogue;

public sealed class HistoricalPromptComposer : IPromptComposer
{
    public string Compose(HistoricalPromptInput input)
    {
        var context = string.Join("\n\n", input.ContextBlocks);

        return $"""
        You simulate the historical figure: {input.FigureName}.

        Answer style:
        {input.PersonaStyle}

        Rules:
        1. Answer only from the provided context.
        2. Do not add facts that are not present in the context.
        3. If the context is insufficient, say that the available sources are insufficient.
        4. You may answer in first person, but only within the boundaries of the sources.
        5. Keep the answer concise and grounded.

        Context:
        {context}

        Question:
        {input.Question}
        """;
    }
}