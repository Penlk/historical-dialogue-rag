using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Web.Composition;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddCoreServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

if (args.Length > 0)
{
    await using var scope = app.Services.CreateAsyncScope();

    if (args is ["corpus", "validate", "--figure", var figureId])
    {
        var validator = scope.ServiceProvider.GetRequiredService<ICorpusValidator>();
        var report = await validator.ValidateAsync(figureId, CancellationToken.None);

        Console.WriteLine($"Figure: {report.FigureId}");
        Console.WriteLine($"Documents: {report.DocumentCount}");
        Console.WriteLine($"Valid: {report.IsValid}");

        foreach (var error in report.Errors)
            Console.WriteLine($"ERROR: {error}");

        foreach (var warning in report.Warnings)
            Console.WriteLine($"WARNING: {warning}");

        return;
    }

    if (args is ["api"])
    {
        // Continue startup.
    }
    else
    {
        Console.Error.WriteLine("Unsupported command.");
        Console.Error.WriteLine("Available commands:");
        Console.Error.WriteLine("  corpus validate --figure <figureId>");
        Console.Error.WriteLine("  api");
        Environment.ExitCode = 1;
        return;
    }
}

app.UseStaticFiles();
app.UseRouting();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/figures", async (
    IFigureProfileProvider figureProvider,
    CancellationToken cancellationToken) =>
{
    var figures = await figureProvider.GetFiguresAsync(cancellationToken);
    return Results.Ok(figures);
});

app.MapRazorPages();

app.Run();
