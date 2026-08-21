using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGoapController
{
    private const int EnemySeekRangeTurns = 1;
    private const int EnemySeekChanceDefault = 6;
    private const int EnemySeekChanceCombatFirst = 30;

    private readonly CharacterTaskController _taskController;
    private readonly int _maxMoveTilesPerTurn;
    private readonly BuildRecipe[] _buildRecipes;
    private readonly CraftRecipe[] _craftRecipes;
    private readonly GoapPlanner _planner = new GoapPlanner();
    private readonly CharacterWorldStateBuilder _worldStateBuilder = new CharacterWorldStateBuilder();
    private readonly CharacterGoapActionSet _actionSet = new CharacterGoapActionSet();
    private readonly CharacterGoalSelector _goalSelector;

    private GoapPlan _currentPlan;
    private PolicyType _plannedPolicy;
    private WeatherType _plannedWeather;
    private WorldEventType _plannedEvent;
    private string _plannedRaidTargetId;

    public CharacterGoalType? CurrentGoal => _currentPlan?.Goal.Type;
    public string CurrentActionName => _currentPlan?.CurrentAction?.Name;

    public CharacterGoapController(
        CharacterTaskController taskController,
        CharacterBrain brain,
        int maxMoveTilesPerTurn,
        BuildRecipe[] buildRecipes,
        CraftRecipe[] craftRecipes)
    {
        _taskController = taskController;
        _goalSelector = new CharacterGoalSelector(brain);
        _maxMoveTilesPerTurn = Mathf.Max(1, maxMoveTilesPerTurn);
        _buildRecipes = buildRecipes;
        _craftRecipes = craftRecipes;
    }

    public IEnumerator RunTurn(
        CharacterEntity owner,
        BigTurnSelectionData selection,
        int smallTurn,
        List<TileNode> activeNodes,
        SmallTurnLogController log,
        float berryHungerRecoverAmount)
    {
        GoapContext context = new GoapContext(
            owner,
            _taskController,
            selection,
            activeNodes,
            log,
            _buildRecipes,
            _craftRecipes,
            smallTurn,
            _maxMoveTilesPerTurn,
            berryHungerRecoverAmount);

        if (TryGetMovementInterrupt(context, out TileNode interruptTarget, out string purposeText))
        {
            yield return _taskController.RunMoveToTileTurn(
                owner, interruptTarget, smallTurn, activeNodes, log, purposeText);
            yield break;
        }

        bool planOwnsForcedRest = _currentPlan?.CurrentAction?.Name == "Rest" &&
                                  _currentPlan.CurrentAction.Status == GoapActionStatus.Running;

        if (_taskController.IsForcedRestRunning && !planOwnsForcedRest)
        {
            _taskController.RunRestTurn(owner, smallTurn, log);
            yield break;
        }

        if (!_taskController.HasRunningTask &&
            _taskController.TryGetRaidTargetTile(activeNodes, out TileNode raidTarget) &&
            raidTarget != owner.CurrentTileNode)
        {
            yield return _taskController.RunMoveToTileTurn(
                owner, raidTarget, smallTurn, activeNodes, log, "적 생성기를 공격하기 위해");
            yield break;
        }

        if (!_taskController.HasRunningTask && HasSelectionChanged(selection))
            InvalidatePlan();

        for (int attempt = 0; attempt < 2; attempt++)
        {
            GoapWorldState currentState = _worldStateBuilder.Build(context);

            if (_currentPlan == null && !TryCreatePlan(currentState, context))
            {
                yield return RunFallback(context);
                yield break;
            }

            if (_currentPlan.IsComplete)
            {
                InvalidatePlan();
                continue;
            }

            IGoapAction action = _currentPlan.CurrentAction;

            if (action.Status == GoapActionStatus.Running)
            {
                if (!action.CanContinue(context))
                {
                    InvalidatePlan();
                    continue;
                }
            }
            else if (!action.ArePreconditionsMet(currentState, context) || !action.CanStart(context))
            {
                InvalidatePlan();
                continue;
            }

            yield return action.ExecuteTurn(context);
            HandleActionResult(context, action);
            yield break;
        }

        yield return RunFallback(context);
    }

    private bool TryCreatePlan(GoapWorldState state, GoapContext context)
    {
        List<GoapGoal> goals = _goalSelector.GetPlanningOrder(state, context);

        for (int i = 0; i < goals.Count; i++)
        {
            List<IGoapAction> actions = _actionSet.Create(context, goals[i]);
            GoapPlan plan = _planner.CreatePlan(state, goals[i], actions, context);
            if (plan == null || plan.IsComplete) continue;

            _currentPlan = plan;
            StoreSelection(context.Selection);
            _currentPlan.CurrentAction.ResetRuntimeState();
            return true;
        }

        return false;
    }

    private void HandleActionResult(GoapContext context, IGoapAction action)
    {
        if (action.Status == GoapActionStatus.Running) return;

        if (action.Status == GoapActionStatus.Failure)
        {
            InvalidatePlan();
            return;
        }

        _currentPlan.Advance();
        if (_currentPlan.IsComplete)
        {
            InvalidatePlan();
            return;
        }

        GoapWorldState actualState = _worldStateBuilder.Build(context);
        if (!_currentPlan.IsRemainingPlanValid(actualState, context))
            InvalidatePlan();
    }

    private bool TryGetMovementInterrupt(
        GoapContext context,
        out TileNode target,
        out string purposeText)
    {
        target = null;
        purposeText = null;
        CharacterEntity owner = context.Owner;

        TileNode enemyTile = _taskController.FindNearestEnemyTile(
            owner.CurrentTileNode,
            context.ActiveNodes,
            _maxMoveTilesPerTurn * EnemySeekRangeTurns);

        if (enemyTile != null && enemyTile != owner.CurrentTileNode)
        {
            int seekChance = EnemySeekChanceDefault + owner.GetStatLevel(StatType.Combat);
            if (context.Selection.Policy == PolicyType.CombatFirst)
                seekChance = EnemySeekChanceCombatFirst;

            if (Random.Range(0, 100) < seekChance)
            {
                target = enemyTile;
                purposeText = "적과 전투하기 위해";
                return true;
            }
        }

        TileNode allyCombatTile = _taskController.FindNearestAllyCombatTile(
            owner.CurrentTileNode, context.ActiveNodes);

        if (allyCombatTile != null && allyCombatTile != owner.CurrentTileNode)
        {
            target = allyCombatTile;
            purposeText = "아군 전투를 지원하기 위해";
            return true;
        }

        return false;
    }

    private IEnumerator RunFallback(GoapContext context)
    {
        IGoapAction fallback = Random.Range(0, 2) == 0
            ? new IdleGoapAction()
            : new WanderGoapAction();

        if (!fallback.CanStart(context)) fallback = new IdleGoapAction();
        yield return fallback.ExecuteTurn(context);
    }

    private bool HasSelectionChanged(BigTurnSelectionData selection)
    {
        if (_currentPlan == null) return false;
        return _plannedPolicy != selection.Policy ||
               _plannedWeather != selection.Weather ||
               _plannedEvent != selection.EventType ||
               _plannedRaidTargetId != selection.RaidTargetGeneratorId;
    }

    private void StoreSelection(BigTurnSelectionData selection)
    {
        _plannedPolicy = selection.Policy;
        _plannedWeather = selection.Weather;
        _plannedEvent = selection.EventType;
        _plannedRaidTargetId = selection.RaidTargetGeneratorId;
    }

    private void InvalidatePlan()
    {
        _currentPlan = null;
    }
}
