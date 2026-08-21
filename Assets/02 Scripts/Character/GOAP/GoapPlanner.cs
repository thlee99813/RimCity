using System.Collections.Generic;

public class GoapPlanner
{
    private const int MaxExpandedNodes = 512;
    private const int MaxPlanDepth = 16;

    private class SearchNode
    {
        public GoapWorldState State;
        public int Cost;
        public List<IGoapAction> Actions;
    }

    public GoapPlan CreatePlan(
        GoapWorldState initialState,
        GoapGoal goal,
        List<IGoapAction> availableActions,
        GoapContext context)
    {
        if (goal.IsSatisfied(initialState)) return new GoapPlan(goal, new List<IGoapAction>());

        List<SearchNode> open = new List<SearchNode>
        {
            new SearchNode
            {
                State = initialState,
                Cost = 0,
                Actions = new List<IGoapAction>()
            }
        };

        Dictionary<GoapWorldState, int> bestCosts = new Dictionary<GoapWorldState, int>
        {
            { initialState, 0 }
        };

        int expanded = 0;

        while (open.Count > 0 && expanded < MaxExpandedNodes)
        {
            int bestIndex = FindLowestCostIndex(open);
            SearchNode current = open[bestIndex];
            open.RemoveAt(bestIndex);
            expanded++;

            if (goal.IsSatisfied(current.State))
                return new GoapPlan(goal, current.Actions);

            if (current.Actions.Count >= MaxPlanDepth) continue;

            for (int i = 0; i < availableActions.Count; i++)
            {
                IGoapAction action = availableActions[i];
                if (!action.ArePreconditionsMet(current.State, context)) continue;

                GoapWorldState nextState = action.ApplyEffects(current.State, context);
                if (nextState.Equals(current.State)) continue;

                int nextCost = current.Cost + action.GetCost(current.State, context);
                if (bestCosts.TryGetValue(nextState, out int knownCost) && knownCost <= nextCost) continue;

                bestCosts[nextState] = nextCost;
                List<IGoapAction> nextActions = new List<IGoapAction>(current.Actions) { action };
                open.Add(new SearchNode
                {
                    State = nextState,
                    Cost = nextCost,
                    Actions = nextActions
                });
            }
        }

        return null;
    }

    private int FindLowestCostIndex(List<SearchNode> nodes)
    {
        int bestIndex = 0;

        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].Cost < nodes[bestIndex].Cost)
                bestIndex = i;
        }

        return bestIndex;
    }
}
