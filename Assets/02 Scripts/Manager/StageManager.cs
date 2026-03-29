using UnityEngine;

public class StageManager : Singleton<StageManager>
{

    private int _expandStep  = 0;

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
        if (_expandStep >= 4) return false;

        _expandStep++;
        ApplyExpandStep(_expandStep);
        return true;
    }
    private void ApplyExpandStep(int step)
    {
        switch (step)
        {
            case 1:
                CameraManager.Instance.ActivateCamera(1);
                SetStageActive(1, true);
                break;

            case 2:
                CameraManager.Instance.ActivateCamera(2);
                SetStageActive(2, true);
                SetStageActive(3, true);
                break;

            case 3:
                CameraManager.Instance.ActivateCamera(3);
                SetStageActive(4, true);
                SetStageActive(5, true);
                break;

            case 4:
                CameraManager.Instance.ActivateCamera(4);
                SetStageActive(6, true);
                SetStageActive(7, true);
                SetStageActive(8, true);
                break;
        }
    }




}
