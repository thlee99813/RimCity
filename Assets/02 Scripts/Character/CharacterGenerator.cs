using UnityEngine;
using System;
public class CharacterGenerator : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private CharacterEntity characterPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private string fixedName = "이태환";
    

    private void Start()
    {
        SpawnCharacter(fixedName);
    
      
    }
    public CharacterEntity SpawnCharacter(string characterName)
        {
            
            Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            CharacterEntity entity = Instantiate(characterPrefab, pos, rot);
            CharacterData data = CreateRandomCharacter(characterName);
            entity.Initialize(data);
            return entity;
        }
    private CharacterData CreateRandomCharacter(string name)
    {
        CharacterData data = new CharacterData
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = string.IsNullOrWhiteSpace(name) ? "무명" : name,
            Gender = UnityEngine.Random.value < 0.5f ? Gender.Male : Gender.Female,
            Age = UnityEngine.Random.Range(18, 51),
            Health = UnityEngine.Random.Range(70f, 101f),
            Mood = UnityEngine.Random.Range(40f, 101f)
        };

        data.Stats[StatType.Combat] = UnityEngine.Random.Range(0, 6);
        data.Stats[StatType.Craft]  = UnityEngine.Random.Range(0, 6);
        data.Stats[StatType.Build]  = UnityEngine.Random.Range(0, 6);
        data.Stats[StatType.Gather] = UnityEngine.Random.Range(0, 6);
        data.Stats[StatType.Social] = UnityEngine.Random.Range(0, 6);
        data.Needs[NeedType.Hunger] = UnityEngine.Random.Range(40f, 101f);
        data.Needs[NeedType.Sleep]  = UnityEngine.Random.Range(40f, 101f);
        data.Needs[NeedType.Fun]    = UnityEngine.Random.Range(40f, 101f);

        Array allTraits = Enum.GetValues(typeof(TraitType));
        int traitCount = UnityEngine.Random.Range(0, 3);
        for (int i = 0; i < traitCount; i++)
        {
            TraitType t = (TraitType)allTraits.GetValue(UnityEngine.Random.Range(0, allTraits.Length));
            if (!data.Traits.Contains(t))
                data.Traits.Add(t);
        }

        return data;
    }
}
