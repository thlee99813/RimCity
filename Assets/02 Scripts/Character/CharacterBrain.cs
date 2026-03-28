using UnityEngine;
using System.Collections.Generic;

public class CharacterBrain : MonoBehaviour
{
    public SmallTurnActionType DecideSmallTurnAction(CharacterData data, BigTurnSelectionData selection)
    {
        Dictionary<SmallTurnActionType, int> weights = new Dictionary<SmallTurnActionType, int>
        {
            { SmallTurnActionType.Idle, 10 },
            { SmallTurnActionType.Wander, 20 },
            { SmallTurnActionType.Gather, 20 },
            { SmallTurnActionType.Craft, 15 },
            { SmallTurnActionType.Build, 15 },
            { SmallTurnActionType.Social, 10 },
            { SmallTurnActionType.Rest, 5 },
            { SmallTurnActionType.Eat, 5 }
        };

        if (selection.Policy == PolicyType.GatherFirst) weights[SmallTurnActionType.Gather] += 20;
        if (selection.Policy == PolicyType.CraftFirst) weights[SmallTurnActionType.Craft] += 20;
        if (selection.Policy == PolicyType.BuildFirst) weights[SmallTurnActionType.Build] += 20;
        if (selection.Policy == PolicyType.SocialFirst) weights[SmallTurnActionType.Social] += 20;
        if (selection.Policy == PolicyType.CombatFirst) weights[SmallTurnActionType.Wander] += 15;

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
}
