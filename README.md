# Historical Dialogue RAG

Р›РѕРєР°Р»СЊРЅРѕРµ ASP.NET Core РїСЂРёР»РѕР¶РµРЅРёРµ РЅР° .NET 9 РґР»СЏ РґРёР°Р»РѕРіР° СЃ РёСЃС‚РѕСЂРёС‡РµСЃРєРёРј РґРµСЏС‚РµР»РµРј РїРѕ РїРѕРґРіРѕС‚РѕРІР»РµРЅРЅРѕРјСѓ РєРѕСЂРїСѓСЃСѓ С‚РµРєСЃС‚РѕРІ.

## РўРµС…РЅРѕР»РѕРіРёРё

- .NET 9
- ASP.NET Core
- Razor Pages
- Qdrant
- Markdown + YAML front matter

## РЎС‚СЂСѓРєС‚СѓСЂР°

src/HistoricalDialogueRag.Web             ASP.NET Core, Razor Pages, API, CLI
src/HistoricalDialogueRag.Core            application Рё domain logic
src/HistoricalDialogueRag.Infrastructure  Qdrant, providers, corpus readers
data/corpus                               СЂСѓС‡РЅРѕР№ РєРѕСЂРїСѓСЃ РёСЃС‚РѕСЂРёС‡РµСЃРєРёС… С‚РµРєСЃС‚РѕРІ
data/registry                             Р»РѕРєР°Р»СЊРЅС‹Рµ СЃР»СѓР¶РµР±РЅС‹Рµ РґР°РЅРЅС‹Рµ РёРЅРґРµРєСЃР°

## РљРѕРјР°РЅРґС‹

Р—Р°РїСѓСЃРє Qdrant:

docker compose up -d

РџСЂРѕРІРµСЂРєР° РєРѕСЂРїСѓСЃР°:

dotnet run --project src/HistoricalDialogueRag.Web -- corpus validate --figure napoleon

Р—Р°РїСѓСЃРє СЃР°Р№С‚Р°/API:

dotnet run --project src/HistoricalDialogueRag.Web -- api

Health check:

http://localhost:5000/health

## Р¤РѕСЂРјР°С‚ РєРѕСЂРїСѓСЃР°

data/corpus/{figureId}/figure.json
data/corpus/{figureId}/clean/*.md

РљР°Р¶РґС‹Р№ .md РґРѕР»Р¶РµРЅ СЃРѕРґРµСЂР¶Р°С‚СЊ YAML front matter СЃ metadata Рё С‚РµРєСЃС‚ РёСЃС‚РѕС‡РЅРёРєР° РЅРёР¶Рµ.
