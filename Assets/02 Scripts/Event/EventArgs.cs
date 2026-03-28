using System;
using UnityEngine;

[Serializable]
public class StageActivatedEventArgs : EventArgs
{
    public StageContext StageContext;
    public StageActivatedEventArgs(StageContext stageContext)
    {
        StageContext = stageContext;
    }
}

