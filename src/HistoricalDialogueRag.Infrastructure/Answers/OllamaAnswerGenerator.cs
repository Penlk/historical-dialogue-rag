using System.Net.Http.Json;
using System.Text.Json;
using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;
using HistoricalDialogueRag.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HistoricalDialogueRag.Infrastructure.Answers;

public sealed class OllamaAnswerGenerator : IAnswerGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly AnswersOptions _options;

    public OllamaAnswerGenerator(
        HttpClient httpClient,
        IOptions<AnswersOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.Ollama.BaseUrl.TrimEnd('/'));
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    public async Task<GeneratedAnswer> GenerateAsync(
        AnswerGenerationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ContextChunks.Count == 0)
            return new GeneratedAnswer(request.FallbackMessage);

        if (string.IsNullOrWhiteSpace(request.Prompt))
            return new GeneratedAnswer(request.FallbackMessage);

        var ollamaRequest = new OllamaChatRequest(
            Model: _options.Ollama.Model,
            Messages:
            [
                new OllamaChatMessage(
                    Role: "user",
                    Content: request.Prompt)
            ],
            Stream: false,
            Options: new OllamaGenerationOptions(
                Temperature: _options.Ollama.Temperature,
                NumPredict: _options.Ollama.NumPredict));

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/chat",
            ollamaRequest,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "generate answer", cancellationToken);

        var body = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
            JsonOptions,
            cancellationToken);

        var answer = body?.Message?.Content?.Trim();

        return string.IsNullOrWhiteSpace(answer)
            ? new GeneratedAnswer(request.FallbackMessage)
            : new GeneratedAnswer(answer);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new InvalidOperationException(
            $"Ollama operation failed: {operation}. " +
            $"Status: {(int)response.StatusCode} {response.ReasonPhrase}. " +
            $"Body: {body}");
    }

    private sealed record OllamaChatRequest(
        string Model,
        IReadOnlyList<OllamaChatMessage> Messages,
        bool Stream,
        OllamaGenerationOptions Options);

    private sealed record OllamaChatMessage(
        string Role,
        string Content);

    private sealed record OllamaGenerationOptions(
        double Temperature,
        int NumPredict);

    private sealed record OllamaChatResponse(
        OllamaChatMessageResponse? Message,
        bool Done);

    private sealed record OllamaChatMessageResponse(
        string Role,
        string Content);
}