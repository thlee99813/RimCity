using UnityEditor;
using UnityEngine;

public class PrefabGridPlacerWindow : EditorWindow
{
    private GameObject _prefab;
    private Transform _parent;
    private Vector3 _origin = Vector3.zero;

    private int _countX = 4;
    private int _countZ = 4;

    private float _spacingX = 4f;
    private float _spacingZ = 4f;

    [MenuItem("Tools/RimCity/Prefab Grid Placer")]
    private static void Open()
    {
        GetWindow<PrefabGridPlacerWindow>("Prefab Grid Placer");
    }

    private void OnGUI()
    {
        _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);
        _parent = (Transform)EditorGUILayout.ObjectField("Parent", _parent, typeof(Transform), true);
        _origin = EditorGUILayout.Vector3Field("Origin", _origin);

        EditorGUILayout.Space();

        _countX = EditorGUILayout.IntField("Count X", _countX);
        _countZ = EditorGUILayout.IntField("Count Z", _countZ);

        _spacingX = EditorGUILayout.FloatField("Spacing X", _spacingX);
        _spacingZ = EditorGUILayout.FloatField("Spacing Z", _spacingZ);

        EditorGUILayout.Space();

        if (GUILayout.Button("Place Grid"))
        {
            PlaceGrid();
        }

        if (GUILayout.Button("Clear Children"))
        {
            ClearChildren();
        }
    }

    private void PlaceGrid()
    {
        for (int z = 0; z < _countZ; z++)
        {
            for (int x = 0; x < _countX; x++)
            {
                Vector3 pos = _origin + new Vector3(x * _spacingX, 0f, z * _spacingZ);

                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
                Undo.RegisterCreatedObjectUndo(obj, "Place Grid Prefab");

                obj.transform.position = pos;
                if (_parent != null) obj.transform.SetParent(_parent, true);
                obj.name = $"{_prefab.name}_{z}_{x}";
            }
        }
    }

    private void ClearChildren()
    {
        if (_parent == null) return;

        for (int i = _parent.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(_parent.GetChild(i).gameObject);
        }
    }
}
