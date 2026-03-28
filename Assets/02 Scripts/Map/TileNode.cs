using System.Collections.Generic;
using UnityEngine;

public class TileNode : MonoBehaviour
{
    [SerializeField] private int _index = -1;
    [SerializeField] private int _gridX;
    [SerializeField] private int _gridZ;
    [SerializeField] private string _coordId;

    [SerializeField] private List<TileNode> _neighbors = new List<TileNode>();

    public int Index => _index;
    public int GridX => _gridX;
    public int GridZ => _gridZ;
    public string CoordId => _coordId;
    public IReadOnlyList<TileNode> Neighbors => _neighbors;
    public Vector3 WorldPosition => transform.position;

    public void SetIndex(int index)
    {
        _index = index;
    }

    public void SetGridCoord(int x, int z)
    {
        _gridX = x;
        _gridZ = z;
        _coordId = $"{x},{z}";
    }

    public void ClearNeighbors()
    {
        _neighbors.Clear();
    }

    public void AddNeighbor(TileNode node)
    {
        if (node == null || node == this || _neighbors.Contains(node)) return;
        _neighbors.Add(node);
    }
}
