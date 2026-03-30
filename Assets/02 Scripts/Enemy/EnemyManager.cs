using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
public class EnemyManager : Singleton<EnemyManager>
{
    [Header("Spawn")]
    [SerializeField] private EnemyEntity _enemyPrefab;
    [SerializeField] private Transform _enemyRoot;
    [SerializeField] private int _minSpawnPerStage = 1;
    [SerializeField] private int _maxSpawnPerStage = 2;
    [SerializeField] private float _spawnYOffset = 0f;

    [SerializeField] private Transform _spawnPoint;

    public readonly List<EnemyEntity> ActiveEnemies = new List<EnemyEntity>();
    private readonly HashSet<StageContext> _spawnedStages = new HashSet<StageContext>();

    protected override void Init()
    {
    }

    private void Start()
    {
        EventManager.Instance.AddListener(MEventType.Stageactivated, OnStageActivated);
        SpawnForAlreadyActiveStages();
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.RemoveListener(MEventType.Stageactivated, this);
    }

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

    private void OnStageActivated(MEventType type, Component sender, EventArgs args)
    {
        StageActivatedEventArgs stageArgs = args as StageActivatedEventArgs;
        if (stageArgs == null || stageArgs.StageContext == null) return;

        TrySpawnForStage(stageArgs.StageContext);
    }

    private void SpawnForAlreadyActiveStages()
    {
        List<StageContext> stages = GameManager.Instance.ActiveStages;
        for (int i = 0; i < stages.Count; i++)
            TrySpawnForStage(stages[i]);
    }

    private void TrySpawnForStage(StageContext stage)
    {
        if (_spawnedStages.Contains(stage)) return;
        _spawnedStages.Add(stage);

        SpawnForStage(stage);
    }

    private void SpawnForStage(StageContext stage)
    {
        TileNode[] tiles = stage.TileNodes;
        if (tiles == null || tiles.Length == 0) return;

        int spawnCount = UnityEngine.Random.Range(_minSpawnPerStage, _maxSpawnPerStage + 1);
        HashSet<TileNode> usedThisSpawn = new HashSet<TileNode>();

        for (int i = 0; i < spawnCount; i++)
        {
            TileNode tile = PickSpawnTileWithoutOverlap(tiles, usedThisSpawn);
            if (tile == null) break;

            usedThisSpawn.Add(tile);

            Vector3 pos = tile.WorldPosition;
            if (_spawnPoint != null) pos.y = _spawnPoint.position.y + _spawnYOffset;
            else pos.y += _spawnYOffset;

            Quaternion rot = _spawnPoint != null ? _spawnPoint.rotation : Quaternion.identity;

            Transform parent = _enemyRoot != null ? _enemyRoot : stage.transform;
            EnemyEntity enemy = Instantiate(_enemyPrefab, pos, rot, parent);
            enemy.SetCurrentTileNode(tile);
        }
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
                if (enemy == null) continue;

                yield return enemy.RunEnemySmallTurn(activeNodes, characters);
            }
        }
    }
    private TileNode PickSpawnTileWithoutOverlap(TileNode[] tiles, HashSet<TileNode> usedThisSpawn)
    {
        int attempts = Mathf.Max(8, tiles.Length * 2);

        for (int i = 0; i < attempts; i++)
        {
            TileNode tile = tiles[UnityEngine.Random.Range(0, tiles.Length)];
            if (tile == null) continue;
            if (usedThisSpawn.Contains(tile)) continue;
            if (IsTileOccupiedByOtherEnemy(tile, null)) continue;
            return tile;
        }

        return null;
    }

    public bool IsTileOccupiedByOtherEnemy(TileNode tile, EnemyEntity self)
    {
        if (tile == null) return false;

        for (int i = 0; i < ActiveEnemies.Count; i++)
        {
            EnemyEntity e = ActiveEnemies[i];
            if (e == null) continue;
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
            if (enemy == null) continue;
            if (enemy.IsDead) continue;
            if (enemy.CurrentTileNode == tile) return enemy;
        }

        return null;
    }


}
