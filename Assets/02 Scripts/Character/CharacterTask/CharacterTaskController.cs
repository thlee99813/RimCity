using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTaskController
{
    private readonly int _maxMoveTilesPerTurn;
    private readonly BuildRecipe[] _buildRecipes;
    private readonly CraftRecipe[] _craftRecipes;

    private readonly CharacterGatherTask _gatherTask = new CharacterGatherTask();
    private readonly CharacterBuildTask _buildTask = new CharacterBuildTask();
    private readonly CharacterCraftTask _craftTask = new CharacterCraftTask();
    private readonly CharacterItemUseTask _itemUseTask = new CharacterItemUseTask();

    public CharacterTaskController(int maxMoveTilesPerTurn, BuildRecipe[] buildRecipes, CraftRecipe[] craftRecipes)
    {
        _maxMoveTilesPerTurn = Mathf.Max(1, maxMoveTilesPerTurn);
        _buildRecipes = buildRecipes;
        _craftRecipes = craftRecipes;
    }

    public SmallTurnActionType ResolveAction(
        CharacterData data,
        CharacterStatus status,
        CharacterEquipment equipment,
        BigTurnSelectionData selection,
        CharacterBrain brain)
    {
        if (_gatherTask.IsForced) return SmallTurnActionType.Gather;
        if (_buildTask.IsForced) return SmallTurnActionType.Build;
        if (_craftTask.IsForced) return SmallTurnActionType.Craft;

        return brain.DecideSmallTurnAction(data, status, equipment, GameManager.Instance.PlayerInventory, selection);
    }

    public IEnumerator RunGatherTurn(CharacterEntity owner, int smallTurn, List<TileNode> activeNodes, SmallTurnLogController logController)
    {
        yield return _gatherTask.RunTurn(owner, smallTurn, activeNodes, logController, _maxMoveTilesPerTurn);
    }

    public IEnumerator RunBuildTurn(CharacterEntity owner, int smallTurn, List<TileNode> activeNodes, SmallTurnLogController logController)
    {
        yield return _buildTask.RunTurn(owner, smallTurn, activeNodes, logController, _buildRecipes, _maxMoveTilesPerTurn);
    }

    public IEnumerator RunCraftTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        yield return _craftTask.RunTurn(owner, smallTurn, logController, _craftRecipes);
    }

    public void RunEatAction(CharacterEntity owner, int smallTurn, SmallTurnLogController logController, float berryHungerRecoverAmount)
    {
        _itemUseTask.RunEatAction(owner, smallTurn, logController, berryHungerRecoverAmount);
    }

    public void RunEquipWoodenSpearTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        _itemUseTask.RunEquipWoodenSpearTurn(owner, smallTurn, logController);
    }

    public void RunEquipStoneSpearTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        _itemUseTask.RunEquipStoneSpearTurn(owner, smallTurn, logController);
    }

    public void RunEquipFanTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        _itemUseTask.RunEquipFanTurn(owner, smallTurn, logController);
    }

    public void RunUseBandageTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        _itemUseTask.RunUseBandageTurn(owner, smallTurn, logController);
    }

    public void RunUseMedkitTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        _itemUseTask.RunUseMedkitTurn(owner, smallTurn, logController);
    }
}
