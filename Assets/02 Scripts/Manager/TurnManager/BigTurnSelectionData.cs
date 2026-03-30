// BigTurnSelectionData.cs
[System.Serializable]
public class BigTurnSelectionData
{
    public PolicyType Policy;
    public WeatherType Weather;
    public WorldEventType EventType;
    public bool ExpandTerritory;
    public string RaidTargetGeneratorId;


    public static BigTurnSelectionData Default()
    {
        return new BigTurnSelectionData
        {
            Policy = PolicyType.GatherFirst,
            Weather = WeatherType.Mild,
            EventType = WorldEventType.None,
            ExpandTerritory = false,
            RaidTargetGeneratorId = null

        };
    }
}
