using UnityEngine;

public class CharacterBrain
{
    private const int PolicyBonus = 15;
    private const int StatBonusPerLevel = 1;
    private const int StatBonusMax = 10;

    public float ApplyCharacterPreferences(
        GoapGoal goal,
        CharacterData data,
        BigTurnSelectionData selection)
    {
        float utility = goal.Utility;

        switch (goal.Type)
        {
            case CharacterGoalType.GatherResource:
                utility += GetStatBonus(data, StatType.Gather);
                if (selection.Policy == PolicyType.GatherFirst) utility += PolicyBonus;
                break;
            case CharacterGoalType.CraftItem:
                utility += GetStatBonus(data, StatType.Craft);
                if (selection.Policy == PolicyType.CraftFirst) utility += PolicyBonus;
                break;
            case CharacterGoalType.BuildStructure:
                utility += GetStatBonus(data, StatType.Build);
                if (selection.Policy == PolicyType.BuildFirst) utility += PolicyBonus;
                break;
            case CharacterGoalType.Socialize:
                utility += GetStatBonus(data, StatType.Social);
                if (selection.Policy == PolicyType.SocialFirst) utility += PolicyBonus;
                break;
        }

        return Mathf.Max(0f, utility);
    }

    private int GetStatBonus(CharacterData data, StatType type)
    {
        if (data.Stats == null || !data.Stats.TryGetValue(type, out int level)) level = 1;

        level = Mathf.Clamp(level, 1, 11);
        int bonus = (level - 1) * StatBonusPerLevel;
        return Mathf.Clamp(bonus, 0, StatBonusMax);
    }
}
