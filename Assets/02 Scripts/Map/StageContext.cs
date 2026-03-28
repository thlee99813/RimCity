using UnityEngine;
using System.Collections.Generic;
using System;
public class StageContext : MonoBehaviour
{
    [SerializeField] private Transform[] _tiles;
    public Transform[] Tiles => _tiles;

    private void OnEnable()
    {
        GameManager.Instance.StageActive(this);
    }
    private void OnDisable()
    {
        GameManager.Instance.UnregisterStage(this);
    }
    public Transform GetNearestTile(Vector3 worldPosition)
    {
        Transform best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < _tiles.Length; i++)
        {
            float sqr = (_tiles[i].position - worldPosition).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = _tiles[i];
            }
        }

        return best;
    }
    
    public Transform GetRandomNeighborTile(Transform currentTile)
    {
        int index = Array.IndexOf(_tiles, currentTile);
        if (index < 0) return currentTile;

        int row = index / 4;
        int col = index % 4;

        List<int> neighbors = new List<int>();

        if (row > 0) neighbors.Add((row - 1) * 4 + col); // 위
        if (row < 3) neighbors.Add((row + 1) * 4 + col); // 아래
        if (col > 0) neighbors.Add(row * 4 + (col - 1)); // 왼
        if (col < 3) neighbors.Add(row * 4 + (col + 1)); // 오른

        if (neighbors.Count == 0) return currentTile;

        int picked = neighbors[UnityEngine.Random.Range(0, neighbors.Count)];
        return _tiles[picked];
    }
}

