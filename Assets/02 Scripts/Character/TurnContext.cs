using System.Collections.Generic;
using UnityEngine;

public enum PolicyType { CombatFirst, CraftFirst, BuildFirst, GatherFirst, SocialFirst }
public enum WeatherType { Mild, Hot, Cold, Heatwave, Drought, Flood, Snowstorm, ExtremeCold }
public enum WorldEventType { None, Visitor, Raid }

public class TurnContext
{
    // 턴 정보
    public int BigTurnIndex;      
    public int SmallTurnIndex;   

    // 플레이어 선택
    public PolicyType Policy;
    public WeatherType Weather;
    public WorldEventType EventType;

    // 전역 보정
    public float WeatherHealthDeltaPerTurn; 
    public float WeatherMoodDeltaPerTurn;   



    // 행동 가중치 보정치
    public Dictionary<StatType, float> StatPriorityBonus = new();
}
