using System.Text.Json;
using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Core.Domain.Figures;
using HistoricalDialogueRag.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace HistoricalDialogueRag.Infrastructure.Corpus;

public sealed class FileFigureProfileProvider : IFigureProfileProvider
{
    private readonly CorpusOptions _options;

    public FileFigureProfileProvider(IOptions<CorpusOptions> options)
    {
        _options = options.Value;
    }

    public async Task<HistoricalFigure> GetFigureAsync(
        string figureId,
        CancellationToken cancellationToken)
    {
        var path = ProjectPathResolver.Resolve(
            Path.Combine(_options.RootPath, figureId, "figure.json"));

        if (!File.Exists(path))
            throw new FileNotFoundException($"Figure profile not found: {path}");

        await using var stream = File.OpenRead(path);

        var figure = await JsonSerializer.DeserializeAsync<HistoricalFigure>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            cancellationToken);

        return figure ?? throw new InvalidOperationException($"Invalid figure profile: {path}");
    }

    public async Task<IReadOnlyList<HistoricalFigure>> GetFiguresAsync(
        CancellationToken cancellationToken)
    {
        var corpusRoot = ProjectPathResolver.Resolve(_options.RootPath);

        if (!Directory.Exists(corpusRoot))
            return [];

        var figures = new List<HistoricalFigure>();

        foreach (var directory in Directory.GetDirectories(corpusRoot))
        {
            var figureId = Path.GetFileName(directory);
            var profilePath = Path.Combine(directory, "figure.json");

            if (!File.Exists(profilePath))
                continue;

            figures.Add(await GetFigureAsync(figureId, cancellationToken));
        }

        return figures;
    }
}