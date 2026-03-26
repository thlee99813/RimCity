using UnityEngine;
using System.Collections;
public class NextStageTrigger : MonoBehaviour
{
    public GameObject startDoor;
    public GameObject limitDoor;
    private bool isTriggered = false;

    
    //[SerializeField] private Transform currentStageWalls;
    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if(other.CompareTag("Player"))
        {
            isTriggered = true;
            StageManager.Instance.NextStage();
            StartCoroutine(WallRoutuine(2f));

        }
    }
    IEnumerator WallRoutuine(float delay)
    {
        Player.Instance.LockPlayerMove(true);
        yield return new WaitForSeconds(delay);
        Tile startTile = startDoor.GetComponent<Tile>();
        Tile limitTile = limitDoor.GetComponent<Tile>();
        //Player.Instance.gameObject.transform.position = StageManager.Instance.currentRespawnPoint.position;
        startDoor.GetComponent<MeshRenderer>().enabled = true;
        startTile.MoveUp(1f);
        yield return new WaitUntil(() => startTile.moveRoutine == null);
        limitTile.MoveForward(3.24f);


        Player.Instance.LockPlayerMove(false);

        yield return new WaitUntil(() => limitTile.moveRoutine == null);


        gameObject.SetActive(false);
    }
}
