using UnityEngine;

public class StageContext : MonoBehaviour
{
    [SerializeField] private Transform[] _tiles; // 16개
    public Transform[] Tiles => _tiles;

    private void OnEnable()
    {
        GameManager.Instance.StageActive(this);
    }
}
