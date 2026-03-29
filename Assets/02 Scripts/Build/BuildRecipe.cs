using System;
using UnityEngine;

[Serializable]

public class BuildRecipe
{
    public string Id;                 // "torch", "campfire", "bed"
    public string DisplayName;        // "횃불", "모닥불", "침대"
    public GameObject Prefab;         // 배치할 프리팹
    public int BuildTurns = 2;        // 건설 완료까지 필요한 턴
    public ResourceCost[] Costs;      // 필요 자원
}
