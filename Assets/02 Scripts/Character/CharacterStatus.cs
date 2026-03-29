using UnityEngine;

[System.Serializable]
public class CharacterStatus
{
    public float Health;
    public float Mood;
    public float Hunger;
    public float Sleep;

    public void InitializeFromData(CharacterData data)
    {
        Health = data.MaxHealth;
        Mood = data.MaxMood;
        Hunger = data.MaxHunger;
        Sleep = data.MaxSleep;
    }

    public void TickNeeds(float hungerDelta, float sleepDelta, float funDelta, CharacterData data)
    {
        Hunger = Mathf.Clamp(Hunger + hungerDelta, 0f, data.MaxHunger);
        Sleep = Mathf.Clamp(Sleep + sleepDelta, 0f, data.MaxSleep);
        Mood = Mathf.Clamp(Mood + funDelta, 0f, data.MaxMood);
    }

    public void AddHealth(float value, CharacterData data)
    {
        Health = Mathf.Clamp(Health + value, 0f, data.MaxHealth);
    }

    public void AddMood(float value, CharacterData data)
    {
        Mood = Mathf.Clamp(Mood + value, 0f, data.MaxMood);
    }
    public void AddHunger(float value, CharacterData data)
    {
        Hunger = Mathf.Clamp(Hunger + value, 0f, data.MaxHunger);
    }

    
}
