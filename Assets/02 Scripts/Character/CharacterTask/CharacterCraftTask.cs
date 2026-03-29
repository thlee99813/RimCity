using System.Collections;
using UnityEngine;

public class CharacterCraftTask
{
    public bool IsForced { get; private set; }

    private CraftRecipe _recipe;
    private int _turnsRemaining;

    public IEnumerator RunTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController log, CraftRecipe[] recipes)
    {
        if (!IsForced)
        {
            if (recipes == null || recipes.Length == 0)
            {
                log.AddLog($"[{smallTurn} 턴] 제작 레시피가 없습니다.");
                yield break;
            }

            _recipe = recipes[Random.Range(0, recipes.Length)];
            if (_recipe == null)
            {
                log.AddLog($"[{smallTurn} 턴] 제작 레시피가 비어 있습니다.");
                yield break;
            }

            PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
            if (!CharacterTaskCommon.CanAfford(inv, _recipe.Costs))
            {
                log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 재료가 부족합니다.");
                yield break;
            }

            CharacterTaskCommon.ConsumeCosts(inv, _recipe.Costs);

            _turnsRemaining = Mathf.Max(1, _recipe.CraftTurns);
            IsForced = true;
        }

        _turnsRemaining--;

        if (_turnsRemaining > 0)
        {
            log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName} 제작 중입니다. ({_turnsRemaining}턴 남음)");
            yield break;
        }

        ApplyCraftResult(owner, GameManager.Instance.PlayerInventory, _recipe.Id);
        log.AddLog($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName} 제작을 완료했습니다.");

        Clear();
    }

    private void ApplyCraftResult(CharacterEntity owner, PlayerResourceInventory inv, string itemId)
    {
        if (itemId == ItemIds.Cloth)
        {
            if (!owner.Equipment.TryEquipArmor(ArmorType.Cloth))
                inv.AddItem(itemId, 1);
            return;
        }

        if (itemId == ItemIds.WoodenSpear)
        {
            owner.EquipWeaponWithSwap(WeaponType.WoodenSpear, inv);
            return;
        }

        if (itemId == ItemIds.StoneSpear)
        {
            owner.EquipWeaponWithSwap(WeaponType.StoneSpear, inv);
            return;
        }

        if (itemId == ItemIds.Fan)
        {
            if (!owner.Equipment.TryEquipUtility(UtilityType.Fan))
                inv.AddItem(itemId, 1);
            return;
        }

        if (itemId == ItemIds.Bandage || itemId == ItemIds.Medkit)
        {
            inv.AddItem(itemId, 1);
        }
    }

    private void Clear()
    {
        IsForced = false;
        _recipe = null;
        _turnsRemaining = 0;
    }
}
