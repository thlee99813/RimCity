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


    public CharacterTaskController(int maxMoveTilesPerTurn)
    {
        _maxMoveTilesPerTurn = Mathf.Max(1, maxMoveTilesPerTurn);
    }

    public SmallTurnActionType ResolveAction(CharacterData data, BigTurnSelectionData selection, CharacterBrain brain)
    {
        if (_isForcedAction) return _forcedAction;
        return brain.DecideSmallTurnAction(data, selection);
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
            default: return "자원";
        }
    }

    private ResourceType DecideGatherTargetType()
    {
        int roll = Random.Range(0, 3);
        if (roll == 0) return ResourceType.Berry;
        if (roll == 1) return ResourceType.Tree;
        return ResourceType.Rock;
    }
}