using System.Collections.Generic;

public struct ActionResult
{
    public string LogText;
    public float HealthDelta;
    public float MoodDelta;
    public Dictionary<NeedType, float> NeedDelta;
    public Dictionary<string, int> ItemDelta;
}