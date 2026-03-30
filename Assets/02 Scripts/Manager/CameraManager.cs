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
    public int CurrentCameraIndex { get; private set; }

    [SerializeField] private CinemachineCamera _smallTurnCameraFollow;

    [SerializeField] private SmallTurnCameraFollow _smallTurnFollowController;

  


    void Start()
    {
        ActivateCamera(1);
    }

    void Update()
    {

    }

    public void ActivateCamera(int index)
    {
        CurrentCameraIndex = index;

        for (int i = 0; i < cams.Length; i++)
        {
            cams[i].Priority = (i == index) ? activePriority : inactivePriority;
        }
    }

    

    public void ActivateSmallTurnCamera()
    {
        _smallTurnCameraFollow.Priority = activePriority + 10;
    }
    public void DeactivateSmallTurnCamera()
    {
        _smallTurnCameraFollow.Priority = inactivePriority;
    }

    public void SetSmallTurnTarget(Transform target)
    {
        _smallTurnFollowController.SetTarget(target);
    }

    public void ClearSmallTurnTarget()
    {
        _smallTurnFollowController.ClearTarget();
    }
    }
