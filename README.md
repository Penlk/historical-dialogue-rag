# Historical Dialogue RAG

Local ASP.NET Core app on .NET 9 for grounded dialogue with a historical figure over a prepared text corpus.

## Stack

- .NET 9
- ASP.NET Core
- Razor Pages
- Qdrant later; local JSON vector store for current dev step
- Markdown + YAML front matter corpus

## Current MVP flow

```text
manual corpus -> validate -> ingest -> local vector store -> ask -> dev answer with sources
```

## Commands

Validate corpus:

```bash
dotnet run --project src/HistoricalDialogueRag.Web -- corpus validate --figure lenin
```

Index corpus:

```bash
dotnet run --project src/HistoricalDialogueRag.Web -- ingest --figure lenin
```

Rebuild index:

```bash
dotnet run --project src/HistoricalDialogueRag.Web -- rebuild --figure lenin
```

Run site/API:

```bash
dotnet run --project src/HistoricalDialogueRag.Web -- api
```

Open the URL printed by ASP.NET Core, usually:

```text
http://localhost:5133
```

Ask endpoint:

```http
POST /api/dialogue/ask
```

Example request:

```json
{
  "figureId": "lenin",
  "question": "What is the main idea of the available text?",
  "topK": 6,
  "minScore": 0.0
}
```

## Corpus format

```text
data/corpus/{figureId}/figure.json
data/corpus/{figureId}/clean/*.md
```

Each `.md` file must contain YAML front matter and source text below it.