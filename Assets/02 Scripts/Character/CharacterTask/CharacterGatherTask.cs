using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGatherTask
{
    public bool IsForced { get; private set; }

    private ResourceNode _targetResource;
    private TileNode _targetTile;
    private ResourceType _targetType = ResourceType.None;

public IEnumerator RunTurn(CharacterEntity owner, int smallTurn, List<TileNode> activeNodes, SmallTurnLogController log, int maxMoveTilesPerTurn, ResourceType preferredType = ResourceType.None)
    {
       if (!IsForced)
        {
            int gatherLevel = owner.GetStatLevel(StatType.Gather);

            ResourceType pick = preferredType;
            if (pick == ResourceType.None || !IsGatherTargetAllowed(pick, gatherLevel))
                pick = DecideGatherTargetType(gatherLevel);

            if (!TryAcquire(owner.CurrentTileNode, activeNodes, pick))
                yield break;

            IsForced = true;
        }


        if (!IsValid(activeNodes))
        {
            Clear();
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 수집 대상을 잃어버렸습니다.");
            yield break;
        }

        if (owner.CurrentTileNode == _targetTile)
        {
            _targetResource.Consume(1);
            GameManager.Instance.PlayerInventory.Add(_targetType, 3);
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {ToKorean(_targetType)}를 수집합니다.");
            owner.AddStatActionCount(StatType.Gather, 1, smallTurn, log);
            Clear();
            yield break;
        }

        List<TileNode> path = CharacterTaskCommon.FindPath(owner.CurrentTileNode, _targetTile, activeNodes);
        if (path == null || path.Count == 0)
        {
            Clear();
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 수집 위치로 이동하지 못합니다.");
            yield break;
        }

        int moveCount = Mathf.Min(maxMoveTilesPerTurn, path.Count);
        log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {ToKorean(_targetType)}를 수집하러 이동합니다. ({moveCount}칸)");

        for (int i = 0; i < moveCount; i++)
            yield return owner.MoveToTile(path[i]);
    }

    private bool TryAcquire(TileNode startTile, List<TileNode> activeNodes, ResourceType targetType)
    {
        _targetResource = null;
        _targetTile = null;
        _targetType = targetType;
        if (startTile == null) return false;

        int bestDistance = int.MaxValue;

        for (int i = 0; i < activeNodes.Count; i++)
        {
            TileNode tile = activeNodes[i];
            if (tile.ResourceTypeOnTile != targetType) continue;

            ResourceNode resource = tile.ResourceNodeOnTile;
            if (resource == null || resource.Amount <= 0 || !resource.gameObject.activeInHierarchy) continue;

            List<TileNode> path = CharacterTaskCommon.FindPath(startTile, tile, activeNodes);
            if (path == null) continue;

            int d = path.Count;
            if (d < bestDistance)
            {
                bestDistance = d;
                _targetResource = resource;
                _targetTile = tile;
            }
        }

        return _targetTile != null && _targetResource != null;
    }

    private bool IsValid(List<TileNode> activeNodes)
    {
        if (_targetTile == null || _targetResource == null) return false;
        if (!activeNodes.Contains(_targetTile)) return false;
        if (_targetTile.ResourceTypeOnTile != _targetType) return false;
        if (_targetTile.ResourceNodeOnTile != _targetResource) return false;
        if (!_targetResource.gameObject.activeInHierarchy) return false;
        if (_targetResource.Amount <= 0) return false;
        return true;
    }

    private void Clear()
    {
        IsForced = false;
        _targetResource = null;
        _targetTile = null;
        _targetType = ResourceType.None;
    }

    private bool IsGatherTargetAllowed(ResourceType type, int gatherLevel)
    {
        if (type == ResourceType.Berry || type == ResourceType.Tree) return true;
        if (type == ResourceType.Grass) return gatherLevel >= 2;
        if (type == ResourceType.Rock) return gatherLevel >= 4;
        return false;
    }

    private ResourceType DecideGatherTargetType(int gatherLevel)
    {
        Dictionary<ResourceType, int> weights = new Dictionary<ResourceType, int>
        {
            { ResourceType.Berry, 35 },
            { ResourceType.Tree, 35 },
            { ResourceType.Grass, gatherLevel >= 2 ? 20 : 0 },
            { ResourceType.Rock, gatherLevel >= 4 ? 10 : 0 }
        };

        return WeightedPickResource(weights);
    }

    private ResourceType WeightedPickResource(Dictionary<ResourceType, int> weights)
    {
        int total = 0;
        foreach (KeyValuePair<ResourceType, int> e in weights) total += Mathf.Max(0, e.Value);
        if (total <= 0) return ResourceType.Berry;

        int roll = Random.Range(0, total);
        int acc = 0;
        foreach (KeyValuePair<ResourceType, int> e in weights)
        {
            int w = Mathf.Max(0, e.Value);
            acc += w;
            if (roll < acc) return e.Key;
        }
        return ResourceType.Berry;
    }


    private string ToKorean(ResourceType t)
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
}
