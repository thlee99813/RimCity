using UnityEngine;

public class StageManager : Singleton<StageManager>
{

    private int _expandStep  = 0;
    private const int FixedPatternSteps = 4;   
    private const int DynamicStartIndex = 9;     
    private const int DynamicChunkSize = 9;   
    private const int DynamicLastIndex = 80;  
    private const int DynamicCameraIndex = 5;   

    protected override void Init()
    {
        
    }
    public GameObject[] Stages;
    
    protected override void Awake()
    {
        base.Awake();

        //SetAllStages(false);
    }
    void Start()
    {
        SetStageActive(0, true);

    }

    
    public void SetAllStages(bool active)
    {
        if (Stages == null) return;
        for (int i = 0; i < Stages.Length; i++)
            SetStageActive(i, active);
    }

    public void SetStageActive(int index, bool active)
    {
        if (Stages == null) return;
        if (index < 0 || index >= Stages.Length) return;
        if (Stages[index] == null) return;

        Stages[index].SetActive(active);
    }

    public bool ExpandOneStep()
    {
        int maxStep = GetMaxExpandStep();
        if (_expandStep >= maxStep) return false;
        _expandStep++;
        ApplyExpandStep(_expandStep);
        return true;
    }
    private void ApplyExpandStep(int step)
    {
        switch (step)
        {
            case 1:
                CameraManager.Instance.ActivateCamera(2);
                SetStageActive(1, true);
                break;

            case 2:
                CameraManager.Instance.ActivateCamera(3);
                SetStageActive(2, true);
                SetStageActive(3, true);
                break;

            case 3:
                CameraManager.Instance.ActivateCamera(4);
                SetStageActive(4, true);
                SetStageActive(5, true);
                break;

            case 4:
                CameraManager.Instance.ActivateCamera(5);
                SetStageActive(6, true);
                SetStageActive(7, true);
                SetStageActive(8, true);
                break;
            default:
                int chunkIndex = step - (FixedPatternSteps + 1); // step5 -> 0
                int start = DynamicStartIndex + chunkIndex * DynamicChunkSize;
                int end = Mathf.Min(start + DynamicChunkSize - 1, GetDynamicEndIndex());

                CameraManager.Instance.ActivateCamera(GetDynamicCameraIndex(step));
                ActivateRange(start, end);
                break;
            
        }
    }
    private int GetDynamicEndIndex()
    {
        if (Stages == null || Stages.Length == 0) return -1;
        return Mathf.Min(DynamicLastIndex, Stages.Length - 1);
    }

    private int GetMaxExpandStep()
    {
        int end = GetDynamicEndIndex();
        if (end < DynamicStartIndex) return FixedPatternSteps;

        int count = end - DynamicStartIndex + 1;
        int chunks = Mathf.CeilToInt(count / (float)DynamicChunkSize);
        return FixedPatternSteps + chunks;
    }

    private void ActivateRange(int start, int end)
    {
        if (end < start) return;

        for (int i = start; i <= end; i++)
            SetStageActive(i, true);
    }
    private int GetDynamicCameraIndex(int step)
    {
        if (step <= 7) return 6; 
        if (step <= 9) return 7; 
        return 8;             
    }






}
