using System;

public struct GoapWorldState : IEquatable<GoapWorldState>
{
    public int Health;
    public int Hunger;
    public int Sleep;
    public int Mood;

    public int Berry;
    public int Tree;
    public int Rock;
    public int Grass;

    public int Cloth;
    public int WoodenSpear;
    public int StoneSpear;
    public int Fan;
    public int Bandage;
    public int Medkit;

    public int GatherableBerry;
    public int GatherableTree;
    public int GatherableRock;
    public int GatherableGrass;
    public int BuildableTileCount;

    public int TorchCount;
    public int CampfireCount;
    public int BedCount;
    public int SweatingStoneCount;

    public ArmorType Armor;
    public WeaponType Weapon;
    public UtilityType Utility;

    public bool AtBed;
    public bool ColdProtected;
    public bool HeatProtected;
    public bool ReachableBed;
    public bool ReachableTorch;
    public bool ReachableCampfire;
    public bool ReachableSweatingStone;

    public bool Socialized;
    public bool Explored;
    public bool Idled;

    public int GetResourceAmount(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Berry: return Berry;
            case ResourceType.Tree: return Tree;
            case ResourceType.Rock: return Rock;
            case ResourceType.Grass: return Grass;
            default: return 0;
        }
    }

    public void AddResource(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Berry: Berry += amount; break;
            case ResourceType.Tree: Tree += amount; break;
            case ResourceType.Rock: Rock += amount; break;
            case ResourceType.Grass: Grass += amount; break;
        }
    }

    public int GetGatherableAmount(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Berry: return GatherableBerry;
            case ResourceType.Tree: return GatherableTree;
            case ResourceType.Rock: return GatherableRock;
            case ResourceType.Grass: return GatherableGrass;
            default: return 0;
        }
    }

    public void ConsumeGatherable(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Berry: GatherableBerry--; break;
            case ResourceType.Tree: GatherableTree--; break;
            case ResourceType.Rock: GatherableRock--; break;
            case ResourceType.Grass: GatherableGrass--; break;
        }
    }

    public bool Equals(GoapWorldState other)
    {
        return Health == other.Health && Hunger == other.Hunger && Sleep == other.Sleep && Mood == other.Mood &&
               Berry == other.Berry && Tree == other.Tree && Rock == other.Rock && Grass == other.Grass &&
               Cloth == other.Cloth && WoodenSpear == other.WoodenSpear && StoneSpear == other.StoneSpear &&
               Fan == other.Fan && Bandage == other.Bandage && Medkit == other.Medkit &&
               GatherableBerry == other.GatherableBerry && GatherableTree == other.GatherableTree &&
               GatherableRock == other.GatherableRock && GatherableGrass == other.GatherableGrass &&
               BuildableTileCount == other.BuildableTileCount && TorchCount == other.TorchCount &&
               CampfireCount == other.CampfireCount && BedCount == other.BedCount &&
               SweatingStoneCount == other.SweatingStoneCount && Armor == other.Armor &&
               Weapon == other.Weapon && Utility == other.Utility && AtBed == other.AtBed &&
               ColdProtected == other.ColdProtected && HeatProtected == other.HeatProtected &&
               ReachableBed == other.ReachableBed && ReachableTorch == other.ReachableTorch &&
               ReachableCampfire == other.ReachableCampfire &&
               ReachableSweatingStone == other.ReachableSweatingStone && Socialized == other.Socialized &&
               Explored == other.Explored && Idled == other.Idled;
    }

    public override bool Equals(object obj)
    {
        return obj is GoapWorldState other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Health;
            hash = hash * 31 + Hunger;
            hash = hash * 31 + Sleep;
            hash = hash * 31 + Mood;
            hash = hash * 31 + Berry;
            hash = hash * 31 + Tree;
            hash = hash * 31 + Rock;
            hash = hash * 31 + Grass;
            hash = hash * 31 + Cloth;
            hash = hash * 31 + WoodenSpear;
            hash = hash * 31 + StoneSpear;
            hash = hash * 31 + Fan;
            hash = hash * 31 + Bandage;
            hash = hash * 31 + Medkit;
            hash = hash * 31 + GatherableBerry;
            hash = hash * 31 + GatherableTree;
            hash = hash * 31 + GatherableRock;
            hash = hash * 31 + GatherableGrass;
            hash = hash * 31 + BuildableTileCount;
            hash = hash * 31 + TorchCount;
            hash = hash * 31 + CampfireCount;
            hash = hash * 31 + BedCount;
            hash = hash * 31 + SweatingStoneCount;
            hash = hash * 31 + (int)Armor;
            hash = hash * 31 + (int)Weapon;
            hash = hash * 31 + (int)Utility;
            hash = hash * 31 + (AtBed ? 1 : 0);
            hash = hash * 31 + (ColdProtected ? 1 : 0);
            hash = hash * 31 + (HeatProtected ? 1 : 0);
            hash = hash * 31 + (ReachableBed ? 1 : 0);
            hash = hash * 31 + (ReachableTorch ? 1 : 0);
            hash = hash * 31 + (ReachableCampfire ? 1 : 0);
            hash = hash * 31 + (ReachableSweatingStone ? 1 : 0);
            hash = hash * 31 + (Socialized ? 1 : 0);
            hash = hash * 31 + (Explored ? 1 : 0);
            hash = hash * 31 + (Idled ? 1 : 0);
            return hash;
        }
    }
}
