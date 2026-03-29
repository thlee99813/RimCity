using System.Collections.Generic;

public static class CharacterTaskCommon
{
    public static List<TileNode> FindPath(TileNode start, TileNode goal, List<TileNode> activeNodes)
    {
        if (start == null || goal == null) return null;
        if (start == goal) return new List<TileNode>();

        HashSet<TileNode> activeSet = new HashSet<TileNode>(activeNodes);
        if (!activeSet.Contains(start) || !activeSet.Contains(goal)) return null;

        Queue<TileNode> queue = new Queue<TileNode>();
        Dictionary<TileNode, TileNode> cameFrom = new Dictionary<TileNode, TileNode>();
        HashSet<TileNode> visited = new HashSet<TileNode>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            TileNode current = queue.Dequeue();
            if (current == goal) break;

            for (int i = 0; i < current.Neighbors.Count; i++)
            {
                TileNode next = current.Neighbors[i];
                if (next == null) continue;
                if (!activeSet.Contains(next)) continue;
                if (visited.Contains(next)) continue;

                visited.Add(next);
                cameFrom[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!visited.Contains(goal)) return null;

        List<TileNode> path = new List<TileNode>();
        TileNode walk = goal;
        while (walk != start)
        {
            path.Add(walk);
            walk = cameFrom[walk];
        }

        path.Reverse();
        return path;
    }

    public static bool CanAfford(PlayerResourceInventory inv, ResourceCost[] costs)
    {
        for (int i = 0; i < costs.Length; i++)
            if (inv.GetAmount(costs[i].Type) < costs[i].Amount) return false;
        return true;
    }

    public static void ConsumeCosts(PlayerResourceInventory inv, ResourceCost[] costs)
    {
        for (int i = 0; i < costs.Length; i++)
            inv.Consume(costs[i].Type, costs[i].Amount);
    }
}
