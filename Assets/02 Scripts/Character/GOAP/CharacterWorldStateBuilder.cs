using System.Collections.Generic;
using UnityEngine;

public class CharacterWorldStateBuilder
{
    private const int BedReachableTurns = 2;

    public GoapWorldState Build(GoapContext context)
    {
        CharacterEntity owner = context.Owner;
        PlayerResourceInventory inventory = context.Inventory;

        GoapWorldState state = new GoapWorldState
        {
            Health = Mathf.RoundToInt(owner.Status.Health),
            Hunger = Mathf.RoundToInt(owner.Status.Hunger),
            Sleep = Mathf.RoundToInt(owner.Status.Sleep),
            Mood = Mathf.RoundToInt(owner.Status.Mood),
            Berry = inventory.Berry,
            Tree = inventory.Tree,
            Rock = inventory.Rock,
            Grass = inventory.Fiber,
            Cloth = inventory.Cloth,
            WoodenSpear = inventory.WoodenSpear,
            StoneSpear = inventory.StoneSpear,
            Fan = inventory.Fan,
            Bandage = inventory.Bandage,
            Medkit = inventory.Medkit,
            Armor = owner.Equipment.Armor,
            Weapon = owner.Equipment.Weapon,
            Utility = owner.Equipment.Utility
        };

        CollectWorldFacts(context, ref state);
        return state;
    }

    private void CollectWorldFacts(GoapContext context, ref GoapWorldState state)
    {
        CharacterEntity owner = context.Owner;
        TileNode current = owner.CurrentTileNode;
        List<TileNode> activeNodes = context.ActiveNodes;
        int gatherLevel = owner.GetStatLevel(StatType.Gather);

        for (int i = 0; i < activeNodes.Count; i++)
        {
            TileNode tile = activeNodes[i];
            if (tile == null) continue;

            if (!tile.IsOccupied && !tile.HasResource)
                state.BuildableTileCount++;

            CollectReachableResource(current, tile, activeNodes, gatherLevel, ref state);
            CollectStructure(current, tile, activeNodes, context.MaxMoveTilesPerTurn, ref state);
        }

        if (CharacterTaskCommon.TryGetPlacedStructureType(current, out StructureType currentType))
            state.AtBed = currentType == StructureType.Bed;

        bool hasNearbyWarmStructure = HasNearbyStructure(current, activeNodes, StructureType.Torch, StructureType.Campfire);
        bool hasNearbyCoolStructure = HasNearbyStructure(current, activeNodes, StructureType.SweatingStone);

        state.ColdProtected = owner.Equipment.Armor == ArmorType.Cloth || hasNearbyWarmStructure;
        state.HeatProtected = owner.Equipment.Utility == UtilityType.Fan || hasNearbyCoolStructure;
    }

    private void CollectReachableResource(
        TileNode current,
        TileNode tile,
        List<TileNode> activeNodes,
        int gatherLevel,
        ref GoapWorldState state)
    {
        ResourceNode resource = tile.ResourceNodeOnTile;
        if (resource == null || resource.Amount <= 0 || !resource.gameObject.activeInHierarchy) return;
        if (!IsGatherTypeAllowed(tile.ResourceTypeOnTile, gatherLevel)) return;
        if (CharacterTaskCommon.FindPath(current, tile, activeNodes) == null) return;

        switch (tile.ResourceTypeOnTile)
        {
            case ResourceType.Berry: state.GatherableBerry += resource.Amount; break;
            case ResourceType.Tree: state.GatherableTree += resource.Amount; break;
            case ResourceType.Rock: state.GatherableRock += resource.Amount; break;
            case ResourceType.Grass: state.GatherableGrass += resource.Amount; break;
        }
    }

    private void CollectStructure(
        TileNode current,
        TileNode tile,
        List<TileNode> activeNodes,
        int maxMoveTilesPerTurn,
        ref GoapWorldState state)
    {
        if (!CharacterTaskCommon.TryGetPlacedStructureType(tile, out StructureType type)) return;

        switch (type)
        {
            case StructureType.Torch: state.TorchCount++; break;
            case StructureType.Campfire: state.CampfireCount++; break;
            case StructureType.Bed: state.BedCount++; break;
            case StructureType.SweatingStone: state.SweatingStoneCount++; break;
        }

        List<TileNode> path = CharacterTaskCommon.FindPath(current, tile, activeNodes);
        if (path == null) return;

        int maxSteps = type == StructureType.Bed
            ? maxMoveTilesPerTurn * BedReachableTurns
            : maxMoveTilesPerTurn;

        if (path.Count > maxSteps) return;

        switch (type)
        {
            case StructureType.Torch: state.ReachableTorch = true; break;
            case StructureType.Campfire: state.ReachableCampfire = true; break;
            case StructureType.Bed: state.ReachableBed = true; break;
            case StructureType.SweatingStone: state.ReachableSweatingStone = true; break;
        }
    }

    private bool HasNearbyStructure(
        TileNode current,
        List<TileNode> activeNodes,
        StructureType primary,
        StructureType secondary = StructureType.None)
    {
        if (current == null) return false;

        for (int i = 0; i < activeNodes.Count; i++)
        {
            TileNode tile = activeNodes[i];
            if (tile == null) continue;

            int dx = Mathf.Abs(tile.GridX - current.GridX);
            int dz = Mathf.Abs(tile.GridZ - current.GridZ);
            if (dx > 2 || dz > 2) continue;
            if (!CharacterTaskCommon.TryGetPlacedStructureType(tile, out StructureType type)) continue;
            if (type == primary || type == secondary) return true;
        }

        return false;
    }

    private bool IsGatherTypeAllowed(ResourceType type, int gatherLevel)
    {
        if (type == ResourceType.Berry || type == ResourceType.Tree) return true;
        if (type == ResourceType.Grass) return gatherLevel >= 4;
        if (type == ResourceType.Rock) return gatherLevel >= 7;
        return false;
    }
}
