using System.Text.Json;
using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HistoricalDialogueRag.Infrastructure.Registry;

public sealed class JsonIndexRegistry : IIndexRegistry
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _path;

    public JsonIndexRegistry(IOptions<IndexingOptions> options)
    {
        _path = ProjectPathResolver.Resolve(options.Value.RegistryPath);
    }

    public async Task SaveAsync(
        IndexRegistryEntry entry,
        CancellationToken cancellationToken)
    {
        var entries = await LoadAsync(cancellationToken);
        entries[entry.FigureId] = entry;

        var directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await using var stream = File.Create(_path);

        await JsonSerializer.SerializeAsync(
            stream,
            entries.Values.OrderBy(item => item.FigureId).ToList(),
            JsonOptions,
            cancellationToken);
    }

    public async Task<IndexRegistryEntry?> GetAsync(
        string figureId,
        CancellationToken cancellationToken)
    {
        var entries = await LoadAsync(cancellationToken);
        return entries.GetValueOrDefault(figureId);
    }

    private async Task<Dictionary<string, IndexRegistryEntry>> LoadAsync(
        CancellationToken cancellationToken)
    {
        if (!File.Exists(_path))
            return new Dictionary<string, IndexRegistryEntry>(StringComparer.OrdinalIgnoreCase);

        await using var stream = File.OpenRead(_path);

        var entries = await JsonSerializer.DeserializeAsync<List<IndexRegistryEntry>>(
            stream,
            JsonOptions,
            cancellationToken);

        return (entries ?? [])
            .ToDictionary(entry => entry.FigureId, StringComparer.OrdinalIgnoreCase);
    }
}