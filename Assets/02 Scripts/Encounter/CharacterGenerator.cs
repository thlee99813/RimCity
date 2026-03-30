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


}
