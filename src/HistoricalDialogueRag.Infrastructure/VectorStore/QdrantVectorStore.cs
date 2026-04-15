using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Core.Domain.Corpus;
using HistoricalDialogueRag.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HistoricalDialogueRag.Infrastructure.VectorStore;

public sealed class QdrantVectorStore : IVectorStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly QdrantOptions _options;

    public QdrantVectorStore(
        HttpClient httpClient,
        IOptions<QdrantOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/'));
    }

    public async Task UpsertAsync(
        IReadOnlyList<VectorPoint> points,
        CancellationToken cancellationToken)
    {
        if (points.Count == 0)
            return;

        await EnsureCollectionAsync(cancellationToken);

        var request = new QdrantUpsertRequest(
            points.Select(ToQdrantPoint).ToList());

        using var response = await _httpClient.PutAsJsonAsync(
            $"/collections/{_options.CollectionName}/points?wait=true",
            request,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "upsert points", cancellationToken);
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        VectorSearchRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TopK <= 0)
            return [];

        if (!await CollectionExistsAsync(cancellationToken))
            return [];

        var qdrantRequest = new QdrantSearchRequest(
            Vector: request.QueryVector,
            Limit: request.TopK,
            ScoreThreshold: request.MinScore,
            WithPayload: true,
            Filter: FigureFilter(request.FigureId));

        using var response = await _httpClient.PostAsJsonAsync(
            $"/collections/{_options.CollectionName}/points/search",
            qdrantRequest,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "search points", cancellationToken);

        var searchResponse = await response.Content.ReadFromJsonAsync<QdrantSearchResponse>(
            JsonOptions,
            cancellationToken);

        return searchResponse?.Result
            .Select(ToVectorSearchResult)
            .Where(result => result is not null)
            .Select(result => result!)
            .ToList() ?? [];
    }

    public async Task DeleteByFigureAsync(
        string figureId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(figureId))
            return;

        if (!await CollectionExistsAsync(cancellationToken))
            return;

        var request = new QdrantDeleteRequest(
            Filter: FigureFilter(figureId));

        using var response = await _httpClient.PostAsJsonAsync(
            $"/collections/{_options.CollectionName}/points/delete?wait=true",
            request,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "delete points by figure", cancellationToken);
    }

    private async Task EnsureCollectionAsync(CancellationToken cancellationToken)
    {
        if (await CollectionExistsAsync(cancellationToken))
            return;

        var request = new QdrantCreateCollectionRequest(
            Vectors: new QdrantVectorsConfig(
                Size: _options.VectorSize,
                Distance: _options.Distance));

        using var response = await _httpClient.PutAsJsonAsync(
            $"/collections/{_options.CollectionName}",
            request,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, "create collection", cancellationToken);
    }

    private async Task<bool> CollectionExistsAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            $"/collections/{_options.CollectionName}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        await EnsureSuccessAsync(response, "check collection", cancellationToken);
        return true;
    }

    private QdrantPoint ToQdrantPoint(VectorPoint point)
    {
        var metadata = point.Chunk.Metadata;

        var payload = new Dictionary<string, object?>
        {
            ["chunkId"] = point.Chunk.ChunkId,
            ["figureId"] = point.Chunk.FigureId,
            ["documentId"] = point.Chunk.DocumentId,
            ["chunkIndex"] = point.Chunk.ChunkIndex,
            ["text"] = point.Chunk.Text,

            ["documentTitle"] = metadata.Title,
            ["author"] = metadata.Author,
            ["documentType"] = metadata.DocumentType.ToString(),
            ["date"] = metadata.Date?.ToString("yyyy-MM-dd"),
            ["year"] = metadata.Year,
            ["textLanguage"] = metadata.TextLanguage,
            ["originalLanguage"] = metadata.OriginalLanguage,
            ["sourceName"] = metadata.SourceName,
            ["sourceUrl"] = metadata.SourceUrl,
            ["license"] = metadata.License,
            ["quality"] = metadata.Quality.ToString()
        };

        return new QdrantPoint(
            Id: ToDeterministicGuid(point.Id),
            Vector: point.Vector,
            Payload: payload);
    }

    private static VectorSearchResult? ToVectorSearchResult(QdrantScoredPoint point)
    {
        if (point.Payload is null)
            return null;

        var metadata = new DocumentMetadata(
            DocumentId: GetRequiredString(point.Payload, "documentId"),
            FigureId: GetRequiredString(point.Payload, "figureId"),
            Title: GetRequiredString(point.Payload, "documentTitle"),
            Author: GetRequiredString(point.Payload, "author"),
            DocumentType: ParseEnum<HistoricalDocumentType>(
                GetRequiredString(point.Payload, "documentType")),
            Date: ParseDateOnly(GetOptionalString(point.Payload, "date")),
            Year: GetOptionalInt(point.Payload, "year"),
            TextLanguage: GetRequiredString(point.Payload, "textLanguage"),
            OriginalLanguage: GetOptionalString(point.Payload, "originalLanguage"),
            SourceName: GetRequiredString(point.Payload, "sourceName"),
            SourceUrl: GetRequiredString(point.Payload, "sourceUrl"),
            License: GetRequiredString(point.Payload, "license"),
            Quality: ParseEnum<DocumentQuality>(
                GetRequiredString(point.Payload, "quality")));

        var chunk = new DocumentChunk(
            ChunkId: GetRequiredString(point.Payload, "chunkId"),
            FigureId: GetRequiredString(point.Payload, "figureId"),
            DocumentId: GetRequiredString(point.Payload, "documentId"),
            ChunkIndex: GetRequiredInt(point.Payload, "chunkIndex"),
            Text: GetRequiredString(point.Payload, "text"),
            Metadata: metadata);

        return new VectorSearchResult(chunk, point.Score);
    }

    private static QdrantFilter FigureFilter(string figureId)
    {
        return new QdrantFilter(
            Must:
            [
                new QdrantFilterCondition(
                    Key: "figureId",
                    Match: new QdrantMatchValue(figureId))
            ]);
    }

    private static Guid ToDeterministicGuid(string value)
    {
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));

        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);

        return new Guid(guidBytes);
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
            $"Qdrant operation failed: {operation}. " +
            $"Status: {(int)response.StatusCode} {response.ReasonPhrase}. " +
            $"Body: {body}");
    }

    private static string GetRequiredString(
        IReadOnlyDictionary<string, JsonElement> payload,
        string key)
    {
        var value = GetOptionalString(payload, key);

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Qdrant payload field is missing: {key}");

        return value;
    }

    private static string? GetOptionalString(
        IReadOnlyDictionary<string, JsonElement> payload,
        string key)
    {
        if (!payload.TryGetValue(key, out var value))
            return null;

        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        return value.GetString();
    }

    private static int GetRequiredInt(
        IReadOnlyDictionary<string, JsonElement> payload,
        string key)
    {
        var value = GetOptionalInt(payload, key);

        if (value is null)
            throw new InvalidOperationException($"Qdrant payload field is missing: {key}");

        return value.Value;
    }

    private static int? GetOptionalInt(
        IReadOnlyDictionary<string, JsonElement> payload,
        string key)
    {
        if (!payload.TryGetValue(key, out var value))
            return null;

        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        return value.GetInt32();
    }

    private static DateOnly? ParseDateOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateOnly.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private static TEnum ParseEnum<TEnum>(string value)
        where TEnum : struct
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new InvalidOperationException(
                $"Invalid enum value '{value}' for {typeof(TEnum).Name}.");
    }

    private sealed record QdrantCreateCollectionRequest(
        [property: JsonPropertyName("vectors")]
        QdrantVectorsConfig Vectors);

    private sealed record QdrantVectorsConfig(
        [property: JsonPropertyName("size")]
        int Size,

        [property: JsonPropertyName("distance")]
        string Distance);

    private sealed record QdrantUpsertRequest(
        [property: JsonPropertyName("points")]
        IReadOnlyList<QdrantPoint> Points);

    private sealed record QdrantPoint(
        [property: JsonPropertyName("id")]
        Guid Id,

        [property: JsonPropertyName("vector")]
        float[] Vector,

        [property: JsonPropertyName("payload")]
        Dictionary<string, object?> Payload);

    private sealed record QdrantSearchRequest(
        [property: JsonPropertyName("vector")]
        float[] Vector,

        [property: JsonPropertyName("limit")]
        int Limit,

        [property: JsonPropertyName("score_threshold")]
        double ScoreThreshold,

        [property: JsonPropertyName("with_payload")]
        bool WithPayload,

        [property: JsonPropertyName("filter")]
        QdrantFilter Filter);

    private sealed record QdrantSearchResponse(
        [property: JsonPropertyName("result")]
        IReadOnlyList<QdrantScoredPoint> Result);

    private sealed record QdrantScoredPoint(
        [property: JsonPropertyName("score")]
        double Score,

        [property: JsonPropertyName("payload")]
        Dictionary<string, JsonElement>? Payload);

    private sealed record QdrantDeleteRequest(
        [property: JsonPropertyName("filter")]
        QdrantFilter Filter);

    private sealed record QdrantFilter(
        [property: JsonPropertyName("must")]
        IReadOnlyList<QdrantFilterCondition> Must);

    private sealed record QdrantFilterCondition(
        [property: JsonPropertyName("key")]
        string Key,

        [property: JsonPropertyName("match")]
        QdrantMatchValue Match);

    private sealed record QdrantMatchValue(
        [property: JsonPropertyName("value")]
        string Value);
}