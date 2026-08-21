using System.Collections.Generic;

public class GoapPlan
{
    private readonly List<IGoapAction> _actions;
    private int _currentIndex;

    public GoapGoal Goal { get; }
    public IGoapAction CurrentAction => IsComplete ? null : _actions[_currentIndex];
    public bool IsComplete => _currentIndex >= _actions.Count;
    public int ActionCount => _actions.Count;

    public GoapPlan(GoapGoal goal, List<IGoapAction> actions)
    {
        Goal = goal;
        _actions = actions;
    }

    public void Advance()
    {
        _currentIndex++;
        if (!IsComplete) CurrentAction.ResetRuntimeState();
    }

    public bool IsRemainingPlanValid(GoapWorldState state, GoapContext context)
    {
        for (int i = _currentIndex; i < _actions.Count; i++)
        {
            IGoapAction action = _actions[i];
            if (!action.ArePreconditionsMet(state, context)) return false;
            state = action.ApplyEffects(state, context);
        }

        return Goal.IsSatisfied(state);
    }
}
