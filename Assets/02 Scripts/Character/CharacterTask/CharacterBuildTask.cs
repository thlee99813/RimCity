using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBuildTask
{
    public bool IsForced { get; private set; }

    private BuildRecipe _recipe;
    private TileNode _targetTile;
    private int _turnsRemaining;
    
    public bool LastFailedByMissingResource { get; private set; }
    public ResourceType LastMissingResourceType { get; private set; } = ResourceType.None;


    public IEnumerator RunTurn(
        CharacterEntity owner,
        int smallTurn,
        List<TileNode> activeNodes,
        SmallTurnLogController log,
        BuildRecipe[] recipes,
        int maxMoveTilesPerTurn,
        int buildLevel,
        string forcedRecipeId = null)
    {
        if (!IsForced)
        {
            LastFailedByMissingResource = false;
            LastMissingResourceType = ResourceType.None;

            if (recipes == null || recipes.Length == 0) yield break;

            PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
            _recipe = PickStartRecipe(recipes, inv, forcedRecipeId, buildLevel);

            if (_recipe == null)
            {
                if (!string.IsNullOrEmpty(forcedRecipeId))
                {
                    BuildRecipe desired = FindRecipeById(recipes, forcedRecipeId);
                    if (desired != null)
                    {
                        ResourceType miss = CharacterTaskCommon.GetFirstMissingResource(inv, desired.Costs);
                        if (miss != ResourceType.None)
                        {
                            LastFailedByMissingResource = true;
                            LastMissingResourceType = miss;
                        }
                    }
                }
                log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 건설 가능한 작업이 없습니다."));

                yield break;
            }
            _targetTile = FindNearestBuildableTile(owner.CurrentTileNode, activeNodes);
            if (_targetTile == null) 
            { 
                log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 건설할 위치를 찾지 못했습니다."));

                Clear(); yield break; 
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
            log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName} 건설 위치로 이동합니다. ({moveCount}칸)"));

            for (int i = 0; i < moveCount; i++)
                yield return owner.MoveToTile(path[i]);

            yield break;
        }

       _turnsRemaining--;
        if (_turnsRemaining > 0)
        {
            log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName} 건설 중입니다. ({_turnsRemaining}턴 남음)"));
            yield break;
        }

        int buildFailChance = GetLowSkillFailChance(buildLevel, _recipe.RecommendedBuildLevel);
        if (buildFailChance > 0 && Random.Range(0, 100) < buildFailChance)
        {
            log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName}을 건설하다가 손이 삐끗했습니다."));
            Clear();
            yield break;
        }

        GameObject built = Object.Instantiate(_recipe.Prefab, _targetTile.transform);

        built.transform.localPosition = Vector3.zero;
        built.transform.localRotation = Quaternion.identity;
        _targetTile.SetStructure(built);

        log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName} 건설을 완료했습니다."));
        owner.AddStatActionCount(StatType.Build, 1, smallTurn, log);

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

    private BuildRecipe FindRecipeById(BuildRecipe[] recipes, string id)
    {
        for (int i = 0; i < recipes.Length; i++)
        {
            BuildRecipe r = recipes[i];
            if (r == null) continue;
            if (r.Id == id) return r;
        }
        return null;
    }

    private BuildRecipe PickStartRecipe(BuildRecipe[] recipes, PlayerResourceInventory inv, string forcedRecipeId, int buildLevel)
    {
        buildLevel = Mathf.Clamp(buildLevel, 1, 11);

        if (!string.IsNullOrEmpty(forcedRecipeId))
        {
            BuildRecipe forced = FindRecipeById(recipes, forcedRecipeId);
            if (forced == null || forced.Prefab == null) return null;
            if (buildLevel < Mathf.Max(1, forced.RequiredBuildLevel)) return null;
            if (!CharacterTaskCommon.CanAfford(inv, forced.Costs)) return null;
            return forced;
        }

        List<BuildRecipe> candidates = new List<BuildRecipe>();

        for (int i = 0; i < recipes.Length; i++)
        {
            BuildRecipe r = recipes[i];
            if (r == null || r.Prefab == null) continue;
            if (buildLevel < Mathf.Max(1, r.RequiredBuildLevel)) continue;
            if (!CharacterTaskCommon.CanAfford(inv, r.Costs)) continue;

            candidates.Add(r);
        }

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    private int GetLowSkillFailChance(int currentLevel, int recommendedLevel)
    {
        int rec = Mathf.Max(1, recommendedLevel);
        int lv = Mathf.Clamp(currentLevel, 1, 11);
        int gap = rec - lv;

        if (gap >= 2) return 60;
        if (gap == 1) return 30;
        return 0;
    }


}
