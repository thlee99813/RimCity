using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherResourceGoapAction : GoapAction
{
    private const int GatherYield = 3;
    private readonly ResourceType _resourceType;

    public override string Name => $"Gather:{_resourceType}";

    public GatherResourceGoapAction(ResourceType resourceType)
    {
        _resourceType = resourceType;
    }

    public override bool ArePreconditionsMet(GoapWorldState state, GoapContext context)
    {
        return state.GetGatherableAmount(_resourceType) > 0;
    }

    public override GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context)
    {
        state.ConsumeGatherable(_resourceType);
        state.AddResource(_resourceType, GatherYield);
        return state;
    }

    public override int GetCost(GoapWorldState state, GoapContext context)
    {
        TileNode target = CharacterTaskCommon.FindNearestReachableResourceTile(
            context.Owner.CurrentTileNode, context.ActiveNodes, _resourceType);
        int moveCost = target == null ? 1 : context.GetMoveTurnCost(target);
        return Mathf.Max(1, moveCost + 1);
    }

    public override bool CanStart(GoapContext context)
    {
        return CharacterTaskCommon.FindNearestReachableResourceTile(
            context.Owner.CurrentTileNode, context.ActiveNodes, _resourceType) != null;
    }

    public override bool CanContinue(GoapContext context)
    {
        return context.TaskController.GatherState == CharacterTaskState.Running;
    }

    public override IEnumerator ExecuteTurn(GoapContext context)
    {
        yield return context.TaskController.RunGatherTurn(
            context.Owner, context.SmallTurn, context.ActiveNodes, context.Log, _resourceType);
        Status = ToGoapStatus(context.TaskController.GatherState);
    }

    private GoapActionStatus ToGoapStatus(CharacterTaskState state)
    {
        if (state == CharacterTaskState.Running) return GoapActionStatus.Running;
        if (state == CharacterTaskState.Success) return GoapActionStatus.Success;
        return GoapActionStatus.Failure;
    }
}

public class CraftItemGoapAction : GoapAction
{
    private readonly CraftRecipe _recipe;

    public override string Name => $"Craft:{_recipe.Id}";

    public CraftItemGoapAction(CraftRecipe recipe)
    {
        _recipe = recipe;
    }

    public override bool ArePreconditionsMet(GoapWorldState state, GoapContext context)
    {
        return context.Owner.GetStatLevel(StatType.Craft) >= Mathf.Max(1, _recipe.RequiredCraftLevel) &&
               GoapResourceCost.CanAfford(state, _recipe.Costs);
    }

