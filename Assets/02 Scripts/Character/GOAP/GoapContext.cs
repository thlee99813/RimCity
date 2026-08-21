using System.Collections.Generic;
using UnityEngine;

public class GoapContext
{
    public CharacterEntity Owner { get; }
    public CharacterTaskController TaskController { get; }
    public PlayerResourceInventory Inventory { get; }
    public BigTurnSelectionData Selection { get; }
    public List<TileNode> ActiveNodes { get; }
    public SmallTurnLogController Log { get; }
    public BuildRecipe[] BuildRecipes { get; }
    public CraftRecipe[] CraftRecipes { get; }
    public int SmallTurn { get; }
    public int MaxMoveTilesPerTurn { get; }
    public float BerryHungerRecoverAmount { get; }

    public GoapContext(
        CharacterEntity owner,
        CharacterTaskController taskController,
        BigTurnSelectionData selection,
        List<TileNode> activeNodes,
        SmallTurnLogController log,
        BuildRecipe[] buildRecipes,
        CraftRecipe[] craftRecipes,
        int smallTurn,
        int maxMoveTilesPerTurn,
        float berryHungerRecoverAmount)
    {
        Owner = owner;
        TaskController = taskController;
        Inventory = GameManager.Instance.PlayerInventory;
        Selection = selection;
        ActiveNodes = activeNodes;
        Log = log;
        BuildRecipes = buildRecipes;
        CraftRecipes = craftRecipes;
        SmallTurn = smallTurn;
        MaxMoveTilesPerTurn = Mathf.Max(1, maxMoveTilesPerTurn);
        BerryHungerRecoverAmount = berryHungerRecoverAmount;
    }

    public BuildRecipe FindBuildRecipe(string id)
    {
        if (BuildRecipes == null) return null;

        for (int i = 0; i < BuildRecipes.Length; i++)
        {
            BuildRecipe recipe = BuildRecipes[i];
            if (recipe != null && recipe.Id == id) return recipe;
        }

        return null;
    }

    public CraftRecipe FindCraftRecipe(string id)
    {
        if (CraftRecipes == null) return null;

        for (int i = 0; i < CraftRecipes.Length; i++)
        {
            CraftRecipe recipe = CraftRecipes[i];
            if (recipe != null && recipe.Id == id) return recipe;
        }

        return null;
    }

    public int GetMoveTurnCost(TileNode target)
    {
        List<TileNode> path = CharacterTaskCommon.FindPath(Owner.CurrentTileNode, target, ActiveNodes);
        if (path == null) return int.MaxValue;
        return Mathf.CeilToInt(path.Count / (float)MaxMoveTilesPerTurn);
    }
}
