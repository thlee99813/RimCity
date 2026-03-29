using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTaskController
{
    private bool _isForcedAction;
    private SmallTurnActionType _forcedAction = SmallTurnActionType.Idle;

    private ResourceNode _gatherTargetResource;
    private TileNode _gatherTargetTile;

    private readonly int _maxMoveTilesPerTurn;

    private ResourceType _gatherTargetType = ResourceType.None;
    private readonly BuildRecipe[] _buildRecipes;
    private readonly CraftRecipe[] _craftRecipes;


    private BuildRecipe _buildRecipe;
    private TileNode _buildTargetTile;
    private int _buildTurnsRemaining;

    private CraftRecipe _craftRecipe;
    private int _craftTurnsRemaining;
    



    public CharacterTaskController(int maxMoveTilesPerTurn, BuildRecipe[] buildRecipes, CraftRecipe[] craftRecipes)
    {
        _maxMoveTilesPerTurn = Mathf.Max(1, maxMoveTilesPerTurn);
        _buildRecipes = buildRecipes;
        _craftRecipes = craftRecipes;

    }

    public SmallTurnActionType ResolveAction(CharacterData data,CharacterStatus status, CharacterEquipment equipment, BigTurnSelectionData selection, CharacterBrain brain)
    {
        if (_isForcedAction) return _forcedAction;
        return brain.DecideSmallTurnAction(data, status, equipment, GameManager.Instance.PlayerInventory, selection);
    }

    public IEnumerator RunGatherTurn(
        CharacterEntity owner,
        int smallTurn,
        List<TileNode> activeNodes,
        SmallTurnLogController logController)
    {
        if (!_isForcedAction)
        {
            ResourceType targetType = DecideGatherTargetType();
            bool found = TryAcquireGatherTarget(owner.CurrentTileNode, activeNodes, targetType);
            if (!found)
            {
                logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 수집할 {ResourceKorean(_gatherTargetType)}를 찾지 못합니다."));
                yield break;
            }

            _isForcedAction = true;
            _forcedAction = SmallTurnActionType.Gather;
        }

        if (!IsGatherTargetValid(activeNodes))
        {
            ClearForcedGather();
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 수집 대상을 잃어버립니다."));
            yield break;
        }

        if (owner.CurrentTileNode == _gatherTargetTile)
        {
            _gatherTargetResource.Consume(1);
            GameManager.Instance.PlayerInventory.Add(_gatherTargetType, 3);


            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {ResourceKorean(_gatherTargetType)}를 수집합니다."));
            ClearForcedGather();
            yield break;
        }

        List<TileNode> path = FindPath(owner.CurrentTileNode, _gatherTargetTile, activeNodes);
        if (path == null || path.Count == 0)
        {
            ClearForcedGather();
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {ResourceKorean(_gatherTargetType)}까지 가는 길을 찾지 못합니다."));
            yield break;
        }

        int moveCount = Mathf.Min(_maxMoveTilesPerTurn, path.Count);
        logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {ResourceKorean(_gatherTargetType)}를 수집하러 이동합니다. ({moveCount}칸 이동)"));

        for (int i = 0; i < moveCount; i++)
        {
            yield return owner.MoveToTile(path[i]);
        }
    }
    public IEnumerator RunBuildTurn(
    CharacterEntity owner,
    int smallTurn,
    List<TileNode> activeNodes,
    SmallTurnLogController logController)
    {
        if (!_isForcedAction)
        {
            if (_buildRecipes == null || _buildRecipes.Length == 0)
            {
                logController.AddLog($"[{smallTurn} 턴] 건축 레시피가 없습니다.");
                yield break;
            }

            _buildRecipe = DecideBuildRecipe();
            if (_buildRecipe == null || _buildRecipe.Prefab == null)
            {
                logController.AddLog($"[{smallTurn} 턴] 건축 레시피가 잘못되었습니다.");
                yield break;
            }

            if (!CanAfford(GameManager.Instance.PlayerInventory, _buildRecipe.Costs))
            {
                logController.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 재료가 부족해 {_buildRecipe.DisplayName} 건축을 시작하지 못합니다.");
                yield break;
            }

            _buildTargetTile = FindNearestBuildableTile(owner.CurrentTileNode, activeNodes);
            if (_buildTargetTile == null)
            {
                logController.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 건축 가능한 타일을 찾지 못합니다.");
                yield break;
            }

            ConsumeCosts(GameManager.Instance.PlayerInventory, _buildRecipe.Costs);
            _buildTurnsRemaining = Mathf.Max(1, _buildRecipe.BuildTurns);

            _isForcedAction = true;
            _forcedAction = SmallTurnActionType.Build;
        }

        if (_buildTargetTile == null || _buildTargetTile.IsOccupied || _buildTargetTile.HasResource)
        {
            ClearForcedBuild();
            yield break;
        }

        if (owner.CurrentTileNode != _buildTargetTile)
        {
            List<TileNode> path = FindPath(owner.CurrentTileNode, _buildTargetTile, activeNodes);
            if (path == null || path.Count == 0)
            {
                ClearForcedBuild();
                logController.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 건축 위치로 이동하지 못합니다.");
                yield break;
            }

            int moveCount = Mathf.Min(_maxMoveTilesPerTurn, path.Count);
            logController.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_buildRecipe.DisplayName} 건축 위치로 이동합니다. ({moveCount}칸)");

            for (int i = 0; i < moveCount; i++)
                yield return owner.MoveToTile(path[i]);

            yield break;
        }

        _buildTurnsRemaining--;
        if (_buildTurnsRemaining > 0)
        {
            logController.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_buildRecipe.DisplayName} 건축 중입니다. ({_buildTurnsRemaining}턴 남음)");
            yield break;
        }

        GameObject built = Object.Instantiate(_buildRecipe.Prefab, _buildTargetTile.transform);
        built.transform.localPosition = Vector3.zero;
        built.transform.localRotation = Quaternion.identity;
        _buildTargetTile.SetStructure(built);

        logController.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_buildRecipe.DisplayName} 건축을 완료했습니다.");
        ClearForcedBuild();
    }
    public IEnumerator RunCraftTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        if (!_isForcedAction)
        {
            if (_craftRecipes == null || _craftRecipes.Length == 0)
            {
                logController.AddLog($"[{smallTurn} 턴] 제작 레시피가 없습니다.");
                yield break;
            }

            _craftRecipe = _craftRecipes[Random.Range(0, _craftRecipes.Length)];
            if (_craftRecipe == null)
            {
                logController.AddLog($"[{smallTurn} 턴] 제작 레시피가 비어 있습니다.");
                yield break;
            }

            PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
            if (!CanAfford(inv, _craftRecipe.Costs))
            {
                logController.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 재료가 부족해 {_craftRecipe.DisplayName} 제작에 실패했습니다.");
                yield break;
            }

            ConsumeCosts(inv, _craftRecipe.Costs);

            _craftTurnsRemaining = Mathf.Max(1, _craftRecipe.CraftTurns);
            _isForcedAction = true;
            _forcedAction = SmallTurnActionType.Craft;
        }

        _craftTurnsRemaining--;

        if (_craftTurnsRemaining > 0)
        {
            logController.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_craftRecipe.DisplayName} 제작 중입니다. ({_craftTurnsRemaining}턴 남음)");
            yield break;
        }

        ApplyCraftResult(owner, GameManager.Instance.PlayerInventory, _craftRecipe.Id);
        logController.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_craftRecipe.DisplayName} 제작을 완료했습니다.");

        ClearForcedCraft();
    }

    private void ApplyCraftResult(CharacterEntity owner, PlayerResourceInventory inv, string itemId)
    {
        if (itemId == ItemIds.Cloth)
        {
            if (!owner.Equipment.TryEquipArmor(ArmorType.Cloth))
                inv.AddItem(itemId, 1);
            return;
        }

        if (itemId == ItemIds.WoodenSpear)
        {
            owner.EquipWeaponWithSwap(WeaponType.WoodenSpear, inv);
            return;
        }

        if (itemId == ItemIds.StoneSpear)
        {
            owner.EquipWeaponWithSwap(WeaponType.StoneSpear, inv);
            return;
        }

        if (itemId == ItemIds.Fan)
        {
            if (!owner.Equipment.TryEquipUtility(UtilityType.Fan))
                inv.AddItem(itemId, 1);
            return;
        }

        if (itemId == ItemIds.Bandage || itemId == ItemIds.Medkit)
        {
            inv.AddItem(itemId, 1);
        }
    }

    private bool TryAcquireGatherTarget(TileNode startTile, List<TileNode> activeNodes,ResourceType targetType)
    {
        _gatherTargetResource = null;
        _gatherTargetTile = null;
        _gatherTargetType = targetType;


        if (startTile == null) return false;

        int bestDistance = int.MaxValue;

        for (int i = 0; i < activeNodes.Count; i++)
        {
            TileNode tile = activeNodes[i];

            if (tile.ResourceTypeOnTile != targetType) continue;
            _gatherTargetType = targetType;

            ResourceNode resource = tile.ResourceNodeOnTile;
            if (resource == null) continue;
            if (resource.Amount <= 0) continue;
            if (!resource.gameObject.activeInHierarchy) continue;

            List<TileNode> path = FindPath(startTile, tile, activeNodes);
            if (path == null) continue;

            int distance = path.Count;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                _gatherTargetResource = resource;
                _gatherTargetTile = tile;
            }
        }

        return _gatherTargetTile != null && _gatherTargetResource != null;
    }

    private bool IsGatherTargetValid(List<TileNode> activeNodes)
    {
        if (_gatherTargetTile == null || _gatherTargetResource == null) return false;
        if (!activeNodes.Contains(_gatherTargetTile)) return false;
        if (_gatherTargetTile.ResourceTypeOnTile != _gatherTargetType) return false;
        if (_gatherTargetTile.ResourceNodeOnTile != _gatherTargetResource) return false;
        if (!_gatherTargetResource.gameObject.activeInHierarchy) return false;
        if (_gatherTargetResource.Amount <= 0) return false;

        return true;
    }

    private void ClearForcedGather()
    {
        _isForcedAction = false;
        _forcedAction = SmallTurnActionType.Idle;
        _gatherTargetResource = null;
        _gatherTargetTile = null;
        _gatherTargetType = ResourceType.None;
    }

    private List<TileNode> FindPath(TileNode start, TileNode goal, List<TileNode> activeNodes)
    {
        if (start == null || goal == null) return null;
        if (start == goal) return new List<TileNode>();

        HashSet<TileNode> activeSet = new HashSet<TileNode>(activeNodes);
        if (!activeSet.Contains(start) || !activeSet.Contains(goal)) return null;

        Queue<TileNode> queue = new Queue<TileNode>();
        Dictionary<TileNode, TileNode> cameFrom = new Dictionary<TileNode, TileNode>();
        HashSet<TileNode> visited = new HashSet<TileNode>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            TileNode current = queue.Dequeue();
            if (current == goal) break;

            for (int i = 0; i < current.Neighbors.Count; i++)
            {
                TileNode next = current.Neighbors[i];
                if (next == null) continue;
                if (!activeSet.Contains(next)) continue;
                if (visited.Contains(next)) continue;

                visited.Add(next);
                cameFrom[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!visited.Contains(goal)) return null;

        List<TileNode> path = new List<TileNode>();
        TileNode walk = goal;
        while (walk != start)
        {
            path.Add(walk);
            walk = cameFrom[walk];
        }

        path.Reverse();
        return path;
    }
    public void RunEatAction(CharacterEntity owner, int smallTurn, SmallTurnLogController logController, float berryHungerRecoverAmount)
    {
        bool consumed = GameManager.Instance.PlayerInventory.Consume(ResourceType.Berry, 1);
        if (!consumed)
        {
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 먹을 산딸기가 없습니다."));
            return;
        }

        owner.Status.AddHunger(berryHungerRecoverAmount, owner.Data);
        logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 산딸기를 먹고 허기를 회복합니다."));
    }

    private string ResourceKorean(ResourceType t)
    {
        switch (t)
        {
            case ResourceType.Berry: return "산딸기";
            case ResourceType.Tree: return "나무";
            case ResourceType.Rock: return "바위";
            case ResourceType.Grass: return "섬유";
            default: return "자원";
        }
    }

    private ResourceType DecideGatherTargetType()
    {
        int roll = Random.Range(0, 4);
        if (roll == 0) return ResourceType.Berry;
        if (roll == 1) return ResourceType.Tree;
        if (roll == 2) return ResourceType.Rock;
        return ResourceType.Grass;
    }

    private BuildRecipe DecideBuildRecipe()
    {
        return _buildRecipes[Random.Range(0, _buildRecipes.Length)];
    }

    private bool CanAfford(PlayerResourceInventory inv, ResourceCost[] costs)
    {
        for (int i = 0; i < costs.Length; i++)
            if (inv.GetAmount(costs[i].Type) < costs[i].Amount) return false;
        return true;
    }

    private void ConsumeCosts(PlayerResourceInventory inv, ResourceCost[] costs)
    {
        for (int i = 0; i < costs.Length; i++)
            inv.Consume(costs[i].Type, costs[i].Amount);
    }

    private TileNode FindNearestBuildableTile(TileNode from, List<TileNode> activeNodes)
    {
        TileNode best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < activeNodes.Count; i++)
        {
            TileNode node = activeNodes[i];
            if (node == null) continue;
            if (node.IsOccupied) continue;
            if (node.HasResource) continue;


            float d = (node.WorldPosition - from.WorldPosition).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = node;
            }
        }

        return best;
    }

    private void ClearForcedBuild()
    {
        _isForcedAction = false;
        _forcedAction = SmallTurnActionType.Idle;
        _buildRecipe = null;
        _buildTargetTile = null;
        _buildTurnsRemaining = 0;
    }
    private void ClearForcedCraft()
    {
        _isForcedAction = false;
        _forcedAction = SmallTurnActionType.Idle;
        _craftRecipe = null;
        _craftTurnsRemaining = 0;
    }
    public void RunEquipWoodenSpearTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;

        if (!inv.ConsumeItem(ItemIds.WoodenSpear, 1))
        {
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 나무창이 없어 착용하지 못합니다."));
            return;
        }

        owner.EquipWeaponWithSwap(WeaponType.WoodenSpear, inv);
        logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 나무창을 착용합니다."));
    }

    public void RunEquipStoneSpearTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;

        if (!inv.ConsumeItem(ItemIds.StoneSpear, 1))
        {
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 돌창이 없어 착용하지 못합니다."));
            return;
        }

        owner.EquipWeaponWithSwap(WeaponType.StoneSpear, inv);
        logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 돌창을 착용합니다."));
    }

    public void RunEquipFanTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;

        if (!inv.ConsumeItem(ItemIds.Fan, 1))
        {
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 부채가 없어 착용하지 못합니다."));
            return;
        }

        // 이미 팬 착용 중이면 소비한 부채를 되돌림
        if (!owner.Equipment.TryEquipUtility(UtilityType.Fan))
        {
            inv.AddItem(ItemIds.Fan, 1);
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 이미 부채를 착용 중입니다."));
            return;
        }

        logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 부채를 착용합니다."));
    }

    public void RunUseBandageTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;

        if (!inv.ConsumeItem(ItemIds.Bandage, 1))
        {
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 붕대가 없어 사용할 수 없습니다."));
            return;
        }

        owner.Status.AddHealth(20f, owner.Data);
        logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 붕대를 사용합니다."));
    }

    public void RunUseMedkitTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;

        if (!inv.ConsumeItem(ItemIds.Medkit, 1))
        {
            logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 구급상자가 없어 사용할 수 없습니다."));
            return;
        }

        owner.Status.AddHealth(60f, owner.Data);
        logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 구급상자를 사용합니다."));
    }



}