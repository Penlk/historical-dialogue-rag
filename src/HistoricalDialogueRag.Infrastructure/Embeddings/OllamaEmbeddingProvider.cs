using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HistoricalDialogueRag.Infrastructure.Embeddings;

public sealed class OllamaEmbeddingProvider : IEmbeddingProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly EmbeddingsOptions _options;

    public OllamaEmbeddingProvider(
        HttpClient httpClient,
        IOptions<EmbeddingsOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.Ollama.BaseUrl.TrimEnd('/'));
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    public async Task<IReadOnlyList<float[]>> EmbedDocumentsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken)
    {
        if (texts.Count == 0)
            return [];

        var vectors = new List<float[]>(texts.Count);
        var batchSize = Math.Max(1, _options.BatchSize);

        foreach (var batch in texts.Chunk(batchSize))
        {
            var batchVectors = await EmbedBatchAsync(batch, cancellationToken);
            vectors.AddRange(batchVectors);
        }

        if (vectors.Count != texts.Count)
        {
            throw new InvalidOperationException(
                $"Ollama returned unexpected embedding count. Expected {texts.Count}, got {vectors.Count}.");
        }

        return vectors;
    }

    public async Task<float[]> EmbedQueryAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var vectors = await EmbedBatchAsync([query], cancellationToken);

        return vectors.Count == 1
            ? vectors[0]
            : throw new InvalidOperationException("Ollama returned unexpected query embedding count.");
    }

    private async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/embed",
            new OllamaEmbedRequest(
                Model: _options.Ollama.Model,
                Input: texts,
                Truncate: _options.Ollama.Truncate),
            JsonOptions,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return await EmbedBatchLegacyAsync(texts, cancellationToken);

        await EnsureSuccessAsync(response, "generate embeddings with /api/embed", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(
            JsonOptions,
            cancellationToken);

        if (result?.Embeddings is null || result.Embeddings.Count == 0)
            throw new InvalidOperationException("Ollama returned an empty embeddings response.");

        return result.Embeddings;
    }

    private async Task<IReadOnlyList<float[]>> EmbedBatchLegacyAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken)
    {
        var vectors = new List<float[]>(texts.Count);

        foreach (var text in texts)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "/api/embeddings",
                new OllamaLegacyEmbeddingRequest(
                    Model: _options.Ollama.Model,
                    Prompt: text),
                JsonOptions,
                cancellationToken);

            await EnsureSuccessAsync(response, "generate embeddings with legacy /api/embeddings", cancellationToken);

            var result = await response.Content.ReadFromJsonAsync<OllamaLegacyEmbeddingResponse>(
                JsonOptions,
                cancellationToken);

            if (result?.Embedding is null || result.Embedding.Length == 0)
                throw new InvalidOperationException("Ollama returned an empty legacy embedding response.");

            vectors.Add(result.Embedding);
        }

        return vectors;
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

    private sealed record OllamaEmbedRequest(
        string Model,
        IReadOnlyList<string> Input,
        bool Truncate);

    private sealed record OllamaEmbedResponse(
        IReadOnlyList<float[]> Embeddings);

    private sealed record OllamaLegacyEmbeddingRequest(
        string Model,
        string Prompt);

    private sealed record OllamaLegacyEmbeddingResponse(
        float[] Embedding);
}