    public override GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context)
    {
        GoapResourceCost.Consume(ref state, _recipe.Costs);

        if (_recipe.Id == ItemIds.Cloth)
        {
            if (state.Armor == ArmorType.None) state.Armor = ArmorType.Cloth;
            else state.Cloth++;
        }
        else if (_recipe.Id == ItemIds.WoodenSpear)
        {
            ReturnEquippedWeapon(ref state);
            state.Weapon = WeaponType.WoodenSpear;
        }
        else if (_recipe.Id == ItemIds.StoneSpear)
        {
            ReturnEquippedWeapon(ref state);
            state.Weapon = WeaponType.StoneSpear;
        }
        else if (_recipe.Id == ItemIds.Fan)
        {
            if (state.Utility == UtilityType.None) state.Utility = UtilityType.Fan;
            else state.Fan++;
        }
        else if (_recipe.Id == ItemIds.Bandage)
        {
            state.Bandage++;
        }
        else if (_recipe.Id == ItemIds.Medkit)
        {
            state.Medkit++;
        }

        state.ColdProtected = state.ColdProtected || state.Armor == ArmorType.Cloth;
        state.HeatProtected = state.HeatProtected || state.Utility == UtilityType.Fan;
        return state;
    }

    public override int GetCost(GoapWorldState state, GoapContext context)
    {
        int failPenalty = GetFailurePenalty(
            context.Owner.GetStatLevel(StatType.Craft), _recipe.RecommendedCraftLevel);
        return Mathf.Max(1, _recipe.CraftTurns + failPenalty);
    }

    public override bool CanStart(GoapContext context)
    {
        return context.Owner.GetStatLevel(StatType.Craft) >= Mathf.Max(1, _recipe.RequiredCraftLevel) &&
               CharacterTaskCommon.CanAfford(context.Inventory, _recipe.Costs);
    }

    public override bool CanContinue(GoapContext context)
    {
        return context.TaskController.CraftState == CharacterTaskState.Running;
    }

    public override IEnumerator ExecuteTurn(GoapContext context)
    {
        yield return context.TaskController.RunCraftTurn(
            context.Owner, context.SmallTurn, context.Log, _recipe.Id);
        Status = ToGoapStatus(context.TaskController.CraftState);
    }

    private void ReturnEquippedWeapon(ref GoapWorldState state)
    {
        if (state.Weapon == WeaponType.WoodenSpear) state.WoodenSpear++;
        if (state.Weapon == WeaponType.StoneSpear) state.StoneSpear++;
    }

    private int GetFailurePenalty(int currentLevel, int recommendedLevel)
    {
        int gap = Mathf.Max(1, recommendedLevel) - Mathf.Clamp(currentLevel, 1, 11);
        if (gap >= 2) return 3;
        if (gap == 1) return 1;
        return 0;
    }

    private GoapActionStatus ToGoapStatus(CharacterTaskState state)
    {
        if (state == CharacterTaskState.Running) return GoapActionStatus.Running;
        if (state == CharacterTaskState.Success) return GoapActionStatus.Success;
        return GoapActionStatus.Failure;
    }
}

public class BuildStructureGoapAction : GoapAction
{
    private readonly BuildRecipe _recipe;

    public override string Name => $"Build:{_recipe.Id}";

    public BuildStructureGoapAction(BuildRecipe recipe)
    {
        _recipe = recipe;
    }

    public override bool ArePreconditionsMet(GoapWorldState state, GoapContext context)
    {
        return _recipe.Prefab != null && state.BuildableTileCount > 0 &&
               context.Owner.GetStatLevel(StatType.Build) >= Mathf.Max(1, _recipe.RequiredBuildLevel) &&
               GoapResourceCost.CanAfford(state, _recipe.Costs);
    }

    public override GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context)
    {
        GoapResourceCost.Consume(ref state, _recipe.Costs);
        state.BuildableTileCount--;

        if (_recipe.Id == "torch")
        {
            state.TorchCount++;
            state.ReachableTorch = true;
            state.ColdProtected = true;
        }
        else if (_recipe.Id == "campfire")
        {
            state.CampfireCount++;
            state.ReachableCampfire = true;
            state.ColdProtected = true;
        }
        else if (_recipe.Id == "bed")
        {
            state.BedCount++;
            state.ReachableBed = true;
            state.AtBed = true;
        }
        else if (_recipe.Id == "coldStone")
        {
            state.SweatingStoneCount++;
            state.ReachableSweatingStone = true;
            state.HeatProtected = true;
        }

        return state;
    }

    public override int GetCost(GoapWorldState state, GoapContext context)
    {
        int failPenalty = GetFailurePenalty(
            context.Owner.GetStatLevel(StatType.Build), _recipe.RecommendedBuildLevel);
        return Mathf.Max(1, _recipe.BuildTurns + 1 + failPenalty);
    }

    public override bool CanStart(GoapContext context)
    {
        return _recipe.Prefab != null && HasBuildableTile(context.ActiveNodes) &&
               context.Owner.GetStatLevel(StatType.Build) >= Mathf.Max(1, _recipe.RequiredBuildLevel) &&
               CharacterTaskCommon.CanAfford(context.Inventory, _recipe.Costs);
    }

    public override bool CanContinue(GoapContext context)
    {
        return context.TaskController.BuildState == CharacterTaskState.Running;
    }

    public override IEnumerator ExecuteTurn(GoapContext context)
    {
        yield return context.TaskController.RunBuildTurn(
            context.Owner, context.SmallTurn, context.ActiveNodes, context.Log, _recipe.Id);
        Status = ToGoapStatus(context.TaskController.BuildState);
    }

    private bool HasBuildableTile(List<TileNode> activeNodes)
    {
        for (int i = 0; i < activeNodes.Count; i++)
        {
            TileNode tile = activeNodes[i];
            if (tile != null && !tile.IsOccupied && !tile.HasResource) return true;
        }

        return false;
    }

    private int GetFailurePenalty(int currentLevel, int recommendedLevel)
    {
        int gap = Mathf.Max(1, recommendedLevel) - Mathf.Clamp(currentLevel, 1, 11);
        if (gap >= 2) return 3;
        if (gap == 1) return 1;
        return 0;
    }

    private GoapActionStatus ToGoapStatus(CharacterTaskState state)
    {
        if (state == CharacterTaskState.Running) return GoapActionStatus.Running;
        if (state == CharacterTaskState.Success) return GoapActionStatus.Success;
        return GoapActionStatus.Failure;
    }
}

