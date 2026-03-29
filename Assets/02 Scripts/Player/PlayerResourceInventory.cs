using UnityEngine;
using System;

public class PlayerResourceInventory : MonoBehaviour
{
    [SerializeField] private int _berry;
    [SerializeField] private int _tree;
    [SerializeField] private int _rock;
    [SerializeField] private int _fiber;

    [SerializeField] private int _cloth;
    [SerializeField] private int _woodenSpear;
    [SerializeField] private int _stoneSpear;
    [SerializeField] private int _fan;
    [SerializeField] private int _bandage;
    [SerializeField] private int _medkit;

    public int Berry => _berry;
    public int Tree => _tree;
    public int Rock => _rock;

    public int Fiber => _fiber;
    public int Cloth => _cloth;
    public int WoodenSpear => _woodenSpear;
    public int StoneSpear => _stoneSpear;
    public int Fan => _fan;
    public int Bandage => _bandage;
    public int Medkit => _medkit;

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
            case ResourceType.Grass: _fiber += amount; changed = true; break;

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
                break;

            case ResourceType.Tree:
                if (_tree < amount) return false;
                _tree -= amount;
                consumed = true;
                break;

            case ResourceType.Rock:
                if (_rock < amount) return false;
                _rock -= amount;
                consumed = true;
                break;
            case ResourceType.Grass:
                if(_fiber < amount) return false;
                _fiber -= amount;
                consumed = true;
                break;
        }

        if (consumed) OnChanged?.Invoke();
        return consumed;
    }

    public int GetAmount(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Berry: return _berry;
            case ResourceType.Tree: return _tree;
            case ResourceType.Rock: return _rock;
            case ResourceType.Grass: return _fiber;
            default: return 0;
        }
    }

    public void AddItem(string itemId, int amount)
    {
        if (amount <= 0) return;
        bool changed = false;

        if (itemId == ItemIds.Cloth) { _cloth += amount; changed = true; }
        else if (itemId == ItemIds.Fan) { _fan += amount; changed = true; }
        else if (itemId == ItemIds.Bandage) { _bandage += amount; changed = true; }
        else if (itemId == ItemIds.Medkit) { _medkit += amount; changed = true; }
        else if (itemId == ItemIds.WoodenSpear) { _woodenSpear += amount; changed = true; }
        else if (itemId == ItemIds.StoneSpear) { _stoneSpear += amount; changed = true; }

        if (changed) OnChanged?.Invoke();
    }

    public void AddWeapon(WeaponType type, int amount)
    {
        if (amount <= 0) return;

        if (type == WeaponType.WoodenSpear) _woodenSpear += amount;
        else if (type == WeaponType.StoneSpear) _stoneSpear += amount;

        OnChanged?.Invoke();
    }
    public bool ConsumeItem(string itemId, int amount)
    {
        if (amount <= 0) return false;
        bool consumed = false;

        if (itemId == ItemIds.Cloth)
        {
            if (_cloth < amount) return false;
            _cloth -= amount; consumed = true;
        }
        else if (itemId == ItemIds.WoodenSpear)
        {
            if (_woodenSpear < amount) return false;
            _woodenSpear -= amount; consumed = true;
        }
        else if (itemId == ItemIds.StoneSpear)
        {
            if (_stoneSpear < amount) return false;
            _stoneSpear -= amount; consumed = true;
        }
        else if (itemId == ItemIds.Fan)
        {
            if (_fan < amount) return false;
            _fan -= amount; consumed = true;
        }
        else if (itemId == ItemIds.Bandage)
        {
            if (_bandage < amount) return false;
            _bandage -= amount; consumed = true;
        }
        else if (itemId == ItemIds.Medkit)
        {
            if (_medkit < amount) return false;
            _medkit -= amount; consumed = true;
        }

        if (consumed) OnChanged?.Invoke();
        return consumed;
    }

}
