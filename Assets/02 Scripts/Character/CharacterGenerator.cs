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
        CharacterData data = CreateRandomCharacter(characterName);
        target.Initialize(data);
        return data;
    }
   private CharacterData CreateRandomCharacter(string name)
    {
        int age = UnityEngine.Random.Range(10, 51);

        CharacterData data = new CharacterData
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = string.IsNullOrWhiteSpace(name) ? "무명" : name,
            Gender = UnityEngine.Random.value < 0.5f ? Gender.Male : Gender.Female,
            Age = age,
            MaxHealth = CalculateMaxHealthByAge(age),
            MaxMood = 100f
        };

        data.Stats[StatType.Combat] = UnityEngine.Random.Range(0, 6);
        data.Stats[StatType.Craft] = UnityEngine.Random.Range(0, 6);
        data.Stats[StatType.Build] = UnityEngine.Random.Range(0, 6);
        data.Stats[StatType.Gather] = UnityEngine.Random.Range(0, 6);
        data.Stats[StatType.Social] = UnityEngine.Random.Range(0, 6);

        

        Array allTraits = Enum.GetValues(typeof(TraitType));
        int traitCount = UnityEngine.Random.Range(0, 3);
        for (int i = 0; i < traitCount; i++)
        {
            TraitType t = (TraitType)allTraits.GetValue(UnityEngine.Random.Range(0, allTraits.Length));
            if (!data.Traits.Contains(t)) data.Traits.Add(t);
        }

        return data;
    }

    private float CalculateMaxHealthByAge(int age)
    {
        age = Mathf.Clamp(age, 0, 100);

        // 0살 50 -> 35살 : 100
        if (age <= 35)
        {
            float t = age / 35f;
            return Mathf.Lerp(50f, 100f, t);
        }

        // 35살 : 100 -> 100살 : 10
        float t2 = (age - 35f) / 65f;
        return Mathf.Lerp(100f, 10f, t2);
    }
}
