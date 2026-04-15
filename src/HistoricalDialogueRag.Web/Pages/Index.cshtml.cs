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

    public bool HasFigures => Figures.Count > 0;

    public string SelectedFigureName
    {
        get
        {
            var selected = Figures.FirstOrDefault(figure =>
                figure.FigureId.Equals(Input.FigureId, StringComparison.OrdinalIgnoreCase));

            return selected?.DisplayName
                   ?? Figures.FirstOrDefault()?.DisplayName
                   ?? "не выбран";
        }
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadFiguresAsync(cancellationToken);
        SetDefaultFigureIfNeeded();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadFiguresAsync(cancellationToken);
        SetDefaultFigureIfNeeded();

        if (!ModelState.IsValid)
            return Page();

        if (!HasFigures)
        {
            ErrorMessage = "Не найден ни один исторический деятель в data/corpus.";
            return Page();
        }

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

        [Required(ErrorMessage = "Введите вопрос.")]
        [StringLength(2000, ErrorMessage = "Вопрос слишком длинный.")]
        public string Question { get; set; } = "Что такое государство?";

        [Range(1, 20)]
        public int TopK { get; set; } = 6;

        [Range(0, 1)]
        public double MinScore { get; set; } = 0.0;
    }
}