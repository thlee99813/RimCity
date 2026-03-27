using System.Collections.Generic;
using UnityEngine;

public class NameplatePool : MonoBehaviour
{
    [SerializeField] private NameplateUI _nameplatePrefab;
    [SerializeField] private Transform _poolRoot;
    [SerializeField] private int _prewarmCount = 20;

    private readonly Queue<NameplateUI> _pool = new();

    private void Awake()
    {
        for (int i = 0; i < _prewarmCount; i++)
        {
            NameplateUI ui = Instantiate(_nameplatePrefab, _poolRoot);
            ui.gameObject.SetActive(false);
            _pool.Enqueue(ui);
        }
    }

    public NameplateUI Get()
    {
        if (_pool.Count > 0)
        {
            NameplateUI ui = _pool.Dequeue();
            ui.gameObject.SetActive(true);
            return ui;
        }

        return Instantiate(_nameplatePrefab, _poolRoot);
    }

    public void Release(NameplateUI ui)
    {
        ui.gameObject.SetActive(false);
        _pool.Enqueue(ui);
    }
}
