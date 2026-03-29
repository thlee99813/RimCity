using UnityEngine;
using System;

public class PlayerResourceInventory : MonoBehaviour
{
    [SerializeField] private int _berry;
    [SerializeField] private int _tree;
    [SerializeField] private int _rock;

    public int Berry => _berry;
    public int Tree => _tree;
    public int Rock => _rock;

    public event Action OnChanged;


    public void Add(ResourceType type, int amount)
    {
        if (amount <= 0) return;

        bool changed = false;


        switch (type)
        {
            case ResourceType.Berry: _berry += amount; changed = true; break;
            case ResourceType.Tree: _tree += amount; changed = true; break;
            case ResourceType.Rock: _rock += amount; changed = true; break;
        }

        if (changed) OnChanged?.Invoke();

    }

    public bool Consume(ResourceType type, int amount)
    {
        if (amount <= 0) return false;
        bool consumed = false;

        switch (type)
        {
            case ResourceType.Berry:
                if (_berry < amount) return false;
                _berry -= amount;
                consumed = true;
                return true;

            case ResourceType.Tree:
                if (_tree < amount) return false;
                _tree -= amount;
                consumed = true;
                return true;

            case ResourceType.Rock:
                if (_rock < amount) return false;
                _rock -= amount;
                consumed = true;
                return true;
        }

        if (consumed) OnChanged?.Invoke();
        return consumed;
    }
}