public class MoveToStructureGoapAction : GoapAction
{
    private readonly StructureType _primary;
    private readonly StructureType _secondary;
    private readonly int _reachableTurns;
    private TileNode _target;

    public override string Name => $"MoveTo:{_primary}";

    public MoveToStructureGoapAction(
        StructureType primary,
        int reachableTurns,
        StructureType secondary = StructureType.None)
    {
        _primary = primary;
        _secondary = secondary;
        _reachableTurns = reachableTurns;
    }

    public override bool ArePreconditionsMet(GoapWorldState state, GoapContext context)
    {
        if (_primary == StructureType.Bed) return state.ReachableBed && !state.AtBed;
        if (_primary == StructureType.SweatingStone) return state.ReachableSweatingStone && !state.HeatProtected;
        return (state.ReachableTorch || state.ReachableCampfire) && !state.ColdProtected;
    }

    public override GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context)
    {
        if (_primary == StructureType.Bed) state.AtBed = true;
        else if (_primary == StructureType.SweatingStone) state.HeatProtected = true;
        else state.ColdProtected = true;
        return state;
    }

    public override int GetCost(GoapWorldState state, GoapContext context)
    {
        TileNode target = FindTarget(context);
        return target == null ? 1 : Mathf.Max(1, context.GetMoveTurnCost(target));
    }

    public override bool CanStart(GoapContext context)
    {
        _target = FindTarget(context);
        return _target != null;
    }

    public override bool CanContinue(GoapContext context)
    {
        return IsTargetValid(context);
    }

    public override IEnumerator ExecuteTurn(GoapContext context)
    {
        if (_target == null) _target = FindTarget(context);
        if (!IsTargetValid(context))
        {
            Status = GoapActionStatus.Failure;
            yield break;
        }

        yield return context.TaskController.RunMoveToTileTurn(
            context.Owner, _target, context.SmallTurn, context.ActiveNodes, context.Log, "보호 구조물 쪽으로");

        Status = context.Owner.CurrentTileNode == _target
            ? GoapActionStatus.Success
            : GoapActionStatus.Running;
    }

    public override void ResetRuntimeState()
    {
        base.ResetRuntimeState();
        _target = null;
    }

    private TileNode FindTarget(GoapContext context)
    {
        return CharacterTaskCommon.FindNearestReachableStructureTile(
            context.Owner.CurrentTileNode,
            context.ActiveNodes,
            context.MaxMoveTilesPerTurn * _reachableTurns,
            _primary,
            _secondary);
    }

    private bool IsTargetValid(GoapContext context)
    {
        if (_target == null || !context.ActiveNodes.Contains(_target)) return false;
        if (!CharacterTaskCommon.TryGetPlacedStructureType(_target, out StructureType type)) return false;
        return type == _primary || type == _secondary;
    }
}

