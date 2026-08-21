public enum CharacterGoalType
{
    RecoverSleep,
    SatisfyHunger,
    RecoverHealth,
    ProtectFromHeat,
    ProtectFromCold,
    EquipItem,
    GatherResource,
    CraftItem,
    BuildStructure,
    Socialize,
    Explore,
    Idle
}

public class GoapGoal
{
    public CharacterGoalType Type { get; }
    public float Utility { get; set; }
    public int DesiredValue { get; }
    public string TargetId { get; }
    public ResourceType TargetResource { get; }

    public GoapGoal(
        CharacterGoalType type,
        float utility,
        int desiredValue = 0,
        string targetId = null,
        ResourceType targetResource = ResourceType.None)
    {
        Type = type;
        Utility = utility;
        DesiredValue = desiredValue;
        TargetId = targetId;
        TargetResource = targetResource;
    }

    public bool IsSatisfied(GoapWorldState state)
    {
        switch (Type)
        {
            case CharacterGoalType.RecoverSleep:
                return state.Sleep >= DesiredValue;
            case CharacterGoalType.SatisfyHunger:
                return state.Hunger >= DesiredValue;
            case CharacterGoalType.RecoverHealth:
                return state.Health >= DesiredValue;
            case CharacterGoalType.ProtectFromHeat:
                return state.HeatProtected;
            case CharacterGoalType.ProtectFromCold:
                return state.ColdProtected;
            case CharacterGoalType.EquipItem:
                return IsItemEquipped(state, TargetId);
            case CharacterGoalType.GatherResource:
                return state.GetResourceAmount(TargetResource) >= DesiredValue;
            case CharacterGoalType.CraftItem:
                return GetItemAmount(state, TargetId) >= DesiredValue || IsItemEquipped(state, TargetId);
            case CharacterGoalType.BuildStructure:
                return GetStructureCount(state, TargetId) >= DesiredValue;
            case CharacterGoalType.Socialize:
                return state.Socialized;
            case CharacterGoalType.Explore:
                return state.Explored;
            case CharacterGoalType.Idle:
                return state.Idled;
            default:
                return false;
        }
    }

    private bool IsItemEquipped(GoapWorldState state, string itemId)
    {
        if (itemId == ItemIds.Cloth) return state.Armor == ArmorType.Cloth;
        if (itemId == ItemIds.WoodenSpear) return state.Weapon == WeaponType.WoodenSpear;
        if (itemId == ItemIds.StoneSpear) return state.Weapon == WeaponType.StoneSpear;
        if (itemId == ItemIds.Fan) return state.Utility == UtilityType.Fan;
        return false;
    }

    private int GetItemAmount(GoapWorldState state, string itemId)
    {
        if (itemId == ItemIds.Cloth) return state.Cloth;
        if (itemId == ItemIds.WoodenSpear) return state.WoodenSpear;
        if (itemId == ItemIds.StoneSpear) return state.StoneSpear;
        if (itemId == ItemIds.Fan) return state.Fan;
        if (itemId == ItemIds.Bandage) return state.Bandage;
        if (itemId == ItemIds.Medkit) return state.Medkit;
        return 0;
    }

    private int GetStructureCount(GoapWorldState state, string recipeId)
    {
        if (recipeId == "torch") return state.TorchCount;
        if (recipeId == "campfire") return state.CampfireCount;
        if (recipeId == "bed") return state.BedCount;
        if (recipeId == "coldStone") return state.SweatingStoneCount;
        return 0;
    }
}
