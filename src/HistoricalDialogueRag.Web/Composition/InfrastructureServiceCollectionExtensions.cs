using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Infrastructure.Configuration;
using HistoricalDialogueRag.Infrastructure.Corpus;

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

        services.AddSingleton<ICorpusDocumentProvider, ManualMarkdownCorpusDocumentProvider>();
        services.AddSingleton<IFigureProfileProvider, FileFigureProfileProvider>();

        return services;
    }
}
