using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;
using HistoricalDialogueRag.Core.Application.Contracts.Dialogue;

namespace HistoricalDialogueRag.Core.Application.Services.Dialogue;

public sealed class DialogueService : IDialogueService
{
    private readonly IFigureProfileProvider _figureProfileProvider;
    private readonly IContextRetriever _contextRetriever;
    private readonly IPromptComposer _promptComposer;
    private readonly IAnswerGenerator _answerGenerator;

    public DialogueService(
        IFigureProfileProvider figureProfileProvider,
        IContextRetriever contextRetriever,
        IPromptComposer promptComposer,
        IAnswerGenerator answerGenerator)
    {
        _figureProfileProvider = figureProfileProvider;
        _contextRetriever = contextRetriever;
        _promptComposer = promptComposer;
        _answerGenerator = answerGenerator;
    }

    public async Task<AskResponse> AskAsync(
        AskRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var figure = await _figureProfileProvider.GetFigureAsync(
            request.FigureId,
            cancellationToken);

        var contextChunks = await _contextRetriever.RetrieveAsync(
            new ContextRetrievalRequest(
                request.FigureId,
                request.Question,
                request.TopK,
                request.MinScore),
            cancellationToken);

        if (contextChunks.Count == 0)
        {
            return new AskResponse(
                Answer: null,
                UsedContext: false,
                Sources: [],
                Message: figure.Persona.FallbackMessage);
        }

        var contextBlocks = contextChunks
            .Select((item, index) => FormatContextBlock(index + 1, item))
            .ToList();

        var prompt = _promptComposer.Compose(
            new HistoricalPromptInput(
                FigureName: figure.DisplayName,
                PersonaStyle: figure.Persona.AnswerStyle,
                Question: request.Question,
                ContextBlocks: contextBlocks));

        var generated = await _answerGenerator.GenerateAsync(
            new AnswerGenerationRequest(
                prompt,
                contextChunks,
                figure.Persona.FallbackMessage),
            cancellationToken);

        var answer = string.IsNullOrWhiteSpace(generated.Text)
            ? figure.Persona.FallbackMessage
            : generated.Text.Trim();

        var sources = contextChunks
            .Select(ToSourceDto)
            .ToList();

        return new AskResponse(
            answer,
            UsedContext: true,
            sources,
            Message: null);
    }

    private static void ValidateRequest(AskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FigureId))
            throw new ArgumentException("FigureId is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Question))
            throw new ArgumentException("Question is required.", nameof(request));

        if (request.TopK <= 0 || request.TopK > 20)
            throw new ArgumentOutOfRangeException(nameof(request), "TopK must be in range 1..20.");

        if (request.MinScore < 0 || request.MinScore > 1)
            throw new ArgumentOutOfRangeException(nameof(request), "MinScore must be in range 0..1.");
    }

    private static string FormatContextBlock(int sourceNumber, RetrievedContextChunk item)
    {
        var metadata = item.Chunk.Metadata;

        return $"""
                [Source {sourceNumber}]
                Title: {metadata.Title}
                Author: {metadata.Author}
                Document type: {metadata.DocumentType}
                Year: {metadata.Year?.ToString() ?? "unknown"}
                Date: {metadata.Date?.ToString("yyyy-MM-dd") ?? "unknown"}
                Source name: {metadata.SourceName}
                Source URL: {metadata.SourceUrl}
                Chunk index: {item.Chunk.ChunkIndex}
                Relevance score: {item.Score:F3}

                Text:
                {item.Chunk.Text}
                """;
    }

    private static SourceDto ToSourceDto(RetrievedContextChunk item)
    {
        var metadata = item.Chunk.Metadata;

        return new SourceDto(
            DocumentTitle: metadata.Title,
            Author: metadata.Author,
            DocumentType: metadata.DocumentType.ToString(),
            Year: metadata.Year,
            SourceName: metadata.SourceName,
            SourceUrl: metadata.SourceUrl,
            ChunkIndex: item.Chunk.ChunkIndex,
            Score: Math.Round(item.Score, 4));
    }
}