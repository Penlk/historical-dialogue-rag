namespace HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;

public sealed record AnswerGenerationRequest(
    string Prompt,
    IReadOnlyList<RetrievedContextChunk> ContextChunks,
    string FallbackMessage);

public sealed record GeneratedAnswer(string Text);

public interface IAnswerGenerator
{
    Task<GeneratedAnswer> GenerateAsync(
        AnswerGenerationRequest request,
        CancellationToken cancellationToken);
}