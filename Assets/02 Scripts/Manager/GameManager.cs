using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    protected override void Init()
    {
        
    }
    
    public CharacterGenerator CharacterGenerator;

    public void StartFirstTurn()
    {
        TurnManager.Instance.StartTurnLoop();
    }

    public void StageActive(StageContext stageContext)
    {
        EventManager.Instance.PostNotification( MEventType.Stageactivated, this, new StageActivatedEventArgs(stageContext));
    }

    
}
