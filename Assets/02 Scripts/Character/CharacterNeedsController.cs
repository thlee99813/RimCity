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

    public void Tick(CharacterStatus status, CharacterData data)
    {
        status.TickNeeds(_hungerDeltaPerTurn, _sleepDeltaPerTurn, _funDeltaPerTurn, data);
        if (status.Hunger <= 0f)
            status.AddHealth(_healthDeltaWhenStarving, data);
    }
}
