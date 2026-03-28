using UnityEngine;
using Unity.Cinemachine;


public class CameraManager : Singleton<CameraManager>
{

    protected override void Init()
    {
        
    }

    public CinemachineCamera[] cams;
    public int activePriority = 20;
    public int inactivePriority = 0;
    void Start()
    {
        ActivateCamera(0);
    }

    void Update()
    {

    }

    public void ActivateCamera(int index)
    {
        for (int i = 0; i < cams.Length; i++)
            {
                cams[i].Priority = (i == index) ? activePriority : inactivePriority;

            }
    }
}
