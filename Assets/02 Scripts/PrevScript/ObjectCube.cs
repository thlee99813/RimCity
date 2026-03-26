using UnityEngine;

public enum ObjectType
    {
        Door,
        Destroy,
        BlockDown,
        BlockUp,

        Special,

        End
    }

public class ObjectCube : MonoBehaviour
{    
    
    

    [SerializeField] private float rotateSpeed = 75f;
    [SerializeField] private GameObject crystal;
    
    [SerializeField] private GameObject rootObject;

    [SerializeField] private GameObject doorObject;

    [SerializeField] private Tile moveTile;

    [SerializeField] private ObjectType cubeType;

    private bool opendoored = false;



    private bool istriggered = false;

    

    void Update()
    {
        if(crystal != null)
        {
            crystal.transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);          
        }
    }

    
    
    public void LaserTrigger()
    {
        switch (cubeType)
        {
            case ObjectType.Door:
                if(opendoored)
                {
                    break;
                }
                
                CameraManager.Instance.PlayImpulseBurst(7);
                doorObject.GetComponent<Tile>().MoveDown(1f);
                opendoored = true;

                break;

            case ObjectType.Destroy:
                rootObject.SetActive(false);
                break;

            case ObjectType.BlockDown:
                if(moveTile.moveRoutine != null)
                {
                    return;
                }

                if(!istriggered)
                {
                    istriggered = true;
                    moveTile.MoveDown();
                }
                else
                {
                    istriggered = false;
                    moveTile.MoveUp();                

                } 

                break;
            case ObjectType.BlockUp:
                if(moveTile.moveRoutine != null)
                {
                    return;
                }
                if(!istriggered)
                {
                    istriggered = true;
                    moveTile.MoveUp();
                }
                else
                {
                    istriggered = false;
                    moveTile.MoveDown();
                
                } 

                break;
            case ObjectType.Special:
                {
                if(moveTile.moveRoutine != null)
                {
                    return;
                }

                if(!istriggered)
                {
                    istriggered = true;
                    moveTile.MoveDown();
                }
                else
                {
                    istriggered = false;
                    moveTile.MoveUp();                

                }
                rootObject.SetActive(false); 

                break;
                }
            case ObjectType.End:
                {
                    StageManager.Instance.StartEnding();
                        break;                    
                }

            default : 

                break;
        }
    }
    public void ResetCube()
        {
            rootObject.SetActive(true);
            istriggered = false;

        }

}
