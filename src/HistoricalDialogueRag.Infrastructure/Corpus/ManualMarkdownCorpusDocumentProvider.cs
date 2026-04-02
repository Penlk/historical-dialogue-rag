using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Core.Domain.Corpus;
using HistoricalDialogueRag.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HistoricalDialogueRag.Infrastructure.Corpus;

public sealed class ManualMarkdownCorpusDocumentProvider : ICorpusDocumentProvider
{
    private readonly CorpusOptions _options;
    private readonly IDeserializer _yamlDeserializer;

    public ManualMarkdownCorpusDocumentProvider(IOptions<CorpusOptions> options)
    {
        _options = options.Value;
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public async Task<IReadOnlyList<SourceDocument>> GetDocumentsAsync(
        CorpusDocumentQuery query,
        CancellationToken cancellationToken)
    {
        var cleanDirectory = Path.Combine(_options.RootPath, query.FigureId, "clean");

        if (!Directory.Exists(cleanDirectory))
            return [];

        var files = Directory.GetFiles(cleanDirectory, "*.md", SearchOption.AllDirectories);
        var documents = new List<SourceDocument>();

        foreach (var file in files)
        {
            var markdown = await File.ReadAllTextAsync(file, cancellationToken);
            var document = ParseMarkdown(markdown, file);

            if (!query.IncludeDraft && document.Metadata.Quality == DocumentQuality.Draft)
                continue;

            if (!query.IncludeNeedsReview && document.Metadata.Quality == DocumentQuality.NeedsReview)
                continue;

            documents.Add(document);
        }

        return documents;
    }

    private SourceDocument ParseMarkdown(string markdown, string path)
    {
        var normalized = markdown.Replace("\r\n", "\n");

        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
            throw new InvalidOperationException($"Missing YAML front matter: {path}");

        var endIndex = normalized.IndexOf("\n---", 4, StringComparison.Ordinal);

        if (endIndex < 0)
            throw new InvalidOperationException($"Invalid YAML front matter: {path}");

        var yaml = normalized.Substring(4, endIndex - 4).Trim();
        var body = normalized.Substring(endIndex + 4).Trim();

        var dto = _yamlDeserializer.Deserialize<DocumentMetadataYaml>(yaml)
            ?? throw new InvalidOperationException($"Invalid YAML metadata: {path}");

        var metadata = dto.ToDomain(path);

        return new SourceDocument(metadata, body);
    }

    private sealed class DocumentMetadataYaml
    {
        public string? DocumentId { get; set; }
        public string? FigureId { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? DocumentType { get; set; }
        public DateTime? Date { get; set; }
        public int? Year { get; set; }
        public string? TextLanguage { get; set; }
        public string? OriginalLanguage { get; set; }
        public string? SourceName { get; set; }
        public string? SourceUrl { get; set; }
        public string? License { get; set; }
        public string? Quality { get; set; }

        public DocumentMetadata ToDomain(string path)
        {
            return new DocumentMetadata(
                Required(DocumentId, nameof(DocumentId), path),
                Required(FigureId, nameof(FigureId), path),
                Required(Title, nameof(Title), path),
                Required(Author, nameof(Author), path),
                ParseEnum<HistoricalDocumentType>(DocumentType, nameof(DocumentType), path),
                Date is null ? null : DateOnly.FromDateTime(Date.Value),
                Year,
                Required(TextLanguage, nameof(TextLanguage), path),
                OriginalLanguage,
                Required(SourceName, nameof(SourceName), path),
                Required(SourceUrl, nameof(SourceUrl), path),
                Required(License, nameof(License), path),
                ParseEnum<DocumentQuality>(Quality, nameof(Quality), path));
        }

        private static string Required(string? value, string fieldName, string path)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Missing required field '{fieldName}' in {path}");

            return value.Trim();
        }

        private static TEnum ParseEnum<TEnum>(string? value, string fieldName, string path)
            where TEnum : struct
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Missing required field '{fieldName}' in {path}");

            if (!Enum.TryParse<TEnum>(value.Trim(), ignoreCase: true, out var parsed))
                throw new InvalidOperationException($"Invalid value '{value}' for field '{fieldName}' in {path}");

            return parsed;
        }
    }
}
