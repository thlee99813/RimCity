using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : Singleton<CharacterManager>
{
    public readonly List<CharacterEntity> ActiveCharacters = new();

    protected override void Init()
    {
    }

    public void Register(CharacterEntity character)
    {
        if (!ActiveCharacters.Contains(character))
            ActiveCharacters.Add(character);
    }

    public void Unregister(CharacterEntity character)
    {
        ActiveCharacters.Remove(character);
    }
    public int GetAliveCount()
    {
        int alive = 0;
        for (int i = 0; i < ActiveCharacters.Count; i++)
        {
            CharacterEntity ch = ActiveCharacters[i];
            if (ch == null || ch.IsDead) continue;
            alive++;
        }
        return alive;
    }

}
