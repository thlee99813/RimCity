using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{


    public static CameraManager Instance { get; private set; }

    [SerializeField] private CinemachineImpulseSource source;

    [SerializeField] private float repeatInterval = 0.12f;

    [SerializeField] public CinemachineCamera[] cameras;

    private Coroutine impulseBurstRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {

    }

    private void DoImpulse()
    {
        source.GenerateImpulse();
    }

    public void PlayImpulseBurst(int repeatCount = 10)
    {

        if (impulseBurstRoutine != null)
        {
            StopCoroutine(impulseBurstRoutine);
        }

        impulseBurstRoutine = StartCoroutine(ImpulseRoutine(repeatCount));
    }

    private IEnumerator ImpulseRoutine(int repeatCount)
    {
        int count = Mathf.Max(1, repeatCount);
        float interval = Mathf.Max(0f, repeatInterval);

        for (int i = 0; i < count; i++)
        {
            DoImpulse();

            if ((i < count - 1) && (interval > 0f))
            {
                yield return new WaitForSeconds(interval);
            }
        }

        impulseBurstRoutine = null;
    }
    public void ChangeStageCamera(int stageIndex)
    {
        int camIndex = stageIndex - 1;

        if (camIndex < 0 || camIndex >= cameras.Length)
        {
            Debug.LogWarning("카메라 오류");
            return;
        }

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = 0;
        }

        cameras[camIndex].Priority = 10;
    }
}
