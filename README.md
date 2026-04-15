# Historical Dialogue RAG

ASP.NET Core приложение для диалога с историческим деятелем на основе подготовленного корпуса его текстов.

Проект использует RAG-подход: пользователь задаёт вопрос, система ищет релевантные фрагменты в корпусе, передаёт их в LLM и формирует ответ с указанием источников.

## Возможности

- диалог с выбранным историческим деятелем;
- ответы строго на основе найденных источников;
- отображение использованных фрагментов и ссылок на источники;
- ручной корпус в Markdown с YAML metadata;
- валидация корпуса перед индексацией;
- пересборка индекса одной CLI-командой;
- web-интерфейс на Razor Pages.

## Технологии

- .NET 9
- ASP.NET Core
- Razor Pages
- Qdrant
- Ollama
- Markdown + YAML front matter
- Docker Compose

## Архитектура

```text
src/
  HistoricalDialogueRag.Core
    Domain models
    Application contracts
    Dialogue / indexing / validation services

  HistoricalDialogueRag.Infrastructure
    Markdown corpus reader
    Qdrant vector store
    Ollama embeddings
    Ollama answer generation
    JSON index registry

  HistoricalDialogueRag.Web
    Razor Pages UI
    Minimal API endpoints
    CLI commands
    DI composition
```

Основная идея разделения:

```text
Web            ввод/вывод, UI, API, CLI
Core           бизнес-сценарии и абстракции
Infrastructure внешние системы и файловая система
```

## Формат корпуса

Корпус хранится в `data/corpus`.

```text
data/
  corpus/
    lenin/
      figure.json
      clean/
        april-theses-1917.md
        state-and-revolution-chapter-1-1917.md
```

`figure.json` описывает исторического деятеля:

```json
{
  "figureId": "lenin",
  "displayName": "Владимир Ленин",
  "fullName": "Владимир Ильич Ленин",
  "lifeYears": "1870–1924",
  "defaultLanguage": "ru",
  "persona": {
    "shortDescription": "Российский революционер, политический теоретик и лидер большевиков.",
    "answerStyle": "Отвечай от первого лица, в резком, полемическом и теоретически уверенном стиле. Используй только идеи, подтверждённые источниками.",
    "fallbackMessage": "В доступных текстах нет достаточного основания для надёжного ответа."
  }
}
```

Каждый документ корпуса — это `.md` файл с YAML metadata:

```md
---
documentId: lenin-april-theses-1917
figureId: lenin
title: Апрельские тезисы
author: Владимир Ильич Ленин
documentType: Article
date: 1917-04-07
year: 1917
textLanguage: ru
originalLanguage: ru
sourceName: Викитека
sourceUrl: "https://ru.wikisource.org/wiki/..."
license: public-domain
quality: Checked
---

# Апрельские тезисы

Текст источника...
```

## Быстрый запуск

### 1. Запустить Qdrant

```bash
docker compose up -d
```

Qdrant dashboard:

```text
http://localhost:6333/dashboard
```

### 2. Установить модели Ollama

```bash
ollama pull nomic-embed-text
ollama pull llama3.1
```

### 3. Проверить корпус

```bash
dotnet run --project src/HistoricalDialogueRag.Web -- corpus validate --figure lenin
```

### 4. Пересобрать индекс

```bash
dotnet run --project src/HistoricalDialogueRag.Web -- rebuild --figure lenin
```

### 5. Запустить web-приложение

```bash
dotnet run --project src/HistoricalDialogueRag.Web -- api
```

По умолчанию приложение доступно по адресу:

```text
http://localhost:5133
```

## API

### Получить список деятелей

```http
GET /api/figures
```

### Задать вопрос

```http
POST /api/dialogue/ask
```

Пример запроса:

```json
{
  "figureId": "lenin",
  "question": "Что такое государство?",
  "topK": 6,
  "minScore": 0.0
}
```

Пример ответа:

```json
{
  "answer": "...",
  "usedContext": true,
  "sources": [
    {
      "documentTitle": "Государство и революция. Глава I",
      "author": "Владимир Ильич Ленин",
      "documentType": "BookFragment",
      "year": 1917,
      "sourceName": "Викитека",
      "sourceUrl": "https://...",
      "chunkIndex": 0,
      "score": 0.73
    }
  ],
  "message": null
}
```

## CLI-команды

Проверить корпус:

```bash
dotnet run --project src/HistoricalDialogueRag.Web -- corpus validate --figure lenin
```

Индексировать корпус:

```bash
dotnet run --project src/HistoricalDialogueRag.Web -- ingest --figure lenin
```

Полностью пересобрать индекс:

```bash
dotnet run --project src/HistoricalDialogueRag.Web -- rebuild --figure lenin
```

Запустить сайт и API:

```bash
dotnet run --project src/HistoricalDialogueRag.Web -- api
```

## Что не хранится в Git

В репозиторий добавляется исходный корпус:

```text
data/corpus/
```

Не добавляются runtime-артефакты:

```text
data/registry/
bin/
obj/
Qdrant storage
.env
```

## Статус проекта

MVP-версия поддерживает:

- ручной корпус исторических текстов;
- валидацию Markdown-документов;
- индексацию в Qdrant;
- embeddings через Ollama;
- генерацию ответа через Ollama;
- web-интерфейс для диалога;
- отображение источников ответа.
