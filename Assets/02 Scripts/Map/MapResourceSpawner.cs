using System.Collections.Generic;
using UnityEngine;

public class MapResourceSpawner : MonoBehaviour
{

    [Header("리소스 프리팹")]
    [SerializeField] private GameObject _berryPrefab;
    [SerializeField] private GameObject _treePrefab;
    [SerializeField] private GameObject _rockPrefab;

    [SerializeField] private GameObject _grassPrefab;

    [Header("부모/위치")]
    [SerializeField] private float _yOffset = 0f;

    [Header("타일별 확률")]
    [SerializeField] private int _noneWeight = 100;
    [SerializeField] private int _berryWeight = 10;
    [SerializeField] private int _treeWeight = 20;
    [SerializeField] private int _rockWeight = 10;

    [SerializeField] private int _grassWeight = 15;

    private readonly int[] _blockedTileNumbers = { 6, 7, 10, 11 };

    private void Start()
    {
        EventManager.Instance.AddListener(MEventType.Stageactivated, OnStageActivated);
    }
    private void OnDisable()
    {
        if (EventManager.Instance == null) return;

        EventManager.Instance.RemoveListener(MEventType.Stageactivated, this);
    }
    private void OnStageActivated(MEventType eventType, Component sender, System.EventArgs args)
    {
        StageActivatedEventArgs eventArgs = (StageActivatedEventArgs)args;
        RespawnByTileProbability(eventArgs.StageContext.TileNodes);
    }

    private void RespawnByTileProbability(TileNode[] tiles)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            int tileNumber = i + 1;
            if (IsBlockedTile(tileNumber)) continue;

            ResourceType spawnType = PickSpawnType();
            if (spawnType == ResourceType.None) continue;

            SpawnAtTile(tiles, i, spawnType);
        }
    }

    private bool IsBlockedTile(int tileNumber)
    {
        for (int i = 0; i < _blockedTileNumbers.Length; i++)
        {
            if (_blockedTileNumbers[i] == tileNumber)
                return true;
        }
        return false;
    }

    private ResourceType PickSpawnType()
    {
        int total = _noneWeight + _berryWeight + _treeWeight + _rockWeight + _grassWeight;
        int roll = Random.Range(0, total);

        if (roll < _noneWeight) return ResourceType.None;
        roll -= _noneWeight;

        if (roll < _berryWeight) return ResourceType.Berry;
        roll -= _berryWeight;

        if (roll < _treeWeight) return ResourceType.Tree;

        if (roll < _grassWeight) return ResourceType.Grass;
        return ResourceType.Rock;
    }

    private void SpawnAtTile(TileNode[] tiles, int tileIndex, ResourceType spawnType)
    {
        TileNode tile = tiles[tileIndex];
        tile.ClearResource();

        Vector3 spawnPos = tile.WorldPosition + new Vector3(0f, _yOffset, 0f);
        GameObject prefab = GetPrefab(spawnType);
        if (prefab == null) return;

        GameObject spawned = Instantiate(prefab, spawnPos, Quaternion.identity, tile.transform);
        ResourceNode spawnedResourceNode = spawned.GetComponent<ResourceNode>();
        if (spawnedResourceNode != null)
        spawnedResourceNode.Type = spawnType;

        tile.SetResource(spawnType, spawnedResourceNode);
    }

    private GameObject GetPrefab(ResourceType spawnType)
    {
        switch (spawnType)
        {
            case ResourceType.Berry: return _berryPrefab;
            case ResourceType.Tree: return _treePrefab;
            case ResourceType.Rock: return _rockPrefab;
            case ResourceType.Grass: return _grassPrefab;
            default: return null;
        }
    }

    

    
  
}