public class RestGoapAction : GoapAction
{
    public override string Name => "Rest";

    public override bool ArePreconditionsMet(GoapWorldState state, GoapContext context)
    {
        return state.Sleep < Mathf.RoundToInt(context.Owner.Data.MaxSleep);
    }

    public override GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context)
    {
        int recovery = state.AtBed ? 100 : state.Sleep <= 0 ? 60 : 20;
        state.Sleep = Mathf.Min(Mathf.RoundToInt(context.Owner.Data.MaxSleep), state.Sleep + recovery);
        return state;
    }

    public override int GetCost(GoapWorldState state, GoapContext context)
    {
        if (state.AtBed) return 1;
        if (state.Sleep <= 0) return 3;
        return 2;
    }

    public override bool CanStart(GoapContext context)
    {
        return context.Owner.Status.Sleep < context.Owner.Data.MaxSleep;
    }

    public override bool CanContinue(GoapContext context)
    {
        return context.TaskController.IsForcedRestRunning;
    }

    public override IEnumerator ExecuteTurn(GoapContext context)
    {
        bool atBed = CharacterTaskCommon.TryGetPlacedStructureType(
            context.Owner.CurrentTileNode, out StructureType type) && type == StructureType.Bed;

        if (!atBed) context.TaskController.PrepareForcedRest(context.Owner);
        context.TaskController.RunRestTurn(context.Owner, context.SmallTurn, context.Log);
        Status = context.TaskController.IsForcedRestRunning
            ? GoapActionStatus.Running
            : GoapActionStatus.Success;
        yield break;
    }
}

public class EatBerryGoapAction : GoapAction
{
    public override string Name => "EatBerry";

    public override bool ArePreconditionsMet(GoapWorldState state, GoapContext context)
    {
        return state.Berry > 0 && state.Hunger < Mathf.RoundToInt(context.Owner.Data.MaxHunger);
    }

    public override GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context)
    {
        state.Berry--;
        state.Hunger = Mathf.Min(
            Mathf.RoundToInt(context.Owner.Data.MaxHunger),
            state.Hunger + Mathf.RoundToInt(context.BerryHungerRecoverAmount));
        return state;
    }

    public override int GetCost(GoapWorldState state, GoapContext context) => 1;
    public override bool CanStart(GoapContext context) => context.Inventory.Berry > 0;
    public override bool CanContinue(GoapContext context) => false;

    public override IEnumerator ExecuteTurn(GoapContext context)
    {
        bool success = context.TaskController.RunEatAction(
            context.Owner, context.SmallTurn, context.Log, context.BerryHungerRecoverAmount);
        Status = success ? GoapActionStatus.Success : GoapActionStatus.Failure;
        yield break;
    }
}

public class UseHealingItemGoapAction : GoapAction
{
    private readonly string _itemId;

    public override string Name => $"Use:{_itemId}";

    public UseHealingItemGoapAction(string itemId)
    {
        _itemId = itemId;
    }

    public override bool ArePreconditionsMet(GoapWorldState state, GoapContext context)
    {
        int amount = _itemId == ItemIds.Medkit ? state.Medkit : state.Bandage;
        return amount > 0 && state.Health < Mathf.RoundToInt(context.Owner.Data.MaxHealth);
    }

