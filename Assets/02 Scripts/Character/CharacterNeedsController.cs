using System.Collections.Generic;
using UnityEngine;

public class CharacterNeedsController
{
    private readonly float _hungerDeltaPerTurn;
    private readonly float _sleepDeltaPerTurn;
    private readonly float _funDeltaPerTurn;
    private readonly float _healthDeltaWhenStarving;
    public struct StructureEffectReport
    {
        public bool HasTorch;
        public bool HasCampfire;
        public bool HasSweatingStone;
        public float MoodBonus;
        public float FinalWeatherHealthDelta;

        public bool HasAny => HasTorch || HasCampfire || HasSweatingStone;
    }

    public CharacterNeedsController(float hungerDeltaPerTurn, float sleepDeltaPerTurn, float funDeltaPerTurn, float healthDeltaWhenStarving)
    {
        _hungerDeltaPerTurn = hungerDeltaPerTurn;
        _sleepDeltaPerTurn = sleepDeltaPerTurn;
        _funDeltaPerTurn = funDeltaPerTurn;
        _healthDeltaWhenStarving = healthDeltaWhenStarving;
    }  

    public StructureEffectReport Tick (CharacterStatus status, CharacterData data, CharacterEquipment equipment, WeatherType weather, TileNode currentTile, List<TileNode> activeNodes)    
    {
        status.TickNeeds(_hungerDeltaPerTurn, _sleepDeltaPerTurn, _funDeltaPerTurn, data);

        float weatherHealthDelta = 0f;

        switch (weather)
        {
            case WeatherType.Mild:
                weatherHealthDelta = 0f;
                break;
            case WeatherType.Hot:
                weatherHealthDelta = -2f;
                break;
            case WeatherType.Cold:
                weatherHealthDelta = -2f;
                break;
            case WeatherType.Heatwave:
                weatherHealthDelta = -4f;
                break;
            case WeatherType.Drought:
                weatherHealthDelta = -2f;
                status.AddHunger(-2f, data);
                break;
            case WeatherType.ExtremeCold:
                weatherHealthDelta = -4f;
                break;
        }

        float envColdMul = 1f;
        float envHeatMul = 1f;
        float moodBonus = 0f;
        bool hasTorch = false;
        bool hasCampfire = false;
        bool hasSweatingStone = false;

        ApplyNearbyStructureEffectsByGrid(currentTile, activeNodes, ref envColdMul, ref envHeatMul, ref moodBonus, ref hasTorch, ref hasCampfire, ref hasSweatingStone);

        if (moodBonus != 0f)
            status.AddMood(moodBonus, data);

        if (weatherHealthDelta < 0f)
        {
            if (weather == WeatherType.Hot || weather == WeatherType.Heatwave || weather == WeatherType.Drought)
                weatherHealthDelta *= equipment.GetHeatDamageMultiplier() * envHeatMul;

            if (weather == WeatherType.Cold || weather == WeatherType.ExtremeCold)
                weatherHealthDelta *= equipment.GetColdDamageMultiplier() * envColdMul;
        }

        status.AddHealth(weatherHealthDelta, data);

        if (status.Hunger <= 0f)
            status.AddHealth(_healthDeltaWhenStarving, data);

        return new StructureEffectReport
        {
            HasTorch = hasTorch,
            HasCampfire = hasCampfire,
            HasSweatingStone = hasSweatingStone,
            MoodBonus = moodBonus,
            FinalWeatherHealthDelta = weatherHealthDelta
        };
    }
    private void ApplyNearbyStructureEffectsByGrid(TileNode currentTile, List<TileNode> activeNodes, ref float coldMul, ref float heatMul, ref float moodBonus,
        ref bool hasTorch, ref bool hasCampfire, ref bool hasSweatingStone)
    {
        if (currentTile == null || activeNodes == null || activeNodes.Count == 0)
            return;

     

        int range = 2; 

        for (int i = 0; i < activeNodes.Count; i++)
        {
            TileNode tile = activeNodes[i];
            if (tile == null) continue;

            int dx = Mathf.Abs(tile.GridX - currentTile.GridX);
            int dz = Mathf.Abs(tile.GridZ - currentTile.GridZ);
            if (dx > range || dz > range) continue;

            CheckTileStructure(tile, ref hasTorch, ref hasCampfire, ref hasSweatingStone);
        }

        if (hasTorch)
        {
            moodBonus += 4f;           // 기분 
            coldMul = Mathf.Min(coldMul, 0.5f); // 추위 피해 50% 감소
        }

        if (hasCampfire)
        {
            moodBonus += 8f;           // 기분
            coldMul = Mathf.Min(coldMul, 0.2f); // 추위 피해 80% 감소
        }

        if (hasSweatingStone)
        {
            moodBonus += 8f;           // 기분
            heatMul = Mathf.Min(heatMul, 0.2f); // 더위 피해 80% 감소
        }
    }

    private void CheckTileStructure(
        TileNode tile,
        ref bool hasTorch,
        ref bool hasCampfire,
        ref bool hasSweatingStone)
    {
        if (tile.PlacedStructure == null)
        {
            return;
        }
        StructureEffectMarker marker = tile.PlacedStructure.GetComponentInChildren<StructureEffectMarker>(true);
        if (marker == null) return;

        switch (marker.Type)
        {
            case StructureType.Torch:
                hasTorch = true;
                break;
            case StructureType.Campfire:
                hasCampfire = true;
                break;
            case StructureType.SweatingStone:
                hasSweatingStone = true;
                break;
        }
    }
}
