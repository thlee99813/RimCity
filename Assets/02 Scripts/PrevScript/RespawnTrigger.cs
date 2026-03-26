using UnityEngine;
using System.Collections;
public class RespawnTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
        StartCoroutine(RespawnRoutine());
        }
        
    }
    IEnumerator RespawnRoutine()
    {
            yield return new WaitForSeconds(0.2f);
            StageManager.Instance.ResetStage();

    }    

}
