using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : Singleton<EnemyManager>
{
    public readonly List<EnemyEntity> ActiveEnemies = new List<EnemyEntity>();

    protected override void Init() { }

    public void Register(EnemyEntity enemy)
    {
        if (enemy == null) return;
        if (!ActiveEnemies.Contains(enemy))
            ActiveEnemies.Add(enemy);
    }

    public void Unregister(EnemyEntity enemy)
    {
        ActiveEnemies.Remove(enemy);
    }

    public IEnumerator RunEnemyPhase(int smallTurnsPerBigTurn, List<TileNode> activeNodes, List<CharacterEntity> characters)
    {
        int enemyTurns = Mathf.Max(1, smallTurnsPerBigTurn);

        for (int turn = 0; turn < enemyTurns; turn++)
        {
            List<EnemyEntity> snapshot = new List<EnemyEntity>(ActiveEnemies);

            for (int i = 0; i < snapshot.Count; i++)
            {
                EnemyEntity enemy = snapshot[i];
                if (enemy == null || enemy.IsDead) continue;
                yield return enemy.RunEnemySmallTurn(activeNodes, characters);
            }
        }
    }

    public bool IsTileOccupiedByOtherEnemy(TileNode tile, EnemyEntity self)
    {
        if (tile == null) return false;

        for (int i = 0; i < ActiveEnemies.Count; i++)
        {
            EnemyEntity e = ActiveEnemies[i];
            if (e == null || e.IsDead) continue;
            if (e == self) continue;
            if (e.CurrentTileNode == tile) return true;
        }

        return false;
    }

    public EnemyEntity GetEnemyOnTile(TileNode tile)
    {
        if (tile == null) return null;

        for (int i = 0; i < ActiveEnemies.Count; i++)
        {
            EnemyEntity enemy = ActiveEnemies[i];
            if (enemy == null || enemy.IsDead) continue;
            if (enemy.CurrentTileNode == tile) return enemy;
        }

        return null;
    }
}
