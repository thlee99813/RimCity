using UnityEngine;
using System.Collections.Generic;

public class CharacterBrain
{
    private const int PolicyBonus = 15;
    private const int StatBonusPerLevel = 1;
    private const int StatBonusMax = 10;

    public SmallTurnActionType DecideSmallTurnAction(
    CharacterData data,
    CharacterStatus status,
    CharacterEquipment equipment,
    PlayerResourceInventory inventory,
    BigTurnSelectionData selection)
    {
        Dictionary<SmallTurnActionType, int> weights = new Dictionary<SmallTurnActionType, int>
        {
            { SmallTurnActionType.Idle, 10 },
            { SmallTurnActionType.Wander, 10 },
            { SmallTurnActionType.Gather, 20 },
            { SmallTurnActionType.Craft, 15 },
            { SmallTurnActionType.Build, 15 },
            { SmallTurnActionType.Social, 10 },
            { SmallTurnActionType.Rest, 5 },
            { SmallTurnActionType.Eat, 5 }
        };

        if (selection.Policy == PolicyType.GatherFirst) weights[SmallTurnActionType.Gather] += PolicyBonus;
        if (selection.Policy == PolicyType.CraftFirst) weights[SmallTurnActionType.Craft] += PolicyBonus;
        if (selection.Policy == PolicyType.BuildFirst) weights[SmallTurnActionType.Build] += PolicyBonus;
        if (selection.Policy == PolicyType.SocialFirst) weights[SmallTurnActionType.Social] += PolicyBonus;

        weights[SmallTurnActionType.Gather] += GetStatBonus(data, StatType.Gather);
        weights[SmallTurnActionType.Craft] += GetStatBonus(data, StatType.Craft);
        weights[SmallTurnActionType.Build] += GetStatBonus(data, StatType.Build);
        weights[SmallTurnActionType.Social] += GetStatBonus(data, StatType.Social);

        if (inventory.Bandage <= 0) weights[SmallTurnActionType.Craft] += 2;
        if (inventory.Medkit <= 0)  weights[SmallTurnActionType.Craft] += 4;



        if (inventory.WoodenSpear > 0 && equipment.Weapon != WeaponType.WoodenSpear)
            weights[SmallTurnActionType.EquipWoodenSpear] = 48;

        if (inventory.StoneSpear > 0 && equipment.Weapon != WeaponType.StoneSpear)
            weights[SmallTurnActionType.EquipStoneSpear] = 64;

        if (inventory.Fan > 0 && equipment.Utility != UtilityType.Fan)
            weights[SmallTurnActionType.EquipFan] = 40;

        if (status.Health <= data.MaxHealth * 0.5f && inventory.Bandage > 0)
            weights[SmallTurnActionType.UseBandage] = 120;

        if (status.Health <= data.MaxHealth * 0.3f && inventory.Medkit > 0)
            weights[SmallTurnActionType.UseMedkit] = 220;

        return WeightedPick(weights);
    }
    private T WeightedPick<T>(Dictionary<T, int> weights)
    {
        int total = 0;
        foreach (KeyValuePair<T, int> entry in weights) total += entry.Value;

        int roll = Random.Range(0, total);
        int cumulative = 0;

        foreach (KeyValuePair<T, int> entry in weights)
        {
            cumulative += entry.Value;
            if (roll < cumulative) return entry.Key;
        }

        foreach (KeyValuePair<T, int> entry in weights) return entry.Key;
        return default;
    }

    private int GetStatBonus(CharacterData data, StatType type)
    {
        if (data == null || data.Stats == null) return 0;
        if (!data.Stats.TryGetValue(type, out int level)) level = 1;

        level = Mathf.Clamp(level, 1, 11);
        int bonus = (level - 1) * StatBonusPerLevel;
        return Mathf.Clamp(bonus, 0, StatBonusMax);
    }

}
