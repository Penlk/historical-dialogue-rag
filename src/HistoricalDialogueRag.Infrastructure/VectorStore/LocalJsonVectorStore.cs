using System.Text.Json;
using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Core.Domain.Corpus;
using HistoricalDialogueRag.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HistoricalDialogueRag.Infrastructure.VectorStore;

public sealed class LocalJsonVectorStore : IVectorStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _path;

    public LocalJsonVectorStore(IOptions<IndexingOptions> options)
    {
        _path = ProjectPathResolver.Resolve(options.Value.LocalVectorStorePath);
    }

    public async Task UpsertAsync(
        IReadOnlyList<VectorPoint> points,
        CancellationToken cancellationToken)
    {
        var existing = await LoadAsync(cancellationToken);
        var map = existing.ToDictionary(point => point.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var point in points)
        {
            map[point.Id] = new LocalVectorPoint(
                point.Id,
                point.Vector,
                point.Chunk);
        }

        await SaveAsync(map.Values.ToList(), cancellationToken);
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        VectorSearchRequest request,
        CancellationToken cancellationToken)
    {
        var points = await LoadAsync(cancellationToken);

        var results = points
            .Where(point => point.Chunk.FigureId.Equals(request.FigureId, StringComparison.OrdinalIgnoreCase))
            .Select(point => new VectorSearchResult(
                point.Chunk,
                Score: CosineSimilarity(request.QueryVector, point.Vector)))
            .Where(result => result.Score >= request.MinScore)
            .OrderByDescending(result => result.Score)
            .Take(request.TopK)
            .ToList();

        return results;
    }

    public async Task DeleteByFigureAsync(
        string figureId,
        CancellationToken cancellationToken)
    {
        var points = await LoadAsync(cancellationToken);

        var filtered = points
            .Where(point => !point.Chunk.FigureId.Equals(figureId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        await SaveAsync(filtered, cancellationToken);
    }

    private async Task<List<LocalVectorPoint>> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_path))
            return [];

        await using var stream = File.OpenRead(_path);

        var points = await JsonSerializer.DeserializeAsync<List<LocalVectorPoint>>(
            stream,
            JsonOptions,
            cancellationToken);

        return points ?? [];
    }

    private async Task SaveAsync(
        List<LocalVectorPoint> points,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await using var stream = File.Create(_path);

        await JsonSerializer.SerializeAsync(
            stream,
            points,
            JsonOptions,
            cancellationToken);
    }

    private static double CosineSimilarity(float[] left, float[] right)
    {
        var length = Math.Min(left.Length, right.Length);

        if (length == 0)
            return 0;

        var dot = 0.0;
        var leftNorm = 0.0;
        var rightNorm = 0.0;

        for (var i = 0; i < length; i++)
        {
            dot += left[i] * right[i];
            leftNorm += left[i] * left[i];
            rightNorm += right[i] * right[i];
        }

        if (leftNorm <= 0 || rightNorm <= 0)
            return 0;

        return dot / (Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm));
    }

    private sealed record LocalVectorPoint(
        string Id,
        float[] Vector,
        DocumentChunk Chunk);
}