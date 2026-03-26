using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public class PlayerLaser : MonoBehaviour
{
    [SerializeField] private GameObject laser;
    private Coroutine moveLockRoutine;
    private float drawTime;
    
    void Awake()
    {
        drawTime = laser.GetComponent<Laser>().drawTime;
    }
    

    
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                laser.SetActive(true);
                //Player.Instance.LockPlayerMove(true);
                if (moveLockRoutine != null)
                {
                    StopCoroutine(moveLockRoutine);
                }

                moveLockRoutine = StartCoroutine(PlayerMoveLock(laser.GetComponent<Laser>().drawTime));            
            }
    }

    IEnumerator PlayerMoveLock(float delay)
    {
        Player.Instance.LockPlayerMove(true);
        Player.Instance.LockPlayerRotate(true);

        yield return new WaitForSeconds(delay + 1f);
        Player.Instance.LockPlayerMove(false);
        Player.Instance.LockPlayerRotate(false);

        moveLockRoutine = null;
    }

      
         
          
    
}