    public override GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context)
    {
        int recovery;
        if (_itemId == ItemIds.Medkit)
        {
            state.Medkit--;
            recovery = 50;
        }
        else
        {
            state.Bandage--;
            recovery = 20;
        }

        state.Health = Mathf.Min(Mathf.RoundToInt(context.Owner.Data.MaxHealth), state.Health + recovery);
        return state;
    }

    public override int GetCost(GoapWorldState state, GoapContext context) => 1;

    public override bool CanStart(GoapContext context)
    {
        return _itemId == ItemIds.Medkit ? context.Inventory.Medkit > 0 : context.Inventory.Bandage > 0;
    }

    public override bool CanContinue(GoapContext context) => false;

    public override IEnumerator ExecuteTurn(GoapContext context)
    {
        bool success = _itemId == ItemIds.Medkit
            ? context.TaskController.RunUseMedkitTurn(context.Owner, context.SmallTurn, context.Log)
            : context.TaskController.RunUseBandageTurn(context.Owner, context.SmallTurn, context.Log);
        Status = success ? GoapActionStatus.Success : GoapActionStatus.Failure;
        yield break;
    }
}

public class EquipItemGoapAction : GoapAction
{
    private readonly string _itemId;

    public override string Name => $"Equip:{_itemId}";

    public EquipItemGoapAction(string itemId)
    {
        _itemId = itemId;
    }

    public override bool ArePreconditionsMet(GoapWorldState state, GoapContext context)
    {
        if (_itemId == ItemIds.WoodenSpear)
            return state.WoodenSpear > 0 && state.Weapon != WeaponType.WoodenSpear;
        if (_itemId == ItemIds.StoneSpear)
            return state.StoneSpear > 0 && state.Weapon != WeaponType.StoneSpear;
        if (_itemId == ItemIds.Fan)
            return state.Fan > 0 && state.Utility != UtilityType.Fan;
        return false;
    }

    public override GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context)
    {
        if (_itemId == ItemIds.WoodenSpear)
        {
            state.WoodenSpear--;
            ReturnWeapon(ref state);
            state.Weapon = WeaponType.WoodenSpear;
        }
        else if (_itemId == ItemIds.StoneSpear)
        {
            state.StoneSpear--;
            ReturnWeapon(ref state);
            state.Weapon = WeaponType.StoneSpear;
        }
        else if (_itemId == ItemIds.Fan)
        {
            state.Fan--;
            state.Utility = UtilityType.Fan;
            state.HeatProtected = true;
        }

        return state;
    }

    public override int GetCost(GoapWorldState state, GoapContext context) => 1;

    public override bool CanStart(GoapContext context)
    {
        if (_itemId == ItemIds.WoodenSpear)
            return context.Inventory.WoodenSpear > 0 && context.Owner.Equipment.Weapon != WeaponType.WoodenSpear;
        if (_itemId == ItemIds.StoneSpear)
            return context.Inventory.StoneSpear > 0 && context.Owner.Equipment.Weapon != WeaponType.StoneSpear;
        if (_itemId == ItemIds.Fan)
            return context.Inventory.Fan > 0 && context.Owner.Equipment.Utility != UtilityType.Fan;
        return false;
    }

    public override bool CanContinue(GoapContext context) => false;

    public override IEnumerator ExecuteTurn(GoapContext context)
    {
        bool success = false;
        if (_itemId == ItemIds.WoodenSpear)
            success = context.TaskController.RunEquipWoodenSpearTurn(context.Owner, context.SmallTurn, context.Log);
        else if (_itemId == ItemIds.StoneSpear)
            success = context.TaskController.RunEquipStoneSpearTurn(context.Owner, context.SmallTurn, context.Log);
        else if (_itemId == ItemIds.Fan)
            success = context.TaskController.RunEquipFanTurn(context.Owner, context.SmallTurn, context.Log);

        Status = success ? GoapActionStatus.Success : GoapActionStatus.Failure;
        yield break;
    }

    private void ReturnWeapon(ref GoapWorldState state)
    {
        if (state.Weapon == WeaponType.WoodenSpear) state.WoodenSpear++;
        if (state.Weapon == WeaponType.StoneSpear) state.StoneSpear++;
    }
}

public class SocializeGoapAction : GoapAction
{
    public override string Name => "Socialize";
    public override bool ArePreconditionsMet(GoapWorldState state, GoapContext context) => !state.Socialized;

