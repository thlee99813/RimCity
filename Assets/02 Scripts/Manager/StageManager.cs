using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    protected override void Init()
    {
        
    }
    public GameObject[] Stages;
    
    protected override void Awake()
    {
        base.Awake();

        SetAllStages(false);
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

    




}
