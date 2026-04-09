using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Core.Application.Contracts.Indexing;

namespace HistoricalDialogueRag.Core.Application.Services.Indexing;

public sealed class CorpusIndexingService : ICorpusIndexingService
{
    private readonly ICorpusDocumentProvider _documentProvider;
    private readonly ICorpusValidator _corpusValidator;
    private readonly IChunker _chunker;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;
    private readonly IIndexRegistry _indexRegistry;

    public CorpusIndexingService(
        ICorpusDocumentProvider documentProvider,
        ICorpusValidator corpusValidator,
        IChunker chunker,
        IEmbeddingProvider embeddingProvider,
        IVectorStore vectorStore,
        IIndexRegistry indexRegistry)
    {
        _documentProvider = documentProvider;
        _corpusValidator = corpusValidator;
        _chunker = chunker;
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
        _indexRegistry = indexRegistry;
    }

    public async Task<IndexCorpusResult> IndexAsync(
        IndexCorpusRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FigureId))
            throw new ArgumentException("FigureId is required.", nameof(request));

        var validationReport = await _corpusValidator.ValidateAsync(
            request.FigureId,
            cancellationToken);

        if (!validationReport.IsValid)
        {
            var errors = string.Join(Environment.NewLine, validationReport.Errors);
            throw new InvalidOperationException($"Corpus is invalid:{Environment.NewLine}{errors}");
        }

        var documents = await _documentProvider.GetDocumentsAsync(
            new CorpusDocumentQuery(request.FigureId),
            cancellationToken);

        if (documents.Count == 0)
            throw new InvalidOperationException($"No checked corpus documents found for figure '{request.FigureId}'.");

        var chunks = documents
            .SelectMany(document => _chunker.Chunk(document))
            .ToList();

        if (chunks.Count == 0)
            throw new InvalidOperationException($"No chunks were created for figure '{request.FigureId}'. Check document body content and chunking rules.");

        var texts = chunks
            .Select(chunk => chunk.Text)
            .ToList();

        var vectors = await _embeddingProvider.EmbedDocumentsAsync(
            texts,
            cancellationToken);

        if (vectors.Count != chunks.Count)
            throw new InvalidOperationException("Embedding provider returned unexpected vector count.");

        var points = chunks
            .Select((chunk, index) => new VectorPoint(
                Id: chunk.ChunkId,
                Vector: vectors[index],
                Chunk: chunk))
            .ToList();

        await _vectorStore.UpsertAsync(points, cancellationToken);

        var result = new IndexCorpusResult(
            request.FigureId,
            documents.Count,
            chunks.Count);

        await _indexRegistry.SaveAsync(
            new IndexRegistryEntry(
                result.FigureId,
                DateTimeOffset.UtcNow,
                result.DocumentCount,
                result.ChunkCount),
            cancellationToken);

        return result;
    }
}