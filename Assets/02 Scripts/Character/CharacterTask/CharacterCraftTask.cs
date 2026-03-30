using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class CharacterCraftTask
{
    public bool IsForced { get; private set; }

    private CraftRecipe _recipe;
    private int _turnsRemaining;

    public bool LastFailedByMissingResource { get; private set; }
    public ResourceType LastMissingResourceType { get; private set; } = ResourceType.None;


    public IEnumerator RunTurn(
        CharacterEntity owner,
        int smallTurn,
        SmallTurnLogController log,
        CraftRecipe[] recipes,
        int craftLevel,
        string forcedRecipeId = null)    
    {
        if (!IsForced)
        {
            LastFailedByMissingResource = false;
            LastMissingResourceType = ResourceType.None;

            if (recipes == null || recipes.Length == 0) yield break;

            PlayerResourceInventory inv = GameManager.Instance.PlayerInventory;
            _recipe = PickStartRecipe(recipes, inv, forcedRecipeId, craftLevel);

            if (_recipe == null)
            {
                if (!string.IsNullOrEmpty(forcedRecipeId))
                {
                    CraftRecipe desired = FindRecipeById(recipes, forcedRecipeId);
                    if (desired != null)
                    {
                        ResourceType miss = CharacterTaskCommon.GetFirstMissingResource(inv, desired.Costs);
                        if (miss != ResourceType.None)
                        {
                            LastFailedByMissingResource = true;
                            LastMissingResourceType = miss;
                        }
                    }
                }
                log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 제작 가능한 작업이 없습니다."));

                yield break;
            }
            CharacterTaskCommon.ConsumeCosts(inv, _recipe.Costs);
            _turnsRemaining = Mathf.Max(1, _recipe.CraftTurns);
            IsForced = true;

        }

        _turnsRemaining--;

        if (_turnsRemaining > 0)
        {
            log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName} 제작 중입니다. ({_turnsRemaining}턴 남음)"));
            yield break;
        }

        int craftFailChance = GetLowSkillFailChance(craftLevel, _recipe.RecommendedCraftLevel);
        if (craftFailChance > 0 && Random.Range(0, 100) < craftFailChance)
        {
            log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName}을 제작하다가 손이 삐끗했습니다."));
            Clear();
            yield break;
        }

        ApplyCraftResult(owner, GameManager.Instance.PlayerInventory, _recipe.Id);

        log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_recipe.DisplayName} 제작을 완료했습니다."));
        owner.AddStatActionCount(StatType.Craft, 1, smallTurn, log);

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
    private CraftRecipe FindRecipeById(CraftRecipe[] recipes, string id)
    {
        for (int i = 0; i < recipes.Length; i++)
        {
            CraftRecipe r = recipes[i];
            if (r == null) continue;
            if (r.Id == id) return r;
        }
        return null;
    }

    private CraftRecipe PickStartRecipe(CraftRecipe[] recipes, PlayerResourceInventory inv, string forcedRecipeId, int craftLevel)
    {
        craftLevel = Mathf.Clamp(craftLevel, 1, 11);

        if (!string.IsNullOrEmpty(forcedRecipeId))
        {
            CraftRecipe forced = FindRecipeById(recipes, forcedRecipeId);
            if (forced == null) return null;
            if (craftLevel < Mathf.Max(1, forced.RequiredCraftLevel)) return null;
            if (!CharacterTaskCommon.CanAfford(inv, forced.Costs)) return null;
            return forced;
        }

        List<CraftRecipe> candidates = new List<CraftRecipe>();
        List<int> weights = new List<int>();
        int total = 0;

        for (int i = 0; i < recipes.Length; i++)
        {
            CraftRecipe r = recipes[i];
            if (r == null) continue;
            if (craftLevel < Mathf.Max(1, r.RequiredCraftLevel)) continue;
            if (!CharacterTaskCommon.CanAfford(inv, r.Costs)) continue;

            int w = 1;

            if (r.Id == ItemIds.Bandage) w += (inv.Bandage <= 0 ? 7 : 2);
            else if (r.Id == ItemIds.Medkit) w += (inv.Medkit <= 0 ? 9 : 2);


            candidates.Add(r);
            weights.Add(w);
            total += w;
        }

        if (candidates.Count == 0) return null;

        int roll = Random.Range(0, total);
        int acc = 0;
        for (int i = 0; i < candidates.Count; i++)
        {
            acc += weights[i];
            if (roll < acc) return candidates[i];
        }

        return candidates[0];
    }
    private int GetLowSkillFailChance(int currentLevel, int recommendedLevel)
    {
        int rec = Mathf.Max(1, recommendedLevel);
        int lv = Mathf.Clamp(currentLevel, 1, 11);
        int gap = rec - lv;

        if (gap >= 2) return 60;
        if (gap == 1) return 30;
        return 0;
    }


    }
