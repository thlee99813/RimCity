using UnityEngine;

public class IdleAction : CharacterAction
{
    public override string Id => "idle";
    public override float GetWeight(CharacterData data, TurnContext ctx) => 1f;

    public override ActionResult Execute(CharacterEntity thischaracter, TurnContext ctx)
    {
        return new ActionResult
        {
            LogText = $"{thischaracter.Data.Name}은(는) 잠시 멈춰 서 있습니다.",
            HealthDelta = 0f,
            MoodDelta = 0f
        };
    }
}