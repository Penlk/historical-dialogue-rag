using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Infrastructure.Configuration;
using HistoricalDialogueRag.Infrastructure.Corpus;
using HistoricalDialogueRag.Infrastructure.Embeddings;
using HistoricalDialogueRag.Infrastructure.Registry;
using HistoricalDialogueRag.Infrastructure.VectorStore;
using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;
using HistoricalDialogueRag.Infrastructure.Answers;

namespace HistoricalDialogueRag.Web.Composition;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<CorpusOptions>()
            .Bind(configuration.GetSection(CorpusOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.RootPath))
            .ValidateOnStart();

        services
            .AddOptions<QdrantOptions>()
            .Bind(configuration.GetSection(QdrantOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.BaseUrl) &&
                !string.IsNullOrWhiteSpace(options.CollectionName) &&
                options.VectorSize > 0)
            .ValidateOnStart();

        services
            .AddOptions<IndexingOptions>()
            .Bind(configuration.GetSection(IndexingOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.RegistryPath) &&
                !string.IsNullOrWhiteSpace(options.LocalVectorStorePath) &&
                options.DevVectorSize > 0)
            .ValidateOnStart();

        services.AddHttpClient("Qdrant");

        services.AddSingleton<ICorpusDocumentProvider, ManualMarkdownCorpusDocumentProvider>();
        services.AddSingleton<IFigureProfileProvider, FileFigureProfileProvider>();

        services.AddSingleton<IEmbeddingProvider, DeterministicDevEmbeddingProvider>();
        services.AddSingleton<IVectorStore, QdrantVectorStore>();
        services.AddSingleton<IIndexRegistry, JsonIndexRegistry>();
        services.AddSingleton<IAnswerGenerator, DevAnswerGenerator>();

        return services;
    }
}