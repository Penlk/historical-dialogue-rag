using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;
using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Core.Application.Services.Corpus;
using HistoricalDialogueRag.Core.Application.Services.Dialogue;
using HistoricalDialogueRag.Core.Application.Services.Indexing;

namespace HistoricalDialogueRag.Web.Composition;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<ICorpusValidator, CorpusValidationService>();
        services.AddSingleton<IChunker, MarkdownChunker>();
        services.AddSingleton<ICorpusIndexingService, CorpusIndexingService>();
        services.AddSingleton<IRebuildIndexService, RebuildIndexService>();
        services.AddSingleton<IContextRetriever, ContextRetriever>();
        services.AddSingleton<IPromptComposer, HistoricalPromptComposer>();
        services.AddSingleton<IDialogueService, DialogueService>();

        return services;
    }
}