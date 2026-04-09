using System.ComponentModel.DataAnnotations;
using HistoricalDialogueRag.Core.Application.Abstractions.Corpus;
using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;
using HistoricalDialogueRag.Core.Application.Contracts.Dialogue;
using HistoricalDialogueRag.Core.Domain.Figures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HistoricalDialogueRag.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IDialogueService _dialogueService;
    private readonly IFigureProfileProvider _figureProfileProvider;

    public IndexModel(
        IDialogueService dialogueService,
        IFigureProfileProvider figureProfileProvider)
    {
        _dialogueService = dialogueService;
        _figureProfileProvider = figureProfileProvider;
    }

    [BindProperty]
    public AskInputModel Input { get; set; } = new();

    public IReadOnlyList<HistoricalFigure> Figures { get; private set; } = [];

    public AskResponse? AskResponse { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadFiguresAsync(cancellationToken);
        SetDefaultFigureIfNeeded();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadFiguresAsync(cancellationToken);

        if (!ModelState.IsValid)
            return Page();

        try
        {
            AskResponse = await _dialogueService.AskAsync(
                new AskRequest(
                    Input.FigureId,
                    Input.Question,
                    Input.TopK,
                    Input.MinScore),
                cancellationToken);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or FileNotFoundException)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }

    private async Task LoadFiguresAsync(CancellationToken cancellationToken)
    {
        Figures = await _figureProfileProvider.GetFiguresAsync(cancellationToken);
    }

    private void SetDefaultFigureIfNeeded()
    {
        if (!string.IsNullOrWhiteSpace(Input.FigureId))
            return;

        Input.FigureId = Figures.FirstOrDefault()?.FigureId ?? "lenin";
    }

    public sealed class AskInputModel
    {
        [Required]
        public string FigureId { get; set; } = "lenin";

        [Required]
        [StringLength(2000)]
        public string Question { get; set; } = "What is the main idea of the available text?";

        [Range(1, 20)]
        public int TopK { get; set; } = 6;

        [Range(0, 1)]
        public double MinScore { get; set; } = 0.0;
    }
}