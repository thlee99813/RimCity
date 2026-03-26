using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LobbyManager : MonoBehaviour
{

    [SerializeField] private string sceneName = "Main";
    private bool isTriggered = false;

    public void StartLobbyTrigger()
    {
        if (isTriggered) return;

        isTriggered = true;
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        StartGame();
    }



  
    public void StartGame()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    



 

}
