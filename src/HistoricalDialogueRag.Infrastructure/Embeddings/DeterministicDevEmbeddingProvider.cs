using System.Security.Cryptography;
using System.Text;
using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HistoricalDialogueRag.Infrastructure.Embeddings;

public sealed class DeterministicDevEmbeddingProvider : IEmbeddingProvider
{
    private readonly int _vectorSize;

    public DeterministicDevEmbeddingProvider(IOptions<IndexingOptions> options)
    {
        _vectorSize = options.Value.DevVectorSize;
    }

    public Task<IReadOnlyList<float[]>> EmbedDocumentsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<float[]> vectors = texts
            .Select(CreateVector)
            .ToList();

        return Task.FromResult(vectors);
    }

    public Task<float[]> EmbedQueryAsync(
        string query,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(CreateVector(query));
    }

    private float[] CreateVector(string text)
    {
        var normalizedText = text.Trim().ToLowerInvariant();
        var bytes = Encoding.UTF8.GetBytes(normalizedText);
        var hash = SHA256.HashData(bytes);

        var vector = new float[_vectorSize];

        for (var i = 0; i < vector.Length; i++)
        {
            var value = hash[i % hash.Length];
            vector[i] = (value - 127.5f) / 127.5f;
        }

        Normalize(vector);

        return vector;
    }

    private static void Normalize(float[] vector)
    {
        var sum = 0.0;

        foreach (var value in vector)
            sum += value * value;

        var length = Math.Sqrt(sum);

        if (length <= 0)
            return;

        for (var i = 0; i < vector.Length; i++)
            vector[i] = (float)(vector[i] / length);
    }
}
