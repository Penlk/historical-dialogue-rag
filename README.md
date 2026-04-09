# Historical Dialogue RAG

Local ASP.NET Core application on .NET 9 for grounded dialogue with a historical figure over a prepared text corpus.

## Current stage

- Manual Markdown corpus with YAML front matter
- Corpus validation command
- Index/rebuild pipeline
- Qdrant vector store
- Dev deterministic embeddings
- Local dev dialogue flow

## Run Qdrant

docker compose up -d

Qdrant UI:

http://localhost:6333/dashboard

## Validate corpus

dotnet run --project src/HistoricalDialogueRag.Web -- corpus validate --figure lenin

## Ingest corpus

dotnet run --project src/HistoricalDialogueRag.Web -- ingest --figure lenin

## Rebuild corpus index

dotnet run --project src/HistoricalDialogueRag.Web -- rebuild --figure lenin

## Run site/API

dotnet run --project src/HistoricalDialogueRag.Web -- api

Open the printed ASP.NET Core URL, usually:

http://localhost:5133

## Corpus format

data/corpus/{figureId}/figure.json
data/corpus/{figureId}/clean/*.md

Each Markdown file must contain YAML front matter and the source text below it.

## Notes

The current embedding provider is deterministic and intended only for local pipeline testing.
Qdrant is already used as the vector store, but real semantic search quality requires a real embedding provider.