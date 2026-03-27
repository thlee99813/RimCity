using UnityEngine;

public class CharacterConditionController
{
    public void Tick(CharacterData data, TurnContext ctx)
    {
        // 욕구 감소
        data.Needs[NeedType.Hunger] = Mathf.Max(0, data.Needs[NeedType.Hunger] - 5f);
        data.Needs[NeedType.Sleep]  = Mathf.Max(0, data.Needs[NeedType.Sleep] - 4f);
        data.Needs[NeedType.Fun]    = Mathf.Max(0, data.Needs[NeedType.Fun] - 3f);

        // 욕구 -> 기분 반영
        float needPenalty = 0f;
        foreach (var n in data.Needs.Values) needPenalty += (100f - n) * 0.01f;
        data.Mood = Mathf.Clamp(data.Mood - needPenalty, 0f, 100f);

        // 날씨/이벤트 체력 영향
        data.Health = Mathf.Clamp(data.Health + ctx.WeatherHealthDeltaPerTurn, 0f, 100f);
    }

    public void ApplyActionResult(CharacterData data, ActionResult result, TurnContext ctx)
    {
        data.Health = Mathf.Clamp(data.Health + result.HealthDelta, 0f, 100f);
        data.Mood = Mathf.Clamp(data.Mood + result.MoodDelta, 0f, 100f);

        if (result.NeedDelta != null)
        {
            foreach (var kv in result.NeedDelta)
            {
                data.Needs[kv.Key] = Mathf.Clamp(data.Needs[kv.Key] + kv.Value, 0f, 100f);
            }
        }
    }
}
