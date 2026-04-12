using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;
using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Infrastructure.Answers;
using HistoricalDialogueRag.Infrastructure.Configuration;
using HistoricalDialogueRag.Infrastructure.Corpus;
using HistoricalDialogueRag.Infrastructure.Embeddings;
using HistoricalDialogueRag.Infrastructure.Registry;
using HistoricalDialogueRag.Infrastructure.VectorStore;

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
            .AddOptions<IndexingOptions>()
            .Bind(configuration.GetSection(IndexingOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.RegistryPath))
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
            .AddOptions<EmbeddingsOptions>()
            .Bind(configuration.GetSection(EmbeddingsOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Provider) &&
                options.BatchSize > 0 &&
                !string.IsNullOrWhiteSpace(options.Ollama.BaseUrl) &&
                !string.IsNullOrWhiteSpace(options.Ollama.Model))
            .ValidateOnStart();

        services
            .AddOptions<AnswersOptions>()
            .Bind(configuration.GetSection(AnswersOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Provider) &&
                !string.IsNullOrWhiteSpace(options.Ollama.BaseUrl) &&
                !string.IsNullOrWhiteSpace(options.Ollama.Model) &&
                options.Ollama.NumPredict > 0)
            .ValidateOnStart();

        services.AddHttpClient<OllamaEmbeddingProvider>();
        services.AddHttpClient<OllamaAnswerGenerator>();
        services.AddHttpClient<QdrantVectorStore>();

        services.AddSingleton<ICorpusDocumentProvider, ManualMarkdownCorpusDocumentProvider>();
        services.AddSingleton<IFigureProfileProvider, FileFigureProfileProvider>();

        services.AddSingleton<IEmbeddingProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<OllamaEmbeddingProvider>());

        services.AddSingleton<IVectorStore>(serviceProvider =>
            serviceProvider.GetRequiredService<QdrantVectorStore>());

        services.AddSingleton<IIndexRegistry, JsonIndexRegistry>();

        services.AddSingleton<IAnswerGenerator>(serviceProvider =>
            serviceProvider.GetRequiredService<OllamaAnswerGenerator>());

        return services;
    }
}