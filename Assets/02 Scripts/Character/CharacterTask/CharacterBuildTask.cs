using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBuildTask
{
    public bool IsForced { get; private set; }

    private BuildRecipe _recipe;
    private TileNode _targetTile;
    private int _turnsRemaining;

    public IEnumerator RunTurn(CharacterEntity owner, int smallTurn, List<TileNode> activeNodes, SmallTurnLogController log, BuildRecipe[] recipes, int maxMoveTilesPerTurn)
    {
        if (!IsForced)
        {
            if (recipes == null || recipes.Length == 0)
            {
                log.AddLog($"[{smallTurn} 턴] 건축 레시피가 없습니다.");
                yield break;
            }

            _recipe = recipes[Random.Range(0, recipes.Length)];
            if (_recipe == null || _recipe.Prefab == null)
            {
                log.AddLog($"[{smallTurn} 턴] 건축 레시피가 비어 있습니다.");
                yield break;
            }

            PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
            if (!CharacterTaskCommon.CanAfford(inv, _recipe.Costs))
            {
                log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 재료가 부족합니다.");
                yield break;
            }

            _targetTile = FindNearestBuildableTile(owner.CurrentTileNode, activeNodes);
            if (_targetTile == null)
            {
                log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 건축 가능한 타일이 없습니다.");
                yield break;
            }

            CharacterTaskCommon.ConsumeCosts(inv, _recipe.Costs);
            _turnsRemaining = Mathf.Max(1, _recipe.BuildTurns);

            IsForced = true;
        }

        if (_targetTile == null || _targetTile.IsOccupied || _targetTile.HasResource)
        {
            Clear();
            yield break;
        }

        if (owner.CurrentTileNode != _targetTile)
        {
            List<TileNode> path = CharacterTaskCommon.FindPath(owner.CurrentTileNode, _targetTile, activeNodes);
            if (path == null || path.Count == 0)
            {
                Clear();
                yield break;
            }

            int moveCount = Mathf.Min(maxMoveTilesPerTurn, path.Count);
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName} 건설 위치로 이동합니다. ({moveCount}칸)");

            for (int i = 0; i < moveCount; i++)
                yield return owner.MoveToTile(path[i]);

            yield break;
        }

        _turnsRemaining--;
        if (_turnsRemaining > 0)
        {
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName} 건설 중입니다. ({_turnsRemaining}턴 남음)");
            yield break;
        }

        GameObject built = Object.Instantiate(_recipe.Prefab, _targetTile.transform);
        built.transform.localPosition = Vector3.zero;
        built.transform.localRotation = Quaternion.identity;
        _targetTile.SetStructure(built);

        log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName} 건설을 완료했습니다.");
        Clear();
    }

    private TileNode FindNearestBuildableTile(TileNode from, List<TileNode> activeNodes)
    {
        if (from == null) return null;

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

    private void Clear()
    {
        IsForced = false;
        _recipe = null;
        _targetTile = null;
        _turnsRemaining = 0;
    }
}
