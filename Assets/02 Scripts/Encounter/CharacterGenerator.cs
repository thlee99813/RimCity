using UnityEngine;
using System;
public class CharacterGenerator : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private CharacterEntity characterPrefab;
    [SerializeField] private Transform spawnPoint;
    
    
    public CharacterEntity SpawnCharacter(string characterName)
    {
        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        CharacterEntity entity = Instantiate(characterPrefab, pos, rot);
        CharacterData data = CreateRandomCharacter(characterName);
        entity.Initialize(data);
        return entity;
    }
    public CharacterData RerollCharacterData(CharacterEntity target, string characterName)
    {
        string fixedId = target.Data != null ? target.Data.Id : Guid.NewGuid().ToString("N");

        CharacterData data = CreateRandomCharacter(characterName);
        data.Id = fixedId;

        target.Initialize(data);
        return data;
    }
   private CharacterData CreateRandomCharacter(string name)
    {
        int age = UnityEngine.Random.Range(10, 51);

        CharacterData data = new CharacterData
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = string.IsNullOrWhiteSpace(name) ? "臾대챸" : name,
            Gender = UnityEngine.Random.value < 0.5f ? Gender.Male : Gender.Female,
            Age = age,
            MaxHealth = CharacterStatFormula.CalculateMaxHealthByAge(age),
            MaxMood = 100f
        };

        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
        data.Stats[type] = UnityEngine.Random.Range(0, 8);
        data.StatProgress[type] = 0;
        }


        

        Array allTraits = Enum.GetValues(typeof(TraitType));
        int traitCount = UnityEngine.Random.Range(0, 3);
        for (int i = 0; i < traitCount; i++)
        {
            TraitType t = (TraitType)allTraits.GetValue(UnityEngine.Random.Range(0, allTraits.Length));
            if (!data.Traits.Contains(t)) data.Traits.Add(t);
        }
        ApplyTraitStatModifiers(data);

        return data;
    }

    
    public CharacterData CreateEncounterCandidate()
    {
        return CreateRandomCharacter("조우자");
    }

    public CharacterEntity SpawnCharacterFromData(CharacterData sourceData, string enteredName)
    {
        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        CharacterEntity entity = Instantiate(characterPrefab, pos, rot);

        CharacterData data = CloneData(sourceData);
        data.Id = Guid.NewGuid().ToString("N");
        data.Name = string.IsNullOrWhiteSpace(enteredName) ? data.Name : enteredName.Trim();

        entity.Initialize(data);
        return entity;
    }
    private CharacterData CloneData(CharacterData source)
    {
        CharacterData c = new CharacterData
        {
            Id = source.Id,
            Name = source.Name,
            Gender = source.Gender,
            Age = source.Age,
            MaxHealth = source.MaxHealth,
            MaxMood = source.MaxMood,
            MaxHunger = source.MaxHunger,
            MaxSleep = source.MaxSleep,
            MaxFun = source.MaxFun
        };

        foreach (var kv in source.Stats) c.Stats[kv.Key] = kv.Value;
        foreach (var kv in source.StatProgress) c.StatProgress[kv.Key] = kv.Value;
        foreach (var kv in source.Needs) c.Needs[kv.Key] = kv.Value;
        for (int i = 0; i < source.Traits.Count; i++) c.Traits.Add(source.Traits[i]);

        return c;
    }
    private void ApplyTraitStatModifiers(CharacterData data)
{
    if (data == null || data.Traits == null) return;

    for (int i = 0; i < data.Traits.Count; i++)
    {
        ApplyOneTrait(data, data.Traits[i]);
    }

    ClampAllStats(data, 1, 11);
}

private void ApplyOneTrait(CharacterData data, TraitType trait)
{
    switch (trait)
    {
        case TraitType.Hikikomori:
            AddStat(data, StatType.Gather, +2);
            AddStat(data, StatType.Social, -2);
            break;

        case TraitType.Psychopath:
            AddStat(data, StatType.Combat, +2);
            AddStat(data, StatType.Social, -2);
            break;

        case TraitType.MisogynyMisandry:
            AddStat(data, StatType.Social, -2);
            break;

        case TraitType.Depression:
            AddStat(data, StatType.Social, -1);
            break;

        case TraitType.Kind:
            AddStat(data, StatType.Social, +2);
            AddStat(data, StatType.Combat, -1);
            break;

        case TraitType.Tough:
            AddStat(data, StatType.Combat, +1);
            AddStat(data, StatType.Build, +1);
            break;

        case TraitType.Diligent:
            AddStat(data, StatType.Craft, +1);
            AddStat(data, StatType.Build, +1);
            AddStat(data, StatType.Gather, +1);
            break;

        case TraitType.Optimistic:
            AddStat(data, StatType.Social, +1);
            AddStat(data, StatType.Gather, +1);
            break;

        case TraitType.AllRounder:
            AddAllStats(data, +1);
            break;

        case TraitType.Inept:
            AddAllStats(data, -1);
            break;

        case TraitType.Artisan:
            AddStat(data, StatType.Craft, +2);
            AddStat(data, StatType.Build, +1);
            AddStat(data, StatType.Combat, -1);
            break;

        case TraitType.Hunter:
            AddStat(data, StatType.Gather, +2);
            AddStat(data, StatType.Combat, +1);
            AddStat(data, StatType.Social, -1);
            break;

        case TraitType.Chatterbox:
            AddStat(data, StatType.Social, +2);
            AddStat(data, StatType.Craft, -1);
            break;

        case TraitType.Coward:
            AddStat(data, StatType.Combat, -3);
            AddStat(data, StatType.Social, +1);
            AddStat(data, StatType.Build, +1);
            AddStat(data, StatType.Gather, +1);
            AddStat(data, StatType.Craft, +1);

            break;
    }
}

    private void AddAllStats(CharacterData data, int delta)
    {
        AddStat(data, StatType.Combat, delta);
        AddStat(data, StatType.Craft, delta);
        AddStat(data, StatType.Build, delta);
        AddStat(data, StatType.Gather, delta);
        AddStat(data, StatType.Social, delta);
    }

    private void AddStat(CharacterData data, StatType type, int delta)
    {
        if (!data.Stats.ContainsKey(type))
            data.Stats[type] = 1;

        data.Stats[type] += delta;
    }

    private void ClampAllStats(CharacterData data, int min, int max)
    {
        data.Stats[StatType.Combat] = Mathf.Clamp(data.Stats[StatType.Combat], min, max);
        data.Stats[StatType.Craft] = Mathf.Clamp(data.Stats[StatType.Craft], min, max);
        data.Stats[StatType.Build] = Mathf.Clamp(data.Stats[StatType.Build], min, max);
        data.Stats[StatType.Gather] = Mathf.Clamp(data.Stats[StatType.Gather], min, max);
        data.Stats[StatType.Social] = Mathf.Clamp(data.Stats[StatType.Social], min, max);
    }


}
