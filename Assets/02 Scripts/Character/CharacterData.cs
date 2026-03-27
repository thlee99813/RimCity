using System;
using System.Collections.Generic;
using UnityEngine;

public enum Gender { Male, Female }
public enum StatType { Combat, Craft, Build, Gather, Social }
public enum NeedType { Hunger, Sleep, Fun }
public enum RelationType { Stranger, Friend, Lover, Spouse, Family }
public enum TraitType { Hikikomori, Psychopath, MisogynyMisandry, Depression, Kind, Tough, Diligent, Optimistic }

[Serializable]
public class CharacterData
{
    public string Id;
    public string Name;
    public Gender Gender;
    public int Age;

    public float Health = 100f;    
    public float Mood = 100f;     

    public Dictionary<StatType, int> Stats = new();

    public Dictionary<NeedType, float> Needs = new();

    public List<TraitType> Traits = new();
}
