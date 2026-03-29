using System.Collections.Generic;
using UnityEngine;

public class TileNode : MonoBehaviour
{
    [SerializeField] private int _index = -1;
    [SerializeField] private int _gridX;
    [SerializeField] private int _gridZ;
    [SerializeField] private string _coordId;

    [SerializeField] private List<TileNode> _neighbors = new List<TileNode>();

    [Header("Resource")]
    [SerializeField] private ResourceType _resourceType = ResourceType.None;
    [SerializeField] private ResourceNode _resourceNode;

    [SerializeField] private GameObject _placedStructure;
    public bool IsOccupied => _placedStructure != null;
    public GameObject PlacedStructure => _placedStructure;

    public int Index => _index;
    public int GridX => _gridX;
    public int GridZ => _gridZ;
    public string CoordId => _coordId;
    public IReadOnlyList<TileNode> Neighbors => _neighbors;
    public Vector3 WorldPosition => transform.position;

    public ResourceType ResourceTypeOnTile => _resourceType;
    public ResourceNode ResourceNodeOnTile => _resourceNode;
    public bool HasResource => _resourceType != ResourceType.None && _resourceNode != null;

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
    public void SetResource(ResourceType type, ResourceNode node)
    {
        _resourceType = type;
        _resourceNode = node;
    }

    public void ClearResource()
    {
        _resourceType = ResourceType.None;
        _resourceNode = null;
    }

    public void SetStructure(GameObject structure)
    {
        _placedStructure = structure;
    }

    public void ClearStructure()
    {
        _placedStructure = null;
    }
}
