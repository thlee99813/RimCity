using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTaskController
{
    private const int ForcedSleepRestTurns = 3;
    private const float ForcedSleepRecoverAmount = 60f;
    private const int AllyCombatSupportRangeTurns = 2;

    private readonly int _maxMoveTilesPerTurn;
    private readonly BuildRecipe[] _buildRecipes;
    private readonly CraftRecipe[] _craftRecipes;

    private readonly CharacterGatherTask _gatherTask = new CharacterGatherTask();
    private readonly CharacterBuildTask _buildTask = new CharacterBuildTask();
    private readonly CharacterCraftTask _craftTask = new CharacterCraftTask();
    private readonly CharacterItemUseTask _itemUseTask = new CharacterItemUseTask();
    private readonly CharacterSocialTask _socialTask = new CharacterSocialTask();

    private int _forcedRestTurnsRemaining;

    public bool HasRunningTask =>
        _gatherTask.IsForced || _buildTask.IsForced || _craftTask.IsForced || _socialTask.IsForced ||
        _forcedRestTurnsRemaining > 0;

    public CharacterTaskState GatherState => _gatherTask.State;
    public CharacterTaskState BuildState => _buildTask.State;
    public CharacterTaskState CraftState => _craftTask.State;
    public CharacterTaskState SocialState => _socialTask.State;
    public bool IsForcedRestRunning => _forcedRestTurnsRemaining > 0;

    public CharacterTaskController(int maxMoveTilesPerTurn, BuildRecipe[] buildRecipes, CraftRecipe[] craftRecipes)
    {
        _maxMoveTilesPerTurn = Mathf.Max(1, maxMoveTilesPerTurn);
        _buildRecipes = buildRecipes;
        _craftRecipes = craftRecipes;
    }

    public IEnumerator RunGatherTurn(
        CharacterEntity owner,
        int smallTurn,
        List<TileNode> activeNodes,
        SmallTurnLogController logController,
        ResourceType preferredType)
    {
        yield return _gatherTask.RunTurn(
            owner, smallTurn, activeNodes, logController, _maxMoveTilesPerTurn, preferredType);
    }

    public IEnumerator RunBuildTurn(
        CharacterEntity owner,
        int smallTurn,
        List<TileNode> activeNodes,
        SmallTurnLogController logController,
        string recipeId)
    {
        yield return _buildTask.RunTurn(
            owner, smallTurn, activeNodes, logController, _buildRecipes, _maxMoveTilesPerTurn,
            owner.GetStatLevel(StatType.Build), recipeId);
    }

    public IEnumerator RunCraftTurn(
        CharacterEntity owner,
        int smallTurn,
        SmallTurnLogController logController,
        string recipeId)
    {
        yield return _craftTask.RunTurn(
            owner, smallTurn, logController, _craftRecipes, owner.GetStatLevel(StatType.Craft), recipeId);
    }

    public IEnumerator RunSocialTurn(
        CharacterEntity owner,
        int smallTurn,
        List<TileNode> activeNodes,
        SmallTurnLogController logController)
    {
        yield return _socialTask.RunTurn(
            owner, smallTurn, activeNodes, logController, _maxMoveTilesPerTurn,
            owner.GetStatLevel(StatType.Social));
    }

    public IEnumerator RunMoveToTileTurn(
        CharacterEntity owner,
        TileNode target,
        int smallTurn,
        List<TileNode> activeNodes,
        SmallTurnLogController logController,
        string purposeText)
    {
        if (target == null || !activeNodes.Contains(target)) yield break;
        if (owner.CurrentTileNode == target) yield break;

        List<TileNode> path = CharacterTaskCommon.FindPath(owner.CurrentTileNode, target, activeNodes);
        if (path == null || path.Count == 0) yield break;

        int moveCount = Mathf.Min(_maxMoveTilesPerTurn, path.Count);
        logController.AddLog(TextUtil.ApplyKoreanParticles(
            $"[{smallTurn} 턴] {owner.Data.Name}은/는 {purposeText} 이동합니다. ({moveCount}칸)"));

        for (int i = 0; i < moveCount; i++)
            yield return owner.MoveToTile(path[i]);
    }

    public void PrepareForcedRest(CharacterEntity owner)
    {
        if (owner.Status.Sleep > 0f || _forcedRestTurnsRemaining > 0) return;
        _forcedRestTurnsRemaining = ForcedSleepRestTurns;
    }

    public void RunRestTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        if (owner.CurrentTileNode != null &&
            CharacterTaskCommon.TryGetPlacedStructureType(owner.CurrentTileNode, out StructureType placedType) &&
            placedType == StructureType.Bed)
        {
            owner.Status.AddSleep(100f, owner.Data);
            logController.AddLog(TextUtil.ApplyKoreanParticles(
                $"[{smallTurn} 턴] {owner.Data.Name}은/는 침대에서 잠을 자고 수면을 회복합니다. (+100)"));
            return;
        }

        if (_forcedRestTurnsRemaining > 0)
        {
            _forcedRestTurnsRemaining--;
            logController.AddLog(TextUtil.ApplyKoreanParticles(
                $"[{smallTurn} 턴] {owner.Data.Name}은/는 강제로 잠을 잡니다."));

            if (_forcedRestTurnsRemaining == 0)
            {
                owner.Status.AddSleep(ForcedSleepRecoverAmount, owner.Data);
                logController.AddLog(TextUtil.ApplyKoreanParticles(
                    $"[{smallTurn} 턴] {owner.Data.Name}은/는 수면이 회복되었습니다. (+{ForcedSleepRecoverAmount})"));
            }
            return;
        }

        owner.Status.AddSleep(20f, owner.Data);
        logController.AddLog(TextUtil.ApplyKoreanParticles(
            $"[{smallTurn} 턴] {owner.Data.Name}은/는 휴식을 취합니다. (수면 +20)"));
    }

    public bool RunEatAction(
        CharacterEntity owner,
        int smallTurn,
        SmallTurnLogController logController,
        float berryHungerRecoverAmount)
    {
        return _itemUseTask.RunEatAction(owner, smallTurn, logController, berryHungerRecoverAmount);
    }

    public bool RunEquipWoodenSpearTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        return _itemUseTask.RunEquipWoodenSpearTurn(owner, smallTurn, logController);
    }

    public bool RunEquipStoneSpearTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        return _itemUseTask.RunEquipStoneSpearTurn(owner, smallTurn, logController);
    }

    public bool RunEquipFanTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        return _itemUseTask.RunEquipFanTurn(owner, smallTurn, logController);
    }

    public bool RunUseBandageTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        return _itemUseTask.RunUseBandageTurn(owner, smallTurn, logController);
    }

    public bool RunUseMedkitTurn(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        return _itemUseTask.RunUseMedkitTurn(owner, smallTurn, logController);
    }

    public bool TryRunForcedCombatTurn(
        CharacterEntity owner,
        int smallTurn,
        SmallTurnLogController logController,
        CharacterCombatTask combatTask)
    {
        EnemyEntity enemyOnTile = EnemyManager.Instance.GetEnemyOnTile(owner.CurrentTileNode);
        EnemyGenerator generatorOnTile = EnemyGenerator.FindOnTile(owner.CurrentTileNode);

        if (enemyOnTile == null && generatorOnTile == null) return false;

        if (enemyOnTile == null && generatorOnTile != null)
        {
            if (RaidDirectiveManager.Instance == null) return false;
            if (!RaidDirectiveManager.Instance.TryGetTarget(out EnemyGenerator raidTarget)) return false;
            if (generatorOnTile != raidTarget) return false;

            combatTask.RunAttackGenerator(owner, generatorOnTile, smallTurn, logController);
            return true;
        }

        AllyCombatSupportManager.Instance.ReportCombatTile(owner.CurrentTileNode);
        PlayerResourceInventory inventory = GameManager.Instance.PlayerInventory;

        if (owner.Status.Sleep <= 0f)
        {
            PrepareForcedRest(owner);
            RunRestTurn(owner, smallTurn, logController);
            combatTask.ApplyEnemyRetaliation(owner, enemyOnTile, smallTurn, logController);
            return true;
        }

        if (owner.Status.Health <= owner.Data.MaxHealth * 0.3f && inventory.Medkit > 0)
        {
            RunUseMedkitTurn(owner, smallTurn, logController);
            combatTask.ApplyEnemyRetaliation(owner, enemyOnTile, smallTurn, logController);
            return true;
        }

        if (owner.Status.Health <= owner.Data.MaxHealth * 0.5f && inventory.Bandage > 0)
        {
            RunUseBandageTurn(owner, smallTurn, logController);
            combatTask.ApplyEnemyRetaliation(owner, enemyOnTile, smallTurn, logController);
            return true;
        }

        combatTask.RunAttackExchange(owner, enemyOnTile, smallTurn, logController);
        return true;
    }

    public TileNode FindNearestEnemyTile(TileNode from, List<TileNode> activeNodes, int maxSteps)
    {
        if (from == null || activeNodes == null) return null;

        TileNode best = null;
        int bestSteps = int.MaxValue;

        for (int i = 0; i < activeNodes.Count; i++)
        {
            TileNode tile = activeNodes[i];
            if (tile == null) continue;

            EnemyEntity enemy = EnemyManager.Instance.GetEnemyOnTile(tile);
            if (enemy == null || enemy.IsDead) continue;

            List<TileNode> path = CharacterTaskCommon.FindPath(from, tile, activeNodes);
            if (path == null || path.Count > maxSteps) continue;

            if (path.Count < bestSteps)
            {
                bestSteps = path.Count;
                best = tile;
            }
        }

        return best;
    }

    public TileNode FindNearestAllyCombatTile(TileNode from, List<TileNode> activeNodes)
    {
        return AllyCombatSupportManager.Instance.FindNearestCombatTile(
            from, activeNodes, _maxMoveTilesPerTurn * AllyCombatSupportRangeTurns);
    }

    public bool TryGetRaidTargetTile(List<TileNode> activeNodes, out TileNode targetTile)
    {
        targetTile = null;
        if (RaidDirectiveManager.Instance == null) return false;
        if (!RaidDirectiveManager.Instance.TryGetTarget(out EnemyGenerator targetGenerator)) return false;
        if (targetGenerator.GeneratorTile == null) return false;
        if (!activeNodes.Contains(targetGenerator.GeneratorTile)) return false;

        targetTile = targetGenerator.GeneratorTile;
        return true;
    }
}
