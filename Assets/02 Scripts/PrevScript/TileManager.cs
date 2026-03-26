using UnityEngine;
using UnityEngine.InputSystem;

public class TileManager : MonoBehaviour
{
    public GameObject TargetTile;
    private Tile tile;
    public GameObject Laser;

    void Start()
    {

       
    }
    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Laser.GetComponent<Laser>().RefreshLaser();
            Laser.SetActive(true);        
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            tile = TargetTile.GetComponent<Tile>();
            tile.MoveDown();        
        }
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            tile = TargetTile.GetComponent<Tile>();
            tile.MoveUp();        
        }

    }

}
