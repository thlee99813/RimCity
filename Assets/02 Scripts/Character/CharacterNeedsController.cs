public class CharacterNeedsController
{
    private readonly float _hungerDeltaPerTurn;
    private readonly float _sleepDeltaPerTurn;
    private readonly float _funDeltaPerTurn;
    private readonly float _healthDeltaWhenStarving;

    public CharacterNeedsController(float hungerDeltaPerTurn, float sleepDeltaPerTurn, float funDeltaPerTurn, float healthDeltaWhenStarving)
    {
        _hungerDeltaPerTurn = hungerDeltaPerTurn;
        _sleepDeltaPerTurn = sleepDeltaPerTurn;
        _funDeltaPerTurn = funDeltaPerTurn;
        _healthDeltaWhenStarving = healthDeltaWhenStarving;
    }  

    public void Tick(CharacterStatus status, CharacterData data, CharacterEquipment equipment, WeatherType weather)
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
        if (weatherHealthDelta < 0f)
        {
            if (weather == WeatherType.Hot || weather == WeatherType.Heatwave || weather == WeatherType.Drought)
                weatherHealthDelta *= equipment.GetHeatDamageMultiplier();

            if (weather == WeatherType.Cold || weather == WeatherType.ExtremeCold)
                weatherHealthDelta *= equipment.GetColdDamageMultiplier();
        }

        status.AddHealth(weatherHealthDelta, data);
        
        if (status.Hunger <= 0f)
            status.AddHealth(_healthDeltaWhenStarving, data);
            
    }
}
