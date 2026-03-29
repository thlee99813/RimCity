using System;
using UnityEngine;

[Serializable]
public class CraftRecipe
{
    public string Id;              
    public string DisplayName;     
    public int CraftTurns = 1;
    public ResourceCost[] Costs;
}
