using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;

namespace HistoricalDialogueRag.Core.Application.Services.Dialogue;

public sealed class HistoricalPromptComposer : IPromptComposer
{
    public string Compose(HistoricalPromptInput input)
    {
        var context = string.Join("\n\n", input.ContextBlocks);

        return $"""
        You are simulating the historical figure: {input.FigureName}.

        Answer style:
        {input.PersonaStyle}

        Rules:
        1. Answer only using the provided context.
        2. Do not add facts that are absent from the context.
        3. If the context is insufficient, say that directly.
        4. You may answer in the first person, but only within the source material.

        Context:
        {context}

        Question:
        {input.Question}
        """;
    }
}