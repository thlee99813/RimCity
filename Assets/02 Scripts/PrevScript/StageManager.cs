using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;


public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    

    public int currentStage = 1;

    public Transform[] respawnPoint;

    public Transform currentRespawnPoint;
    
    [SerializeField] private Transform[] stageRoots;

    [SerializeField] private Transform currentStageRoot;

    [SerializeField] private CinemachineCamera endingCam;
    [SerializeField] private float camMoveSpeed = 4f;
    [SerializeField] private float targetZ = -80f;
    [SerializeField] private CanvasGroup endingCanvas;
    [SerializeField] private float fadeSpeed = 0.1f;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    void Start()
    {
        currentRespawnPoint = respawnPoint[currentStage - 1];
        currentStageRoot = stageRoots[currentStage - 1];

    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetStage();
        }
   
    }

    public void NextStage()
    {
        currentStage++;
        currentRespawnPoint = respawnPoint[currentStage - 1];
        currentStageRoot = stageRoots[currentStage - 1];
        CameraManager.Instance.ChangeStageCamera(currentStage);
    }

    public void ResetStage()
    {

        Tile[] tiles = currentStageRoot.GetComponentsInChildren<Tile>(true);
        ObjectCube[] objectCubes = currentStageRoot.GetComponentsInChildren<ObjectCube>(true);
        

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].ResetTile();
        }
        for (int i = 0; i < objectCubes.Length; i++)
        {
            objectCubes[i].ResetCube();
        }
        Player.Instance.transform.position = currentRespawnPoint.position;
        Player.Instance.playerMove.rb.linearVelocity = Vector3.zero;
        Player.Instance.playerMove.rb.angularVelocity = Vector3.zero;

    }

    public void StartEnding()
    {
        endingCam = CameraManager.Instance.cameras[currentStage - 1];
        StartCoroutine(EndingRoutine());
    }
    IEnumerator EndingRoutine()
    {
        Player.Instance.LockPlayerMove(true);
        Player.Instance.LockPlayerRotate(true);


        Vector3 targetPos = endingCam.transform.position;
        targetPos.z = targetZ;

        endingCanvas.alpha = 0f;


        while (Mathf.Abs(endingCam.transform.position.z - targetZ) > 0.01f)
        {
            endingCam.transform.position = Vector3.MoveTowards(
                endingCam.transform.position,
                targetPos,
                camMoveSpeed * Time.deltaTime
            );
            endingCanvas.alpha = Mathf.MoveTowards(endingCanvas.alpha, 1f, fadeSpeed * Time.deltaTime);

            yield return null;
        }

        endingCam.transform.position = targetPos;
        endingCanvas.alpha = 1f;

    }
    
}
