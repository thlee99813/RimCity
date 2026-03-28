using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileNeighborBakerWindow : EditorWindow
{
    private Transform _root;
    private bool _includeInactive = true;

    [MenuItem("Tools/RimCity/Bake Tile Neighbors")]
    private static void Open()
    {
        GetWindow<TileNeighborBakerWindow>("Tile Neighbor Baker");
    }

    private void OnGUI()
    {
        _root = (Transform)EditorGUILayout.ObjectField("Tile Root", _root, typeof(Transform), true);
        _includeInactive = EditorGUILayout.Toggle("Include Inactive", _includeInactive);

        if (GUILayout.Button("Bake (4-direction by GridX/GridZ)"))
        {
            Bake();
        }
    }

    private void Bake()
    {
        if (_root == null) return;

        TileNode[] nodes = _root.GetComponentsInChildren<TileNode>(_includeInactive);
        Dictionary<Vector2Int, TileNode> map = new Dictionary<Vector2Int, TileNode>(nodes.Length);

        for (int i = 0; i < nodes.Length; i++)
        {
            Vector2Int key = new Vector2Int(nodes[i].GridX, nodes[i].GridZ);
            map[key] = nodes[i];
        }

        Undo.RecordObjects(nodes, "Bake Tile Neighbors");

        Vector2Int[] dirs =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        for (int i = 0; i < nodes.Length; i++)
        {
            TileNode node = nodes[i];
            node.ClearNeighbors();

            Vector2Int p = new Vector2Int(node.GridX, node.GridZ);

            for (int d = 0; d < dirs.Length; d++)
            {
                Vector2Int np = p + dirs[d];
                if (map.TryGetValue(np, out TileNode neighbor))
                {
                    node.AddNeighbor(neighbor);
                }
            }

            EditorUtility.SetDirty(node);
        }

        Debug.Log($"Neighbor Bake 완료: {nodes.Length} tiles");
    }
}
