using UnityEngine;
using System.Collections.Generic;
using System;
public class StageContext : MonoBehaviour
{
    [SerializeField] private TileNode[] _tileNodes;
    public TileNode[] TileNodes => _tileNodes;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        GameManager.Instance.StageActive(this);
    }

    private void OnDisable()
    {
        GameManager.Instance.UnregisterStage(this);
    }
    
}

