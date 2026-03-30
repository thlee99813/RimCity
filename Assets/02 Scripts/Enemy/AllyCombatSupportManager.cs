using UnityEngine;
using System.Collections.Generic;

public class AllyCombatSupportManager : Singleton<AllyCombatSupportManager>
{
    private readonly List<TileNode> _combatTiles = new List<TileNode>();

    protected override void Init() { }

    public void ClearReports()
    {
        _combatTiles.Clear();
    }

    public void ReportCombatTile(TileNode tile)
    {
        if (tile == null) return;
        if (_combatTiles.Contains(tile)) return;
        _combatTiles.Add(tile);
    }

    public TileNode FindNearestCombatTile(TileNode from, List<TileNode> activeNodes, int maxSteps)
    {
        if (from == null || activeNodes == null) return null;

        TileNode best = null;
        int bestSteps = int.MaxValue;

        for (int i = _combatTiles.Count - 1; i >= 0; i--)
        {
            TileNode tile = _combatTiles[i];

            if (tile == null || !activeNodes.Contains(tile))
            {
                _combatTiles.RemoveAt(i);
                continue;
            }

            EnemyEntity enemy = EnemyManager.Instance.GetEnemyOnTile(tile);
            if (enemy == null || enemy.IsDead)
            {
                _combatTiles.RemoveAt(i);
                continue;
            }

            List<TileNode> path = CharacterTaskCommon.FindPath(from, tile, activeNodes);
            if (path == null) continue;

            int steps = path.Count;
            if (steps > maxSteps) continue;

            if (steps < bestSteps)
            {
                bestSteps = steps;
                best = tile;
            }
        }

        return best;
    }
}
