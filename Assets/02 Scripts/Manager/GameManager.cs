using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    protected override void Init()
    {
        
    }
    
    public CharacterGenerator CharacterGenerator;
    public readonly List<StageContext> ActiveStages = new List<StageContext>();
    public PlayerResourceInventory PlayerInventory;


    public void StartFirstTurn()
    {
        TurnManager.Instance.StartTurnLoop();
    }

    public void StageActive(StageContext stageContext)
    {
        RegisterStage(stageContext);
        EventManager.Instance.PostNotification(MEventType.Stageactivated, this, new StageActivatedEventArgs(stageContext));
    }

    public void RegisterStage(StageContext stageContext)
    {
        if (!ActiveStages.Contains(stageContext))
            ActiveStages.Add(stageContext);

            
    }

    public void UnregisterStage(StageContext stageContext)
    {
        ActiveStages.Remove(stageContext);

    }
    public StageContext GetAnyActiveStage()
    {
        if (ActiveStages.Count == 0) return null;
        return ActiveStages[0];
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }


    
}
