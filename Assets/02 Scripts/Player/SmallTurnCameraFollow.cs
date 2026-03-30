using UnityEngine;
using Unity.Cinemachine;

public class SmallTurnCameraFollow : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _smallTurnCam;

    public void SetTarget(Transform target)
    {
        _smallTurnCam.Follow = target;
    }

    
    public void ClearTarget()
    {
        _smallTurnCam.Follow = null;
    }
}
