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

    [Header("Interval")]
    [SerializeField] private bool _spawnOnEnable = true;
    [SerializeField] private int _spawnEveryBigTurns = 3; // 활성화 이후 N턴마다

    [Header("Spawned Enemy Stats")]
    [SerializeField] private float _spawnEnemyHealth = 40f;
    [SerializeField] private float _spawnEnemyAttack = 8f;
    [Header("Generator")]
    [SerializeField] private TileNode _generatorTile;       // 생성기 위치 타일
    [SerializeField] private float _maxHealth = 200f;
    [SerializeField] private bool _destroyOnDeath = false;  // true면 Destroy(gameObject)

    [Header("Identity")]
    [SerializeField] private string _generatorId;
    [SerializeField] private string _generatorName = "적 생성기";

    public string GeneratorId => _generatorId;
    public string GeneratorName => string.IsNullOrWhiteSpace(_generatorName) ? name : _generatorName;


    public float CurrentHealth => _currentHealth;
    public bool IsDestroyed => _isDestroyed;
    public TileNode GeneratorTile => _generatorTile;

    private float _currentHealth;
    private bool _isDestroyed;

    private static readonly List<EnemyGenerator> _activeGenerators = new List<EnemyGenerator>();


    private StageContext _stage;
    private int _activatedBigTurn;
    private int _lastSpawnBigTurn = int.MinValue;
    private bool _isBound;

    private void Awake()
    {
        if (string.IsNullOrEmpty(_generatorId))
        _generatorId = Guid.NewGuid().ToString("N");
        _stage = GetComponentInParent<StageContext>();
        _currentHealth = Mathf.Max(1f, _maxHealth);

        if (_generatorTile == null && _stage != null && _stage.TileNodes != null && _stage.TileNodes.Length > 0)
            _generatorTile = FindNearestTile(_stage.TileNodes, transform.position);
    }

    private void OnEnable()
    {
        if (_isDestroyed) return;

        if (!_activeGenerators.Contains(this))
            _activeGenerators.Add(this);

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
        _activeGenerators.Remove(this);
        if (GeneratorName == "코마")
        {
            UIManager.Instance.ShowKomaEnding();
        }

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
        if (_isDestroyed) return;

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
        if (_isDestroyed) return;

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
    public bool TakeDamage(float amount)
    {
        if (_isDestroyed) return false;
        if (amount <= 0f) return false;

        _currentHealth = Mathf.Max(0f, _currentHealth - amount);

        if (_currentHealth <= 0f)
        {
            DestroyGenerator();
            return true;
        }

        return false;
    }

    private void DestroyGenerator()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;

        if (_isBound && EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(MEventType.BigTurnStarted, this);
            _isBound = false;
        }

        _activeGenerators.Remove(this);

        if (_destroyOnDeath) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    public static EnemyGenerator FindOnTile(TileNode tile)
    {
        if (tile == null) return null;

        for (int i = 0; i < _activeGenerators.Count; i++)
        {
            EnemyGenerator gen = _activeGenerators[i];
            if (gen == null) continue;
            if (gen._isDestroyed) continue;
            if (gen._generatorTile == tile) return gen;
        }

        return null;
    }

    private TileNode FindNearestTile(TileNode[] tiles, Vector3 worldPos)
    {
        TileNode best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < tiles.Length; i++)
        {
            TileNode n = tiles[i];
            if (n == null) continue;

            float sqr = (n.WorldPosition - worldPos).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = n;
            }
        }

        return best;
    }
    public static List<EnemyGenerator> GetActiveGeneratorsSnapshot()
{
    List<EnemyGenerator> result = new List<EnemyGenerator>();

    for (int i = 0; i < _activeGenerators.Count; i++)
    {
        EnemyGenerator gen = _activeGenerators[i];
        if (gen == null) continue;
        if (gen._isDestroyed) continue;
        result.Add(gen);
    }

    return result;
}

    public static EnemyGenerator FindById(string generatorId)
    {
        if (string.IsNullOrEmpty(generatorId)) return null;

        for (int i = 0; i < _activeGenerators.Count; i++)
        {
            EnemyGenerator gen = _activeGenerators[i];
            if (gen == null) continue;
            if (gen._isDestroyed) continue;
            if (gen._generatorId == generatorId) return gen;
        }

        return null;
    }


}
