using UnityEngine;

public class Player : MonoBehaviour
{

    public static Player Instance { get; private set; }
    
    public PlayerLaser playerLaser; 
    public PlayerMove playerMove;

    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        playerLaser = this.gameObject.GetComponent<PlayerLaser>();
        playerMove = this.gameObject.GetComponent<PlayerMove>();
    }
    public void LockPlayerMove(bool Lock)
    {
        playerMove.PlayerMoveLock = Lock;   
    }
    public void LockPlayerRotate(bool Lock)
    {
        playerMove.SetRotateLock(Lock);   
    }
}
