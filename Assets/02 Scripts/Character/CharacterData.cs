using System;
using System.Collections.Generic;
using UnityEngine;

public enum Gender { Male, Female }
public enum StatType { Combat, Craft, Build, Gather, Social }
public enum NeedType { Hunger, Sleep, Mood }
public enum RelationType { Stranger, Friend, Lover, Spouse, Family }
public enum TraitType
{
    Hikikomori, Psychopath, MisogynyMisandry, Depression, Kind, Tough, Diligent, Optimistic,
    AllRounder,   // 전 능력 +1
    Inept,        // 전 능력 -1
    Artisan,      // 제작+2, 건축+1, 전투-1
    Hunter,       // 수집+2, 전투+1, 매력-1
    Chatterbox,   // 매력+2, 제작-1
    Coward        // 전투-2, 매력+1
}
[Serializable]
public class CharacterData
{
    public string Id;
    public string Name;
    public Gender Gender;
    public int Age;

    public float MaxHealth = 100f;
    public float MaxMood = 100f;
    public float MaxHunger = 100f;
    public float MaxSleep = 100f;
    public float MaxFun = 100f;    

    public Dictionary<StatType, int> Stats = new();
    
    public Dictionary<StatType, int> StatProgress = new();

    public Dictionary<NeedType, float> Needs = new();

    public List<TraitType> Traits = new();
}
