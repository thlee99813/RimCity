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
        if (costs == null || costs.Length == 0) return true;

        for (int i = 0; i < costs.Length; i++)
            if (inv.GetAmount(costs[i].Type) < costs[i].Amount) return false;
        return true;
    }

    public static void ConsumeCosts(PlayerResourceInventory inv, ResourceCost[] costs)
    {
        if (costs == null || costs.Length == 0) return;

        for (int i = 0; i < costs.Length; i++)
        inv.Consume(costs[i].Type, costs[i].Amount);
    }

    public static ResourceType GetFirstMissingResource(PlayerResourceInventory inv, ResourceCost[] costs)
    {
        if (costs == null) return ResourceType.None;

        for (int i = 0; i < costs.Length; i++)
        {
            if (inv.GetAmount(costs[i].Type) < costs[i].Amount)
                return costs[i].Type;
        }

        return ResourceType.None;
    }

    public static TileNode FindNearestReachableResourceTile(
    TileNode start,
    List<TileNode> activeNodes,
    ResourceType targetType)
    {
        if (start == null || activeNodes == null) return null;

        TileNode best = null;
        int bestSteps = int.MaxValue;

        for (int i = 0; i < activeNodes.Count; i++)
        {
            TileNode tile = activeNodes[i];
            if (tile == null) continue;
            if (tile.ResourceTypeOnTile != targetType) continue;

            ResourceNode node = tile.ResourceNodeOnTile;
            if (node == null || node.Amount <= 0 || !node.gameObject.activeInHierarchy) continue;

            List<TileNode> path = FindPath(start, tile, activeNodes);
            if (path == null) continue;

            int steps = path.Count;
            if (steps < bestSteps)
            {
                bestSteps = steps;
                best = tile;
            }
        }

        return best;
    }

    public static bool TryGetPlacedStructureType(TileNode tile, out StructureType type)
    {
        type = StructureType.None;
        if (tile == null || tile.PlacedStructure == null) return false;

        StructureEffectMarker marker = tile.PlacedStructure.GetComponentInChildren<StructureEffectMarker>(true);
        if (marker == null) return false;

        type = marker.Type;
        return true;
    }

    public static TileNode FindNearestReachableStructureTile(
        TileNode start,
        List<TileNode> activeNodes,
        int maxSteps,
        StructureType primary,
        StructureType secondary = StructureType.None)
    {
        if (start == null || activeNodes == null) return null;

        TileNode best = null;
        int bestSteps = int.MaxValue;

        for (int i = 0; i < activeNodes.Count; i++)
        {
            TileNode tile = activeNodes[i];
            if (tile == null) continue;

            if (!TryGetPlacedStructureType(tile, out StructureType foundType)) continue;
            if (foundType != primary && foundType != secondary) continue;

            List<TileNode> path = FindPath(start, tile, activeNodes);
            if (path == null) continue;

            int steps = path.Count;
            if (steps > maxSteps) continue;

            if (steps < bestSteps)
            {
                bestSteps = steps;
                best = tile;
            }
        }

        return best;
    }

}
