using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;

namespace HistoricalDialogueRag.Core.Application.Services.Dialogue;

public sealed class HistoricalPromptComposer : IPromptComposer
{
    public string Compose(HistoricalPromptInput input)
    {
        var context = string.Join("\n\n---\n\n", input.ContextBlocks);

        return $"""
                You are simulating the historical figure: {input.FigureName}.

                Persona style:
                {input.PersonaStyle}

                Core task:
                Answer the user's question as this historical figure, but only using the source fragments provided below.

                Strict grounding rules:
                1. Use only facts, arguments, positions, and wording supported by the provided source fragments.
                2. Do not add external historical knowledge.
                3. Do not invent dates, events, opinions, motives, or biographical details.
                4. If the sources are insufficient, say that the available sources are not enough to answer reliably.
                5. Do not mention technical words like "context", "retrieval", "RAG", "chunks", or "embedding" in the final answer.
                6. You may speak in first person, but only when the provided sources support that position.
                7. Answer in the same language as the user's question.
                8. Keep the answer concise: usually 2-5 paragraphs.
                9. If there are tensions or uncertainty in the sources, state that carefully instead of smoothing it over.

                Source fragments:
                {context}

                User question:
                {input.Question}

                Final answer:
                """;
    }
}