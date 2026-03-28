using UnityEditor;
using UnityEngine;

public class TileIdAssignerWindow : EditorWindow
{
    private Transform _root;
    private Transform _originTile;   // (0,0)
    private Transform _xAxisTile;    // (1,0) 위치 타일
    private Transform _zAxisTile;    // (0,1) 위치 타일
    private int _width = 48;
    private bool _idStartAtOne = true;

    [MenuItem("Tools/RimCity/Assign Tile Grid ID")]
    private static void Open()
    {
        GetWindow<TileIdAssignerWindow>("Tile ID Assigner");
    }

    private void OnGUI()
    {
        _root = (Transform)EditorGUILayout.ObjectField("Tile Root", _root, typeof(Transform), true);
        _originTile = (Transform)EditorGUILayout.ObjectField("Origin Tile (0,0)", _originTile, typeof(Transform), true);
        _xAxisTile = (Transform)EditorGUILayout.ObjectField("X Axis Tile (1,0)", _xAxisTile, typeof(Transform), true);
        _zAxisTile = (Transform)EditorGUILayout.ObjectField("Z Axis Tile (0,1)", _zAxisTile, typeof(Transform), true);
        _width = EditorGUILayout.IntField("Grid Width (X)", _width);
        _idStartAtOne = EditorGUILayout.Toggle("ID Start At 1", _idStartAtOne);

        if (GUILayout.Button("Assign"))
            AssignByBasis();
    }

    private void AssignByBasis()
    {
        if (_root == null || _originTile == null || _xAxisTile == null || _zAxisTile == null || _width <= 0)
            return;

        TileNode[] nodes = _root.GetComponentsInChildren<TileNode>(true);
        Undo.RecordObjects(nodes, "Assign Tile IDs");

        Vector3 o3 = _root.InverseTransformPoint(_originTile.position);
        Vector3 x3 = _root.InverseTransformPoint(_xAxisTile.position) - o3;
        Vector3 z3 = _root.InverseTransformPoint(_zAxisTile.position) - o3;

        Vector2 bx = new Vector2(x3.x, x3.z); // +X 축 벡터
        Vector2 bz = new Vector2(z3.x, z3.z); // +Z 축 벡터

        float det = bx.x * bz.y - bx.y * bz.x;
        if (Mathf.Abs(det) < 0.0001f)
        {
            Debug.LogError("X/Z 축 타일이 잘못 지정됨(두 축이 거의 평행).");
            return;
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            TileNode node = nodes[i];
            Vector3 p3 = _root.InverseTransformPoint(node.transform.position) - o3;
            Vector2 p = new Vector2(p3.x, p3.z);

            // p = a*bx + b*bz  -> a,b 계산
            float a = (p.x * bz.y - p.y * bz.x) / det; // gridX
            float b = (bx.x * p.y - bx.y * p.x) / det; // gridZ

            int gridX = Mathf.RoundToInt(a);
            int gridZ = Mathf.RoundToInt(b);

            int id = gridZ * _width + gridX + (_idStartAtOne ? 1 : 0);

            node.SetGridCoord(gridX, gridZ);
            node.SetIndex(id);

            EditorUtility.SetDirty(node);
        }

        Debug.Log($"할당 완료: {nodes.Length}개");
    }
}
