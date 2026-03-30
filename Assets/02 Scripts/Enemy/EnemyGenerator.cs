using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyGenerator : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private EnemyEntity _enemyPrefab;
    [SerializeField] private Transform _enemyRoot;
    [SerializeField] private TileNode[] _spawnTiles; 
    [SerializeField] private int _spawnCountPerWave = 1;
    [SerializeField] private float _spawnYOffset = 0f;
    [SerializeField] private bool _avoidEnemyOverlap = true;

    [Header("Interval (BigTurn)")]
    [SerializeField] private bool _spawnOnEnable = true;
    [SerializeField] private int _spawnEveryBigTurns = 3; // 활성화 이후 N턴마다

    [Header("Spawned Enemy Stats")]
    [SerializeField] private float _spawnEnemyHealth = 40f;
    [SerializeField] private float _spawnEnemyAttack = 8f;

    private StageContext _stage;
    private int _activatedBigTurn;
    private int _lastSpawnBigTurn = int.MinValue;
    private bool _isBound;

    private void Awake()
    {
        _stage = GetComponentInParent<StageContext>();
    }

    private void OnEnable()
    {
        TryBindEvent();

        _activatedBigTurn = GetCurrentBigTurn();
        if (_spawnOnEnable)
        {
            SpawnWave();
            _lastSpawnBigTurn = _activatedBigTurn;
        }
    }

    private void Start()
    {
        TryBindEvent();
    }

    private void OnDisable()
    {
        if (!_isBound) return;
        if (EventManager.Instance == null) return;

        EventManager.Instance.RemoveListener(MEventType.BigTurnStarted, this);
        _isBound = false;
    }

    private void TryBindEvent()
    {
        if (_isBound) return;
        if (EventManager.Instance == null) return;

        EventManager.Instance.AddListener(MEventType.BigTurnStarted, OnBigTurnStarted);
        _isBound = true;
    }

    private void OnBigTurnStarted(MEventType type, Component sender, EventArgs args)
    {
        BigTurnStartedEventArgs turnArgs = args as BigTurnStartedEventArgs;
        if (turnArgs == null) return;

        int bigTurn = turnArgs.BigTurn;
        if (_spawnEveryBigTurns <= 0) return;
        if (bigTurn <= _lastSpawnBigTurn) return;
        if ((bigTurn - _activatedBigTurn) < _spawnEveryBigTurns) return;
        if ((bigTurn - _activatedBigTurn) % _spawnEveryBigTurns != 0) return;

        SpawnWave();
        _lastSpawnBigTurn = bigTurn;
    }

    private int GetCurrentBigTurn()
    {
        if (TurnManager.Instance == null) return 0;
        return TurnManager.Instance.CurrentBigTurn;
    }

    private TileNode[] GetSourceTiles()
    {
        if (_spawnTiles != null && _spawnTiles.Length > 0)
            return _spawnTiles;

        if (_stage != null)
            return _stage.TileNodes;

        return null;
    }

    private void SpawnWave()
    {
        if (_enemyPrefab == null) return;

        TileNode[] tiles = GetSourceTiles();
        if (tiles == null || tiles.Length == 0) return;

        HashSet<TileNode> usedThisWave = new HashSet<TileNode>();
        int count = Mathf.Max(1, _spawnCountPerWave);

        for (int i = 0; i < count; i++)
        {
            TileNode tile = PickTile(tiles, usedThisWave);
            if (tile == null) break;

            usedThisWave.Add(tile);

            Vector3 pos = tile.WorldPosition + new Vector3(0f, _spawnYOffset, 0f);
            Transform parent = _enemyRoot;
            EnemyEntity enemy = Instantiate(_enemyPrefab, pos, Quaternion.identity);

            if (parent != null)
                enemy.transform.SetParent(parent, true);

            enemy.transform.localScale = _enemyPrefab.transform.localScale;
            enemy.InitializeStats(_spawnEnemyHealth, _spawnEnemyAttack);
            enemy.SetCurrentTileNode(tile);
        }
    }

    private TileNode PickTile(TileNode[] tiles, HashSet<TileNode> usedThisWave)
    {
        int attempts = Mathf.Max(8, tiles.Length * 2);

        for (int i = 0; i < attempts; i++)
        {
            TileNode tile = tiles[UnityEngine.Random.Range(0, tiles.Length)];
            if (tile == null) continue;
            if (usedThisWave.Contains(tile)) continue;

            if (_avoidEnemyOverlap && EnemyManager.Instance != null)
            {
                if (EnemyManager.Instance.IsTileOccupiedByOtherEnemy(tile, null)) continue;
            }

            return tile;
        }

        return null;
    }
}
