using UnityEngine;
using UnityEngine.InputSystem;

public class GameDebuggerInput : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            CameraManager.Instance.ActivateCamera(1);
            StageManager.Instance.SetStageActive(1, true);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            CameraManager.Instance.ActivateCamera(2);
            StageManager.Instance.SetStageActive(2, true);
            StageManager.Instance.SetStageActive(3, true);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            CameraManager.Instance.ActivateCamera(3);
            StageManager.Instance.SetStageActive(4, true);
            StageManager.Instance.SetStageActive(5, true);
            

        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            CameraManager.Instance.ActivateCamera(4);
            StageManager.Instance.SetStageActive(6, true);
            StageManager.Instance.SetStageActive(7, true);
            StageManager.Instance.SetStageActive(8, true);
            

        }
        else if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            //CameraManager.Instance.ActivateCamera(3);
            
            

        }

 

     
    }
}
