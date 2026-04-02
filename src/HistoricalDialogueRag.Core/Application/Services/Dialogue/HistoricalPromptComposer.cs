using HistoricalDialogueRag.Core.Application.Abstractions.Dialogue;

namespace HistoricalDialogueRag.Core.Application.Services.Dialogue;

public sealed class HistoricalPromptComposer : IPromptComposer
{
    public string Compose(HistoricalPromptInput input)
    {
        var context = string.Join("\n\n", input.ContextBlocks);

        return $"""
        РўС‹ РёРјРёС‚РёСЂСѓРµС€СЊ РёСЃС‚РѕСЂРёС‡РµСЃРєРѕРіРѕ РґРµСЏС‚РµР»СЏ: {input.FigureName}.

        РЎС‚РёР»СЊ РѕС‚РІРµС‚Р°:
        {input.PersonaStyle}

        РџСЂР°РІРёР»Р°:
        1. РћС‚РІРµС‡Р°Р№ С‚РѕР»СЊРєРѕ РЅР° РѕСЃРЅРѕРІРµ РїСЂРµРґРѕСЃС‚Р°РІР»РµРЅРЅРѕРіРѕ РєРѕРЅС‚РµРєСЃС‚Р°.
        2. РќРµ РґРѕР±Р°РІР»СЏР№ С„Р°РєС‚С‹, РєРѕС‚РѕСЂС‹С… РЅРµС‚ РІ РєРѕРЅС‚РµРєСЃС‚Рµ.
        3. Р•СЃР»Рё РєРѕРЅС‚РµРєСЃС‚Р° РЅРµРґРѕСЃС‚Р°С‚РѕС‡РЅРѕ, РїСЂСЏРјРѕ СЃРєР°Р¶Рё РѕР± СЌС‚РѕРј.
        4. РњРѕР¶РЅРѕ РѕС‚РІРµС‡Р°С‚СЊ РѕС‚ РїРµСЂРІРѕРіРѕ Р»РёС†Р°, РЅРѕ С‚РѕР»СЊРєРѕ РІ СЂР°РјРєР°С… РёСЃС‚РѕС‡РЅРёРєРѕРІ.

        РљРѕРЅС‚РµРєСЃС‚:
        {context}

        Р’РѕРїСЂРѕСЃ:
        {input.Question}
        """;
    }
}
