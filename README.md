# Historical Dialogue RAG

Local ASP.NET Core app on .NET 9 for grounded dialogue with a historical figure over a prepared text corpus.

## Current MVP stack

- .NET 9
- ASP.NET Core Razor Pages
- Qdrant
- Ollama embeddings
- Markdown corpus with YAML front matter

## Main commands

Start Qdrant:

docker compose up -d

Pull the embedding model:

ollama pull nomic-embed-text

Validate corpus:

dotnet run --project src/HistoricalDialogueRag.Web -- corpus validate --figure lenin

Rebuild index:

dotnet run --project src/HistoricalDialogueRag.Web -- rebuild --figure lenin

Run web app:

dotnet run --project src/HistoricalDialogueRag.Web -- api

Qdrant dashboard:

http://localhost:6333/dashboard

## Corpus format

data/corpus/{figureId}/figure.json
data/corpus/{figureId}/clean/*.md

Each Markdown document must contain YAML front matter and source text below it.

## Notes

The current answer generator is still a dev stub. Retrieval uses real Ollama embeddings and Qdrant.