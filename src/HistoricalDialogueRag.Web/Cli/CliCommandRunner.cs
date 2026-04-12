using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Core.Application.Abstractions.Indexing;
using HistoricalDialogueRag.Core.Application.Contracts.Indexing;

namespace HistoricalDialogueRag.Web.Cli;

public static class CliCommandRunner
{
    public static async Task<bool> TryRunCliCommandAsync(
        this WebApplication app,
        string[] args)
    {
        if (args.Length == 0)
            return false;

        if (args is ["api"])
            return false;

        await using var scope = app.Services.CreateAsyncScope();

        if (args is ["corpus", "validate", "--figure", var validateFigureId])
        {
            await RunValidateCorpusAsync(scope.ServiceProvider, validateFigureId);
            return true;
        }

        if (args is ["ingest", "--figure", var ingestFigureId])
        {
            await RunIngestAsync(scope.ServiceProvider, ingestFigureId);
            return true;
        }

        if (args is ["rebuild", "--figure", var rebuildFigureId])
        {
            await RunRebuildAsync(scope.ServiceProvider, rebuildFigureId);
            return true;
        }

        PrintUnsupportedCommand();
        Environment.ExitCode = 1;
        return true;
    }

    private static async Task RunValidateCorpusAsync(
        IServiceProvider serviceProvider,
        string figureId)
    {
        var validator = serviceProvider.GetRequiredService<ICorpusValidator>();
        var report = await validator.ValidateAsync(figureId, CancellationToken.None);

        Console.WriteLine($"Figure: {report.FigureId}");
        Console.WriteLine($"Documents: {report.DocumentCount}");
        Console.WriteLine($"Valid: {report.IsValid}");

        foreach (var error in report.Errors)
            Console.WriteLine($"ERROR: {error}");

        foreach (var warning in report.Warnings)
            Console.WriteLine($"WARNING: {warning}");

        Environment.ExitCode = report.IsValid ? 0 : 1;
    }

    private static async Task RunIngestAsync(
        IServiceProvider serviceProvider,
        string figureId)
    {
        var indexingService = serviceProvider.GetRequiredService<ICorpusIndexingService>();

        var result = await indexingService.IndexAsync(
            new IndexCorpusRequest(figureId),
            CancellationToken.None);

        Console.WriteLine("Ingest completed.");
        Console.WriteLine($"Figure: {result.FigureId}");
        Console.WriteLine($"Documents: {result.DocumentCount}");
        Console.WriteLine($"Chunks: {result.ChunkCount}");
    }

    private static async Task RunRebuildAsync(
        IServiceProvider serviceProvider,
        string figureId)
    {
        var rebuildService = serviceProvider.GetRequiredService<IRebuildIndexService>();

        var result = await rebuildService.RebuildAsync(
            figureId,
            CancellationToken.None);

        Console.WriteLine("Rebuild completed.");
        Console.WriteLine($"Figure: {result.FigureId}");
        Console.WriteLine($"Documents: {result.DocumentCount}");
        Console.WriteLine($"Chunks: {result.ChunkCount}");
    }

    private static void PrintUnsupportedCommand()
    {
        Console.Error.WriteLine("Unsupported command.");
        Console.Error.WriteLine("Available commands:");
        Console.Error.WriteLine("  corpus validate --figure <figureId>");
        Console.Error.WriteLine("  ingest --figure <figureId>");
        Console.Error.WriteLine("  rebuild --figure <figureId>");
        Console.Error.WriteLine("  api");
    }
}