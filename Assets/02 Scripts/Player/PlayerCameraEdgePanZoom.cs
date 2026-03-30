using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerCameraEdgePanZoom : MonoBehaviour
{
    [SerializeField] private float edgeSize = 20f;
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float zoomSpeed = 60f;
    [SerializeField] private float minFov = 25f;
    [SerializeField] private float maxFov = 70f;

    private void Update()
    {
        if (Mouse.current == null) return;
        if (CameraManager.Instance == null) return;

        int index = CameraManager.Instance.CurrentCameraIndex;
        if (index < 0 || index >= CameraManager.Instance.cams.Length) return;

        CinemachineCamera cam = CameraManager.Instance.cams[index];
        if (cam == null) return;

        HandleEdgePan(cam);
        HandleZoom(cam);
    }

    private void HandleEdgePan(CinemachineCamera cam)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        float x = 0f;
        float z = 0f;

        if (mousePos.x <= edgeSize) x = -1f;
        else if (mousePos.x >= Screen.width - edgeSize) x = 1f;

        if (mousePos.y <= edgeSize) z = -1f;
        else if (mousePos.y >= Screen.height - edgeSize) z = 1f;

        Vector3 right = cam.transform.right; right.y = 0f; right.Normalize();
        Vector3 forward = cam.transform.forward; forward.y = 0f; forward.Normalize();

        Vector3 move = (right * x + forward * z) * panSpeed * Time.deltaTime;
        cam.transform.position += move;
    }

    private void HandleZoom(CinemachineCamera cam)
    {
        float wheel = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(wheel) < 0.01f) return;

        var lens = cam.Lens;
        lens.FieldOfView = Mathf.Clamp(lens.FieldOfView - wheel * zoomSpeed * Time.deltaTime, minFov, maxFov);
        cam.Lens = lens;
    }
}
