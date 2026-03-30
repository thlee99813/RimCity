using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTaskController
{
    private readonly int _maxMoveTilesPerTurn;
    private readonly BuildRecipe[] _buildRecipes;
    private readonly CraftRecipe[] _craftRecipes;

    private readonly CharacterGatherTask _gatherTask = new CharacterGatherTask();
    private readonly CharacterBuildTask _buildTask = new CharacterBuildTask();
    private readonly CharacterCraftTask _craftTask = new CharacterCraftTask();
    private readonly CharacterItemUseTask _itemUseTask = new CharacterItemUseTask();
    private readonly CharacterSocialTask _socialTask = new CharacterSocialTask();


    private ResourceType _nextGatherType = ResourceType.None;
    private string _nextBuildRecipeId;
    private string _nextCraftRecipeId;
    private TileNode _moveTargetTile;

    private int _forcedRestTurnsRemaining;

    private ResourceType _missingFocusType = ResourceType.None;
    private int _missingFocusTurnsRemaining;

    private const float HungerEatThreshold = 35f;
    private const float SleepLowThreshold = 20f;

    private const int SleepBedBuildChance = 20;      
    private const int HungerBerryGatherChance = 20;  
    private const int WeatherPrepChance = 5;        
    private const int MissingFocusGatherChance = 45; 

    private const int ForcedSleepRestTurns = 3;
    private const float ForcedSleepRecoverAmount = 60f;

    private const int BedReachableTurns = 2;


    public CharacterTaskController(int maxMoveTilesPerTurn, BuildRecipe[] buildRecipes, CraftRecipe[] craftRecipes)
    {
        _maxMoveTilesPerTurn = Mathf.Max(1, maxMoveTilesPerTurn);
        _buildRecipes = buildRecipes;
        _craftRecipes = craftRecipes;
    }

    public SmallTurnActionType ResolveAction(
        CharacterEntity owner,
        CharacterData data,
        CharacterStatus status,
        CharacterEquipment equipment,
        BigTurnSelectionData selection,
        List<TileNode> activeNodes,
        CharacterBrain brain)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
        TileNode current = owner.CurrentTileNode;

        if (_forcedRestTurnsRemaining > 0) return SmallTurnActionType.Rest;
        if (status.Sleep <= 0f)
        {
            _forcedRestTurnsRemaining = ForcedSleepRestTurns;
            return SmallTurnActionType.Rest;
        }

        if (_gatherTask.IsForced) return SmallTurnActionType.Gather;
        if (_buildTask.IsForced) return SmallTurnActionType.Build;
        if (_craftTask.IsForced) return SmallTurnActionType.Craft;
        if (_socialTask.IsForced) return SmallTurnActionType.Social;


        if (status.Sleep < SleepLowThreshold)
        {
            // 2턴(8타일) 내 침대 우선
            TileNode nearBedTile = CharacterTaskCommon.FindNearestReachableStructureTile(
                current, activeNodes, _maxMoveTilesPerTurn * BedReachableTurns, StructureType.Bed);

            if (nearBedTile != null)
            {
                _moveTargetTile = nearBedTile;

                if (current == nearBedTile)
                    return SmallTurnActionType.Rest;

                return SmallTurnActionType.MoveToShelter;
            }

            // 침대가 멀거나 없으면 무작위 건설 시도
            if (Random.Range(0, 100) < SleepBedBuildChance)
            {
                ResourceType bedMissing = ResourceType.None;
                if (TryPrepareBuild("bed", inv, out bedMissing))
                    return SmallTurnActionType.Build;

                if (TrySetGatherFocusForMissing(bedMissing, current, activeNodes))
                    return SmallTurnActionType.Gather;
            }

            return SmallTurnActionType.Rest;
        }

        if (status.Hunger <= HungerEatThreshold)
        {
            if (inv.Berry > 0) return SmallTurnActionType.Eat;

            if (Random.Range(0, 100) < HungerBerryGatherChance)
            {
                if (TrySetGatherFocus(ResourceType.Berry, current, activeNodes))
                    return SmallTurnActionType.Gather;
            }
        }

        if (selection.Weather == WeatherType.Hot || selection.Weather == WeatherType.Heatwave || selection.Weather == WeatherType.Drought)
        {
            TileNode coolTile = CharacterTaskCommon.FindNearestReachableStructureTile(
                current, activeNodes, _maxMoveTilesPerTurn, StructureType.SweatingStone);

            if (coolTile != null)
            {
                _moveTargetTile = coolTile;
                return SmallTurnActionType.MoveToShelter;
            }
            ResourceType fanMissing = ResourceType.None;
            bool fanReady = false;
            if (equipment.Utility != UtilityType.Fan)
                fanReady = TryPrepareCraft(ItemIds.Fan, inv, out fanMissing);

            ResourceType coolMissing = ResourceType.None;
            bool stoneReady = TryPrepareBuild("coldStone", inv, out coolMissing);

            if (Random.Range(0, 100) < WeatherPrepChance)
            {
                if (fanReady && stoneReady)
                    return Random.Range(0, 2) == 0 ? SmallTurnActionType.Craft : SmallTurnActionType.Build;
                if (fanReady) return SmallTurnActionType.Craft;
                if (stoneReady) return SmallTurnActionType.Build;
            }

            if (fanMissing != ResourceType.None) PushMissingFocus(fanMissing, 2);
            if (coolMissing != ResourceType.None) PushMissingFocus(coolMissing, 2);
            
        }

        if (selection.Weather == WeatherType.Cold || selection.Weather == WeatherType.ExtremeCold)
        {
            TileNode warmTile = CharacterTaskCommon.FindNearestReachableStructureTile(
                current, activeNodes, _maxMoveTilesPerTurn, StructureType.Campfire, StructureType.Torch);

            if (warmTile != null)
            {
                _moveTargetTile = warmTile;
                return SmallTurnActionType.MoveToShelter;
            }

            ResourceType clothMissing = ResourceType.None;
            bool clothReady = false;
            if (equipment.Armor != ArmorType.Cloth)
                clothReady = TryPrepareCraft(ItemIds.Cloth, inv, out clothMissing);

            ResourceType warmMissing = ResourceType.None;
            bool warmReady = TryPrepareAnyWarmBuild(inv, out warmMissing);

            if (Random.Range(0, 100) < WeatherPrepChance)
            {
                if (clothReady && warmReady)
                    return Random.Range(0, 2) == 0 ? SmallTurnActionType.Craft : SmallTurnActionType.Build;
                if (clothReady) return SmallTurnActionType.Craft;
                if (warmReady) return SmallTurnActionType.Build;
            }

            if (clothMissing != ResourceType.None) PushMissingFocus(clothMissing, 2);
            if (warmMissing != ResourceType.None) PushMissingFocus(warmMissing, 2);
        }

        // 붕대/구급상자 확보 우선 (높은 우선)
        // if (inv.Bandage <= 0)
        // {
        //     ResourceType bandageMissing = ResourceType.None;
        //     bool canCraftBandage = TryPrepareCraft(ItemIds.Bandage, inv, out bandageMissing);
        //     if (canCraftBandage) return SmallTurnActionType.Craft;

        //     if (TrySetGatherFocusForMissing(bandageMissing, current, activeNodes) && Random.Range(0, 100) < 45)
        //         return SmallTurnActionType.Gather;
        // }


        // if (inv.Medkit <= 0)
        // {
        //     ResourceType medkitMissing = ResourceType.None;
        //     bool canCraftMedkit = TryPrepareCraft(ItemIds.Medkit, inv, out medkitMissing);
        //     if (canCraftMedkit) return SmallTurnActionType.Craft;

        //     if (TrySetGatherFocusForMissing(medkitMissing, current, activeNodes) && Random.Range(0, 100) < 40)
        //         return SmallTurnActionType.Gather;
        // }


        // 부족 재료 기억 포커스
        if (_missingFocusTurnsRemaining > 0)
        {
            _missingFocusTurnsRemaining--;

            if (Random.Range(0, 100) < MissingFocusGatherChance)
            {
                if (TrySetGatherFocus(_missingFocusType, current, activeNodes))
                    return SmallTurnActionType.Gather;
            }
        }


        int gatherLevel = owner.GetStatLevel(StatType.Gather);
        SmallTurnActionType picked = brain.DecideSmallTurnAction(data, status, equipment, inv, selection);
        return ValidatePickedAction(picked, current, activeNodes, inv, gatherLevel);

    }

    private SmallTurnActionType ValidatePickedAction(
        SmallTurnActionType picked,
        TileNode current,
        List<TileNode> activeNodes,
        PlayerResourceInventory inv,
        int gatherLevel)
    {
        if (picked == SmallTurnActionType.Eat)
        {
            if (inv.Berry > 0) return picked;
            if (TrySetGatherFocus(ResourceType.Berry, current, activeNodes)) return SmallTurnActionType.Gather;
            return SmallTurnActionType.Rest;
        }

        if (picked == SmallTurnActionType.Gather)
        {
            if (HasAnyReachableResource(current, activeNodes, gatherLevel)) return picked;
            return SmallTurnActionType.Wander;
        }

        if (picked == SmallTurnActionType.Build)
        {
            if (TryPrepareAnyBuild(inv)) return SmallTurnActionType.Build;
            if (TrySetGatherFocus(_missingFocusType, current, activeNodes)) return SmallTurnActionType.Gather;
            return SmallTurnActionType.Wander;
        }

        if (picked == SmallTurnActionType.Craft)
        {
            if (TryPrepareAnyCraft(inv)) return SmallTurnActionType.Craft;
            if (TrySetGatherFocus(_missingFocusType, current, activeNodes)) return SmallTurnActionType.Gather;
            return SmallTurnActionType.Wander;
        }

        return picked;
    }

    private bool CanStartBuildRecipe(PlayerResourceInventory inv, string recipeId, out ResourceType missing)
    {
        missing = ResourceType.None;
        BuildRecipe recipe = FindBuildRecipe(recipeId);
        if (recipe == null) return false;

        missing = CharacterTaskCommon.GetFirstMissingResource(inv, recipe.Costs);
        return missing == ResourceType.None;
    }

    private bool TryPrepareBuild(string recipeId, PlayerResourceInventory inv, out ResourceType missing)
    {
        missing = ResourceType.None;
        BuildRecipe recipe = FindBuildRecipe(recipeId);
        if (recipe == null || recipe.Prefab == null) return false;

        missing = CharacterTaskCommon.GetFirstMissingResource(inv, recipe.Costs);
        if (missing != ResourceType.None) return false;

        _nextBuildRecipeId = recipeId;
        return true;
    }

    private bool TryPrepareAnyWarmBuild(PlayerResourceInventory inv, out ResourceType missing)
    {
        missing = ResourceType.None;

        if (TryPrepareBuild("campfire", inv, out missing)) return true;
        if (TryPrepareBuild("torch", inv, out missing)) return true;

        return false;
    }

    private bool TryPrepareCraft(string recipeId, PlayerResourceInventory inv, out ResourceType missing)
    {
        missing = ResourceType.None;
        CraftRecipe recipe = FindCraftRecipe(recipeId);
        if (recipe == null) return false;

        missing = CharacterTaskCommon.GetFirstMissingResource(inv, recipe.Costs);
        if (missing != ResourceType.None) return false;

        _nextCraftRecipeId = recipeId;
        return true;
    }

    private bool TryPrepareAnyBuild(PlayerResourceInventory inv)
    {
        if (_buildRecipes == null || _buildRecipes.Length == 0) return false;

        List<BuildRecipe> affordable = new List<BuildRecipe>();

        for (int i = 0; i < _buildRecipes.Length; i++)
        {
            BuildRecipe r = _buildRecipes[i];
            if (r == null || r.Prefab == null) continue;
            if (!CharacterTaskCommon.CanAfford(inv, r.Costs)) continue;
            affordable.Add(r);
        }

        if (affordable.Count == 0) return false;
        _nextBuildRecipeId = affordable[Random.Range(0, affordable.Count)].Id;
        return true;
    }

    private bool TryPrepareAnyCraft(PlayerResourceInventory inv)
    {
        if (_craftRecipes == null || _craftRecipes.Length == 0) return false;

        List<CraftRecipe> affordable = new List<CraftRecipe>();

        for (int i = 0; i < _craftRecipes.Length; i++)
        {
            CraftRecipe r = _craftRecipes[i];
            if (r == null) continue;
            if (!CharacterTaskCommon.CanAfford(inv, r.Costs)) continue;
            affordable.Add(r);
        }

        if (affordable.Count == 0) return false;
        _nextCraftRecipeId = affordable[Random.Range(0, affordable.Count)].Id;
        return true;
    }

    private bool HasAnyReachableResource(TileNode current, List<TileNode> activeNodes, int gatherLevel)
    {
        if (gatherLevel >= 1 && CharacterTaskCommon.FindNearestReachableResourceTile(current, activeNodes, ResourceType.Berry) != null) return true;
        if (gatherLevel >= 1 && CharacterTaskCommon.FindNearestReachableResourceTile(current, activeNodes, ResourceType.Tree) != null) return true;
        if (gatherLevel >= 4 && CharacterTaskCommon.FindNearestReachableResourceTile(current, activeNodes, ResourceType.Grass) != null) return true;
        if (gatherLevel >= 7 && CharacterTaskCommon.FindNearestReachableResourceTile(current, activeNodes, ResourceType.Rock) != null) return true;
        return false;
    }


    private bool TrySetGatherFocus(ResourceType type, TileNode current, List<TileNode> activeNodes)
    {
        if (type == ResourceType.None) return false;
        TileNode target = CharacterTaskCommon.FindNearestReachableResourceTile(current, activeNodes, type);
        if (target == null) return false;

        _nextGatherType = type;
        return true;
    }

    private bool TrySetGatherFocusForMissing(ResourceType missing, TileNode current, List<TileNode> activeNodes)
    {
        if (missing == ResourceType.None) return false;
        PushMissingFocus(missing, 2);
        return TrySetGatherFocus(missing, current, activeNodes);
    }

    private void PushMissingFocus(ResourceType type, int turns)
    {
        if (type == ResourceType.None) return;
        _missingFocusType = type;
        _missingFocusTurnsRemaining = Mathf.Max(_missingFocusTurnsRemaining, turns);
    }

    private BuildRecipe FindBuildRecipe(string id)
    {
        if (_buildRecipes == null) return null;

        for (int i = 0; i < _buildRecipes.Length; i++)
        {
            BuildRecipe r = _buildRecipes[i];
            if (r == null) continue;
            if (r.Id == id) return r;
        }
        return null;
    }

    private CraftRecipe FindCraftRecipe(string id)
    {
        if (_craftRecipes == null) return null;

        for (int i = 0; i < _craftRecipes.Length; i++)
        {
            CraftRecipe r = _craftRecipes[i];
            if (r == null) continue;
            if (r.Id == id) return r;
        }
        return null;
    }

    public IEnumerator RunGatherTurn(CharacterEntity owner, int smallTurn, List<TileNode> activeNodes, SmallTurnLogController logController)
    {
        ResourceType preferred = _nextGatherType;
        _nextGatherType = ResourceType.None;

        yield return _gatherTask.RunTurn(owner, smallTurn, activeNodes, logController, _maxMoveTilesPerTurn, preferred);
    }

    public IEnumerator RunBuildTurn(CharacterEntity owner, int smallTurn, List<TileNode> activeNodes, SmallTurnLogController logController)
    {
        string forcedRecipeId = _nextBuildRecipeId;
        _nextBuildRecipeId = null;

        yield return _buildTask.RunTurn(owner, smallTurn, activeNodes, logController, _buildRecipes, _maxMoveTilesPerTurn, owner.GetStatLevel(StatType.Build), forcedRecipeId);

        if (_buildTask.LastFailedByMissingResource)
            PushMissingFocus(_buildTask.LastMissingResourceType, 2);
    }

    public IEnumerator RunCraftTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        string forcedRecipeId = _nextCraftRecipeId;
        _nextCraftRecipeId = null;

        yield return _craftTask.RunTurn(owner, smallTurn, logController, _craftRecipes, owner.GetStatLevel(StatType.Craft), forcedRecipeId);

        if (_craftTask.LastFailedByMissingResource)
            PushMissingFocus(_craftTask.LastMissingResourceType, 2);
    }

    public IEnumerator RunMoveToShelterTurn(CharacterEntity owner, int smallTurn, List<TileNode> activeNodes, SmallTurnLogController logController)
    {
        if (_moveTargetTile == null || !activeNodes.Contains(_moveTargetTile))
        {
            _moveTargetTile = null;
            yield break;
        }

        if (owner.CurrentTileNode == _moveTargetTile)
        {
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 보호 구조물 근처에 머뭅니다."));
            _moveTargetTile = null;
            yield break;
        }

        List<TileNode> path = CharacterTaskCommon.FindPath(owner.CurrentTileNode, _moveTargetTile, activeNodes);
        if (path == null || path.Count == 0)
        {
            _moveTargetTile = null;
            yield break;
        }

        int moveCount = Mathf.Min(_maxMoveTilesPerTurn, path.Count);
        logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 보호 구조물 쪽으로 이동합니다. ({moveCount}칸)"));

        for (int i = 0; i < moveCount; i++)
            yield return owner.MoveToTile(path[i]);
    }

    public void RunRestTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {

        if (owner.CurrentTileNode != null &&
            CharacterTaskCommon.TryGetPlacedStructureType(owner.CurrentTileNode, out StructureType placedType) &&
            placedType == StructureType.Bed)
        {
            owner.Status.AddSleep(100f, owner.Data);
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 침대에서 잠을 자고 수면을 회복합니다. (+100)"));
            return;
        }
        if (_forcedRestTurnsRemaining > 0)
        {
            _forcedRestTurnsRemaining--;
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 강제로 잠을 잡니다."));

            if (_forcedRestTurnsRemaining == 0)
            {
                owner.Status.AddSleep(ForcedSleepRecoverAmount, owner.Data);
                logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 수면이 회복되었습니다. (+{ForcedSleepRecoverAmount})"));
            }
            return;
        }

            owner.Status.AddSleep(20f, owner.Data);
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 휴식을 취합니다. (수면 +20)"));

    }

    public void RunEatAction(CharacterEntity owner, int smallTurn, SmallTurnLogController logController, float berryHungerRecoverAmount)
    {
        _itemUseTask.RunEatAction(owner, smallTurn, logController, berryHungerRecoverAmount);
    }

    public void RunEquipWoodenSpearTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        _itemUseTask.RunEquipWoodenSpearTurn(owner, smallTurn, logController);
    }

    public void RunEquipStoneSpearTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        _itemUseTask.RunEquipStoneSpearTurn(owner, smallTurn, logController);
    }

    public void RunEquipFanTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        _itemUseTask.RunEquipFanTurn(owner, smallTurn, logController);
    }

    public void RunUseBandageTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        _itemUseTask.RunUseBandageTurn(owner, smallTurn, logController);
    }

    public void RunUseMedkitTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        _itemUseTask.RunUseMedkitTurn(owner, smallTurn, logController);
    }
    public IEnumerator RunSocialTurn(CharacterEntity owner, int smallTurn, List<TileNode> activeNodes, SmallTurnLogController logController)
    {
        int socialLevel = owner.GetStatLevel(StatType.Social);
        yield return _socialTask.RunTurn(owner, smallTurn, activeNodes, logController, _maxMoveTilesPerTurn, socialLevel);
    }

}
