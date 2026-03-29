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


    [SerializeField] private ObjectType cubeType;





    

    void Update()
    {
        if(crystal != null)
        {
            crystal.transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);          
        }
    }

    
    

    public void ResetCube()
        {
            rootObject.SetActive(true);

        }

}
