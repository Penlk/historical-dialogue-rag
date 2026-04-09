# Historical Dialogue RAG

Local ASP.NET Core application on .NET 9 for dialogue with a historical figure using a prepared text corpus.

## Stack

- .NET 9
- ASP.NET Core
- Razor Pages
- Qdrant later
- Markdown with YAML front matter
- Local dev vector store for early MVP checks

## Structure

```text
src/HistoricalDialogueRag.Web             ASP.NET Core, Razor Pages, API, CLI
src/HistoricalDialogueRag.Core            application and domain logic
src/HistoricalDialogueRag.Infrastructure  corpus readers, vector store, providers
data/corpus                               manually prepared historical texts
data/registry                             local runtime index files
```

## Commands

Validate corpus:

```powershell
dotnet run --project src/HistoricalDialogueRag.Web -- corpus validate --figure lenin
```

Ingest corpus:

```powershell
dotnet run --project src/HistoricalDialogueRag.Web -- ingest --figure lenin
```

Rebuild index:

```powershell
dotnet run --project src/HistoricalDialogueRag.Web -- rebuild --figure lenin
```

Run web/API:

```powershell
dotnet run --project src/HistoricalDialogueRag.Web -- api
```

Health check:

```text
http://localhost:5133/health
```

## Corpus format

```text
data/corpus/{figureId}/figure.json
data/corpus/{figureId}/clean/*.md
```

Each Markdown file must start with YAML front matter and contain source text below it.