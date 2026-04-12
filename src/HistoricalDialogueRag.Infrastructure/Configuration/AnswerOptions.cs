namespace HistoricalDialogueRag.Infrastructure.Configuration;

public sealed class AnswersOptions
{
    public const string SectionName = "Answers";

    public string Provider { get; init; } = "ollama";
    public OllamaAnswerOptions Ollama { get; init; } = new();
}

public sealed class OllamaAnswerOptions
{
    public string BaseUrl { get; init; } = "http://localhost:11434";
    public string Model { get; init; } = "llama3.1";
    public double Temperature { get; init; } = 0.2;
    public int NumPredict { get; init; } = 700;
}