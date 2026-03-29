using System;
using UnityEngine;

public enum ArmorType
{
    None,
    Cloth
}

public enum WeaponType
{
    None,
    WoodenSpear,
    StoneSpear
}

public enum UtilityType
{
    None,
    Fan
}

[Serializable]
public class CharacterEquipment
{
    [SerializeField] private ArmorType _armor = ArmorType.None;
    [SerializeField] private WeaponType _weapon = WeaponType.None;
    [SerializeField] private UtilityType _utility = UtilityType.None;

    public ArmorType Armor => _armor;
    public WeaponType Weapon => _weapon;
    public UtilityType Utility => _utility;

    public bool TryEquipArmor(ArmorType type)
    {
        if (_armor != ArmorType.None) return false;
        _armor = type;
        return true;
    }

    public bool TryEquipWeapon(WeaponType type)
    {
        if (_weapon != WeaponType.None) return false;
        _weapon = type;
        return true;
    }

    public bool TryEquipUtility(UtilityType type)
    {
        if (_utility != UtilityType.None) return false;
        _utility = type;
        return true;
    }

    public float GetCombatMultiplier()
    {
        switch (_weapon)
        {
            case WeaponType.WoodenSpear: return 1.5f;
            case WeaponType.StoneSpear: return 2.0f;
            default: return 1f;
        }
    }

    public float GetHeatDamageMultiplier()
    {
        return _utility == UtilityType.Fan ? 0.7f : 1f;
    }

    public float GetColdDamageMultiplier()
    {
        return _armor == ArmorType.Cloth ? 0.7f : 1f;
    }

    public void UnequipArmor() { _armor = ArmorType.None; }
    public void UnequipWeapon() { _weapon = WeaponType.None; }
    public void UnequipUtility() { _utility = UtilityType.None; }
}