    public override GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context)
    {
        state.Socialized = true;
        return state;
    }

    public override int GetCost(GoapWorldState state, GoapContext context) => 2;
    public override bool CanStart(GoapContext context) => true;
    public override bool CanContinue(GoapContext context) => context.TaskController.SocialState == CharacterTaskState.Running;

    public override IEnumerator ExecuteTurn(GoapContext context)
    {
        yield return context.TaskController.RunSocialTurn(
            context.Owner, context.SmallTurn, context.ActiveNodes, context.Log);

        CharacterTaskState state = context.TaskController.SocialState;
        Status = state == CharacterTaskState.Running
            ? GoapActionStatus.Running
            : state == CharacterTaskState.Success ? GoapActionStatus.Success : GoapActionStatus.Failure;
    }
}

public class WanderGoapAction : GoapAction
{
    public override string Name => "Wander";
    public override bool ArePreconditionsMet(GoapWorldState state, GoapContext context) => !state.Explored;

    public override GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context)
    {
        state.Explored = true;
        return state;
    }

    public override int GetCost(GoapWorldState state, GoapContext context) => 1;
    public override bool CanStart(GoapContext context) => context.Owner.CurrentTileNode != null;
    public override bool CanContinue(GoapContext context) => false;

    public override IEnumerator ExecuteTurn(GoapContext context)
    {
        context.Log.AddLog(TextUtil.ApplyKoreanParticles(
            $"[{context.SmallTurn} 턴] {context.Owner.Data.Name}은/는 주변을 돌아다닙니다."));

        TileNode nextNode = GetRandomNeighbor(context);
        if (nextNode != null && nextNode != context.Owner.CurrentTileNode)
            yield return context.Owner.MoveToTile(nextNode);

        Status = GoapActionStatus.Success;
    }

    private TileNode GetRandomNeighbor(GoapContext context)
    {
        TileNode current = context.Owner.CurrentTileNode;
        List<TileNode> valid = new List<TileNode>();

        for (int i = 0; i < current.Neighbors.Count; i++)
        {
            TileNode neighbor = current.Neighbors[i];
            if (context.ActiveNodes.Contains(neighbor)) valid.Add(neighbor);
        }

        if (valid.Count == 0) return current;
        return valid[Random.Range(0, valid.Count)];
    }
}

public class IdleGoapAction : GoapAction
{
    public override string Name => "Idle";
    public override bool ArePreconditionsMet(GoapWorldState state, GoapContext context) => !state.Idled;

    public override GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context)
    {
        state.Idled = true;
        return state;
    }

    public override int GetCost(GoapWorldState state, GoapContext context) => 1;
    public override bool CanStart(GoapContext context) => true;
    public override bool CanContinue(GoapContext context) => false;

    public override IEnumerator ExecuteTurn(GoapContext context)
    {
        context.Log.AddLog(TextUtil.ApplyKoreanParticles(
            $"[{context.SmallTurn} 턴] {context.Owner.Data.Name}은/는 아무것도 하지 않습니다."));
        Status = GoapActionStatus.Success;
        yield break;
    }
}

public static class GoapResourceCost
{
    public static bool CanAfford(GoapWorldState state, ResourceCost[] costs)
    {
        if (costs == null) return true;

        for (int i = 0; i < costs.Length; i++)
        {
            if (state.GetResourceAmount(costs[i].Type) < costs[i].Amount) return false;
        }

        return true;
    }

    public static void Consume(ref GoapWorldState state, ResourceCost[] costs)
    {
        if (costs == null) return;

        for (int i = 0; i < costs.Length; i++)
            state.AddResource(costs[i].Type, -costs[i].Amount);
    }
}

