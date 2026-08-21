using System.Collections.Generic;
using UnityEngine;

public class CharacterGoalSelector
{
    private const float HungerThreshold = 35f;
    private const float SleepThreshold = 20f;
    private const float UrgentUtilityThreshold = 100f;

    private readonly CharacterBrain _brain;

    public CharacterGoalSelector(CharacterBrain brain)
    {
        _brain = brain;
    }

    public List<GoapGoal> GetPlanningOrder(GoapWorldState state, GoapContext context)
    {
        List<GoapGoal> candidates = CreateCandidates(state, context);
        List<GoapGoal> urgent = new List<GoapGoal>();
        List<GoapGoal> routine = new List<GoapGoal>();

        for (int i = 0; i < candidates.Count; i++)
        {
            GoapGoal goal = candidates[i];
            goal.Utility = _brain.ApplyCharacterPreferences(goal, context.Owner.Data, context.Selection);

            if (goal.Utility >= UrgentUtilityThreshold) urgent.Add(goal);
            else routine.Add(goal);
        }

        urgent.Sort((left, right) => right.Utility.CompareTo(left.Utility));
        List<GoapGoal> ordered = new List<GoapGoal>(urgent);

        while (routine.Count > 0)
        {
            int pickedIndex = WeightedPickIndex(routine);
            ordered.Add(routine[pickedIndex]);
            routine.RemoveAt(pickedIndex);
        }

        return ordered;
    }

    private List<GoapGoal> CreateCandidates(GoapWorldState state, GoapContext context)
    {
        List<GoapGoal> goals = new List<GoapGoal>();
        CharacterEntity owner = context.Owner;

        AddSurvivalGoals(goals, state, context);
        AddEquipmentGoals(goals, state);
        AddRoutineWorkGoals(goals, state, context);

        if (state.Sleep >= SleepThreshold && state.Sleep < Mathf.RoundToInt(owner.Data.MaxSleep))
            goals.Add(new GoapGoal(CharacterGoalType.RecoverSleep, 5f, Mathf.Min(state.Sleep + 20, Mathf.RoundToInt(owner.Data.MaxSleep))));

        goals.Add(new GoapGoal(CharacterGoalType.Socialize, 10f));
        goals.Add(new GoapGoal(CharacterGoalType.Explore, 10f));
        goals.Add(new GoapGoal(CharacterGoalType.Idle, 10f));
        return goals;
    }

    private void AddSurvivalGoals(List<GoapGoal> goals, GoapWorldState state, GoapContext context)
    {
        CharacterEntity owner = context.Owner;

        if (state.Sleep <= 0)
        {
            goals.Add(new GoapGoal(CharacterGoalType.RecoverSleep, 1000f, Mathf.Min(60, Mathf.RoundToInt(owner.Data.MaxSleep))));
        }
        else if (state.Sleep < SleepThreshold)
        {
            goals.Add(new GoapGoal(CharacterGoalType.RecoverSleep, 320f, Mathf.Min(40, Mathf.RoundToInt(owner.Data.MaxSleep))));
        }

        if (state.Hunger <= HungerThreshold)
        {
            int desiredHunger = Mathf.Min(
                Mathf.RoundToInt(owner.Data.MaxHunger),
                Mathf.Max(36, state.Hunger + Mathf.RoundToInt(context.BerryHungerRecoverAmount)));
            goals.Add(new GoapGoal(CharacterGoalType.SatisfyHunger, 300f, desiredHunger));
        }

        float healthRatio = owner.Status.Health / owner.Data.MaxHealth;
        if (healthRatio <= 0.3f)
        {
            int desiredHealth = Mathf.Min(
                Mathf.RoundToInt(owner.Data.MaxHealth), state.Health + 50);
            goals.Add(new GoapGoal(CharacterGoalType.RecoverHealth, 520f, desiredHealth));
        }
        else if (healthRatio <= 0.5f)
        {
            int desiredHealth = Mathf.Min(
                Mathf.RoundToInt(owner.Data.MaxHealth), state.Health + 20);
            goals.Add(new GoapGoal(CharacterGoalType.RecoverHealth, 360f, desiredHealth));
        }

        if (context.Selection.Weather == WeatherType.Heatwave && !state.HeatProtected)
            goals.Add(new GoapGoal(CharacterGoalType.ProtectFromHeat, 180f));

        if (context.Selection.Weather == WeatherType.ExtremeCold && !state.ColdProtected)
            goals.Add(new GoapGoal(CharacterGoalType.ProtectFromCold, 180f));
    }

