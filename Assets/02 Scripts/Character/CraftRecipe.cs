using System;
using UnityEngine;

[Serializable]
public class CraftRecipe
{
    public string Id;              
    public string DisplayName;     
    public int CraftTurns = 1;
    public ResourceCost[] Costs;

    public int RequiredCraftLevel = 1;      // 최소 제작 레벨
    public int RecommendedCraftLevel = 1;   // 추천 제작 레벨

}