public class CharacterGoapActionSet
{
    public List<IGoapAction> Create(GoapContext context, GoapGoal goal)
    {
        List<IGoapAction> actions = new List<IGoapAction>();

        switch (goal.Type)
        {
            case CharacterGoalType.RecoverSleep:
                actions.Add(new MoveToStructureGoapAction(StructureType.Bed, 2));
                actions.Add(new RestGoapAction());
                AddBuildAction(actions, context, "bed");
                AddGatherAction(actions, ResourceType.Tree);
                AddGatherAction(actions, ResourceType.Rock);
                break;
            case CharacterGoalType.SatisfyHunger:
                actions.Add(new EatBerryGoapAction());
                AddGatherAction(actions, ResourceType.Berry);
                break;
            case CharacterGoalType.RecoverHealth:
                actions.Add(new UseHealingItemGoapAction(ItemIds.Medkit));
                actions.Add(new UseHealingItemGoapAction(ItemIds.Bandage));
                AddCraftAction(actions, context, ItemIds.Medkit);
                AddCraftAction(actions, context, ItemIds.Bandage);
                AddGatherAction(actions, ResourceType.Grass);
                AddGatherAction(actions, ResourceType.Rock);
                break;
            case CharacterGoalType.ProtectFromHeat:
                actions.Add(new MoveToStructureGoapAction(StructureType.SweatingStone, 1));
                actions.Add(new EquipItemGoapAction(ItemIds.Fan));
                AddCraftAction(actions, context, ItemIds.Fan);
                AddBuildAction(actions, context, "coldStone");
                AddGatherAction(actions, ResourceType.Grass);
                AddGatherAction(actions, ResourceType.Rock);
                break;
            case CharacterGoalType.ProtectFromCold:
                actions.Add(new MoveToStructureGoapAction(StructureType.Campfire, 1, StructureType.Torch));
                AddCraftAction(actions, context, ItemIds.Cloth);
                AddBuildAction(actions, context, "torch");
                AddBuildAction(actions, context, "campfire");
                AddGatherAction(actions, ResourceType.Tree);
                AddGatherAction(actions, ResourceType.Rock);
                AddGatherAction(actions, ResourceType.Grass);
                break;
            case CharacterGoalType.EquipItem:
                actions.Add(new EquipItemGoapAction(goal.TargetId));
                break;
            case CharacterGoalType.GatherResource:
                AddGatherAction(actions, goal.TargetResource);
                break;
            case CharacterGoalType.CraftItem:
                AddCraftAction(actions, context, goal.TargetId);
                AddRecipeGatherActions(actions, context.FindCraftRecipe(goal.TargetId)?.Costs);
                break;
            case CharacterGoalType.BuildStructure:
                AddBuildAction(actions, context, goal.TargetId);
                AddRecipeGatherActions(actions, context.FindBuildRecipe(goal.TargetId)?.Costs);
                break;
            case CharacterGoalType.Socialize:
                actions.Add(new SocializeGoapAction());
                break;
            case CharacterGoalType.Explore:
                actions.Add(new WanderGoapAction());
                break;
            case CharacterGoalType.Idle:
                actions.Add(new IdleGoapAction());
                break;
        }

        return actions;
    }

    private void AddGatherAction(List<IGoapAction> actions, ResourceType type)
    {
        if (type == ResourceType.None) return;

        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].Name == $"Gather:{type}") return;
        }

        actions.Add(new GatherResourceGoapAction(type));
    }

    private void AddRecipeGatherActions(List<IGoapAction> actions, ResourceCost[] costs)
    {
        if (costs == null) return;
        for (int i = 0; i < costs.Length; i++) AddGatherAction(actions, costs[i].Type);
    }

    private void AddCraftAction(List<IGoapAction> actions, GoapContext context, string recipeId)
    {
        CraftRecipe recipe = context.FindCraftRecipe(recipeId);
        if (recipe != null) actions.Add(new CraftItemGoapAction(recipe));
    }

    private void AddBuildAction(List<IGoapAction> actions, GoapContext context, string recipeId)
    {
        BuildRecipe recipe = context.FindBuildRecipe(recipeId);
        if (recipe != null) actions.Add(new BuildStructureGoapAction(recipe));
    }
}