    private void AddEquipmentGoals(List<GoapGoal> goals, GoapWorldState state)
    {
        if (state.StoneSpear > 0 && state.Weapon != WeaponType.StoneSpear)
            goals.Add(new GoapGoal(CharacterGoalType.EquipItem, 64f, targetId: ItemIds.StoneSpear));

        if (state.WoodenSpear > 0 && state.Weapon != WeaponType.WoodenSpear)
            goals.Add(new GoapGoal(CharacterGoalType.EquipItem, 48f, targetId: ItemIds.WoodenSpear));

        if (state.Fan > 0 && state.Utility != UtilityType.Fan)
            goals.Add(new GoapGoal(CharacterGoalType.EquipItem, 40f, targetId: ItemIds.Fan));
    }

    private void AddRoutineWorkGoals(List<GoapGoal> goals, GoapWorldState state, GoapContext context)
    {
        AddGatherGoal(goals, state, ResourceType.Berry);
        AddGatherGoal(goals, state, ResourceType.Tree);
        AddGatherGoal(goals, state, ResourceType.Grass);
        AddGatherGoal(goals, state, ResourceType.Rock);

        if (context.CraftRecipes != null)
        {
            for (int i = 0; i < context.CraftRecipes.Length; i++)
            {
                CraftRecipe recipe = context.CraftRecipes[i];
                if (recipe == null) continue;

                float utility = 15f;
                if (recipe.Id == ItemIds.Bandage) utility += state.Bandage <= 0 ? 7f : 2f;
                if (recipe.Id == ItemIds.Medkit) utility += state.Medkit <= 0 ? 9f : 2f;

                goals.Add(new GoapGoal(
                    CharacterGoalType.CraftItem,
                    utility,
                    GetItemAmount(state, recipe.Id) + 1,
                    recipe.Id));
            }
        }

        if (context.BuildRecipes != null)
        {
            for (int i = 0; i < context.BuildRecipes.Length; i++)
            {
                BuildRecipe recipe = context.BuildRecipes[i];
                if (recipe == null || recipe.Prefab == null) continue;

                goals.Add(new GoapGoal(
                    CharacterGoalType.BuildStructure,
                    15f,
                    GetStructureCount(state, recipe.Id) + 1,
                    recipe.Id));
            }
        }
    }

    private void AddGatherGoal(List<GoapGoal> goals, GoapWorldState state, ResourceType type)
    {
        if (state.GetGatherableAmount(type) <= 0) return;

        goals.Add(new GoapGoal(
            CharacterGoalType.GatherResource,
            20f + Random.Range(0f, 3f),
            state.GetResourceAmount(type) + 3,
            targetResource: type));
    }

    private int WeightedPickIndex(List<GoapGoal> goals)
    {
        float total = 0f;
        for (int i = 0; i < goals.Count; i++) total += Mathf.Max(0.1f, goals[i].Utility);

        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < goals.Count; i++)
        {
            cumulative += Mathf.Max(0.1f, goals[i].Utility);
            if (roll <= cumulative) return i;
        }

        return goals.Count - 1;
    }

    private int GetItemAmount(GoapWorldState state, string itemId)
    {
        if (itemId == ItemIds.Cloth) return state.Cloth;
        if (itemId == ItemIds.WoodenSpear) return state.WoodenSpear;
        if (itemId == ItemIds.StoneSpear) return state.StoneSpear;
        if (itemId == ItemIds.Fan) return state.Fan;
        if (itemId == ItemIds.Bandage) return state.Bandage;
        if (itemId == ItemIds.Medkit) return state.Medkit;
        return 0;
    }

    private int GetStructureCount(GoapWorldState state, string recipeId)
    {
        if (recipeId == "torch") return state.TorchCount;
        if (recipeId == "campfire") return state.CampfireCount;
        if (recipeId == "bed") return state.BedCount;
        if (recipeId == "coldStone") return state.SweatingStoneCount;
        return 0;
    }
}
